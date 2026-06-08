using Core.DAO;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class IzdavacServices
    {
        private IzdavacDAO dao = new IzdavacDAO();
        public void Dodaj(Izdavac izdavac)
        {
            AutorDAO autDao = new AutorDAO();
            List<Autor> autori = autDao.Load();
            Autor a = null;
            for (int i = 0; i < autori.Count; i++)
            {
                if (autori[i].LK == izdavac.Sef.LK)
                {
                    a = autori[i];
                    break;
                }
            }
            if (a == null)
                throw new ArgumentException("Ne postoji autor sa unetom LK.");
            a.Validate();
            izdavac.Validacija();
            List<Izdavac> izdavaci = dao.Ucitaj();
            for (int i = 0; i < izdavaci.Count; i++)
            {
                if (izdavaci[i].Sifra == izdavac.Sifra)
                    throw new ArgumentException("Izdavac sa ovom sifrom već postoji.");
            }
            izdavaci.Add(izdavac);
            dao.Sacuvaj(izdavaci);
        }

        public void Izmeni(string sifraIzmeni, Izdavac izmenjen)
        {
            List<Izdavac> izdavaci = dao.Ucitaj();
            bool zamenjeno = false;
            for (int i = 0; i < izdavaci.Count; i++)
            {
                if (izdavaci[i].Sifra == sifraIzmeni)
                {
                    izmenjen.Validacija();
                    izdavaci[i] = izmenjen;
                    zamenjeno = true;
                    izmenjen.Sifra = sifraIzmeni;
                    break;

                }
            }
            if (zamenjeno == false)
                throw new ArgumentException("Ne postoji izdavac sa ovom sifrom");
            dao.Sacuvaj(izdavaci);
        }

        public void Obrisi(string sifraObrisi)
        {
            List<Izdavac> izdavaci = dao.Ucitaj();
            bool obrisan = false;
            for (int i = 0; i < izdavaci.Count; i++)
            {
                if (izdavaci[i].Sifra == sifraObrisi)
                {
                    izdavaci.RemoveAt(i);
                    obrisan = true;
                    break;
                }
            }
            if (obrisan == false)
                throw new ArgumentException("Ne postoji izdavac sa ovom sifrom");
            dao.Sacuvaj(izdavaci);
        }

        public List<Izdavac> VratiSve()
        {
            SyncSveIzKnjiga();
            return dao.Ucitaj();
        }

        private void EnsureIzdavaciSeededFromData()
        {
            KnjigaDAO knjigaDao = new KnjigaDAO();
            var sveKnjige = knjigaDao.Load() ?? new List<Knjiga>();
            var postojeciIzdavaci = dao.Ucitaj() ?? new List<Izdavac>();

            bool imaPromena = false;
            var naziviIzKnjiga = sveKnjige
                .Where(k => !string.IsNullOrWhiteSpace(k.Izdavac))
                .Select(k => k.Izdavac.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var naziv in naziviIzKnjiga)
            {
                if (!postojeciIzdavaci.Any(i => i.Naziv.Equals(naziv, StringComparison.OrdinalIgnoreCase)))
                {
                    var novi = new Izdavac
                    {
                        Sifra = "IZD-" + Guid.NewGuid().ToString().Substring(0, 4).ToUpper(),
                        Naziv = naziv,
                        Sef = new Autor
                        {
                            Ime = "Sistem",
                            Prezime = "Generisano",
                            LK = "000000",
                            God_Iskustva = 5 // Da prođe validaciju
                        },
                        SpisakAutora = new List<Autor>(),
                        SpisakKnjiga = new List<Knjiga>()
                    };

                    postojeciIzdavaci.Add(novi);
                    imaPromena = true;
                }
            }

            if (imaPromena)
            {
                dao.Sacuvaj(postojeciIzdavaci);
            }
        }

        public void Prikazi()
        {
            List<Izdavac> izdavaci = dao.Ucitaj();
            if (izdavaci.Count == 0)
            {
                Console.WriteLine("Nema izdavaca");
                return;
            }
            for (int i = 0; i < izdavaci.Count; i++)
            {
                Console.WriteLine((i + 1) + ". " + izdavaci[i].ToString());
                Console.WriteLine("--------------------------------------");
            }
        }
        public void PostaviSefa(string sifraIzdavaca, Autor izabraniAutor)
        {
            if (izabraniAutor == null)
                throw new ArgumentException("Nije izabran autor.");

            List<Izdavac> sviIzdavaci = dao.Ucitaj();
            Izdavac izdavac = sviIzdavaci.FirstOrDefault(i => i.Sifra == sifraIzdavaca);
            if (izdavac == null)
                throw new ArgumentException("Ne postoji izdavač sa datom šifrom.");

            AutorDAO autDao = new AutorDAO();
            List<Autor> sviAutori = autDao.Load();

            Autor puniAutor = sviAutori.FirstOrDefault(a => a.LK == izabraniAutor.LK);
            if (puniAutor == null)
                throw new ArgumentException("Ne postoji autor sa izabranom LK.");

            if (puniAutor.God_Iskustva < 5)
                throw new ArgumentException("Šef izdavača mora imati najmanje 5 godina iskustva.");

            izdavac.Sef = puniAutor;

            izdavac.Validacija();

            dao.Sacuvaj(sviIzdavaci);
        }
        public void SyncAutoriSaIzdavacima()
        {
            KnjigaDAO knjigaDao = new KnjigaDAO();
            var sveKnjige = knjigaDao.Load() ?? new List<Knjiga>();
            var sviIzdavaci = dao.Ucitaj() ?? new List<Izdavac>();

            bool promena = false;

            foreach (var izdavac in sviIzdavaci)
            {
                var knjigeOvogIzdavaca = sveKnjige
                    .Where(k => k.Izdavac != null && k.Izdavac.Trim().Equals(izdavac.Naziv, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var aktivniAutoriLK = knjigeOvogIzdavaca
                    .SelectMany(k => k.Autori ?? new List<Autor>())
                    .Select(a => a.LK.Trim())
                    .Distinct()
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                int brojPre = izdavac.SpisakAutora.Count;
                izdavac.SpisakAutora = izdavac.SpisakAutora
                    .Where(a => aktivniAutoriLK.Contains(a.LK.Trim()))
                    .ToList();

                if (izdavac.SpisakAutora.Count != brojPre)
                    promena = true;
            }

            if (promena)
            {
                dao.Sacuvaj(sviIzdavaci);
            }
        }
        public void SyncSveIzKnjiga()
        {
            // 1) osiguraj da postoje svi izdavači koji se pojavljuju u knjigama
            EnsureIzdavaciSeededFromData();

            var knjigaDao = new KnjigaDAO();
            var sveKnjige = knjigaDao.Load() ?? new List<Knjiga>();

            var sviIzdavaci = dao.Ucitaj() ?? new List<Izdavac>();
            bool promena = false;

            foreach (var izd in sviIzdavaci)
            {
                var naziv = (izd.Naziv ?? "").Trim();
                if (naziv == "") continue;

                // sve knjige ovog izdavača
                var knjigeOvog = sveKnjige
                    .Where(k => k != null && !string.IsNullOrWhiteSpace(k.Izdavac) &&
                                k.Izdavac.Trim().Equals(naziv, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // ISBN lista
                var isbns = knjigeOvog
                    .Where(k => !string.IsNullOrWhiteSpace(k.ISBN))
                    .Select(k => k.ISBN.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // LK autora iz tih knjiga
                var lks = knjigeOvog
                    .SelectMany(k => k.Autori ?? new List<Autor>())
                    .Where(a => a != null && !string.IsNullOrWhiteSpace(a.LK))
                    .Select(a => a.LK.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // uporedi i setuj
                string oldIsbns = string.Join(",", (izd.SpisakKnjiga ?? new List<Knjiga>()).Select(x => (x.ISBN ?? "").Trim()));
                string newIsbns = string.Join(",", isbns);

                string oldLks = string.Join(",", (izd.SpisakAutora ?? new List<Autor>()).Select(x => (x.LK ?? "").Trim()));
                string newLks = string.Join(",", lks);

                if (!string.Equals(oldIsbns, newIsbns, StringComparison.OrdinalIgnoreCase))
                {
                    izd.SpisakKnjiga = isbns.Select(x => new Knjiga { ISBN = x }).ToList();
                    promena = true;
                }

                if (!string.Equals(oldLks, newLks, StringComparison.OrdinalIgnoreCase))
                {
                    izd.SpisakAutora = lks.Select(x => new Autor { LK = x }).ToList();
                    promena = true;
                }
            }

            if (promena)
                dao.Sacuvaj(sviIzdavaci);
        }
    }
}
