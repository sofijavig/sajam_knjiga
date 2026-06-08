using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using Core.DAO;

namespace Core.Services
{
    public class PosetilacServices
    {
        private PosetilacDAO dao=new PosetilacDAO();
        public void Dodaj(Posetilac posetilac)
        {
            posetilac.Validacija();
            List<Posetilac> posetioci = dao.Ucitaj();
            for (int i = 0; i < posetioci.Count; i++)
            {
                if (posetioci[i].CK == posetilac.CK)
                    throw new ArgumentException("Posetilac sa tim CK već postoji.");
            }
            posetioci.Add(posetilac);
            dao.Sacuvaj(posetioci);
        }

        public void Izmeni(string ckIzmeni, Posetilac izmenjen) 
        {
            List<Posetilac> posetioci= dao.Ucitaj();
            bool zamenjeno= false;
            for (int i = 0; i < posetioci.Count; i++)
            {
                if (posetioci[i].CK==ckIzmeni)
                {
                    izmenjen.CK = ckIzmeni;
                    izmenjen.Validacija();
                    posetioci[i] = izmenjen;
                    zamenjeno = true;
                    break;
                }
            }
            if (zamenjeno == false)
                throw new ArgumentException("Ne postoji posetilac sa ovim brojem CK");
            dao.Sacuvaj(posetioci);
        }

        public void Obrisi(string ckObrisi)
        {
            List<Posetilac> posetioci= dao.Ucitaj();
            bool obrisan= false;
            for (int i = 0; i < posetioci.Count; i++)
            {
                if (posetioci[i].CK == ckObrisi)
                {
                    posetioci.RemoveAt(i);
                    obrisan = true;
                    break;
                }
            }
            if (obrisan == false)
                throw new ArgumentException("Ne postoji posetilac sa ovim brojem CK");
            dao.Sacuvaj(posetioci);
        }

        public List<Posetilac> VratiSve()
        {
            return dao.Ucitaj();
        }

        private static string[] ParsirajDelove(string unos, int maxDelova)
        {
            if (string.IsNullOrWhiteSpace(unos))
                return Array.Empty<string>();

            return unos.Split(',')
                       .Select(x => (x ?? "").Trim())
                       .Where(x => !string.IsNullOrWhiteSpace(x))
                       .Take(maxDelova)
                       .ToArray();
        }

        private static bool SadrziBezObziraNaVelicinu(string? izvor, string deo)
        {
            izvor ??= "";
            return izvor.Contains(deo, StringComparison.OrdinalIgnoreCase);
        }

        public List<Posetilac> Pretrazi(string unos)
        {
            List<Posetilac> svi = dao.Ucitaj();
            string[] delovi = ParsirajDelove(unos, 3);

            if (delovi.Length == 0)
                return svi;

            // 1 deo → prezime
            if (delovi.Length == 1)
            {
                string prezime = delovi[0];
                return svi
                    .Where(p => SadrziBezObziraNaVelicinu(p.Prezime, prezime))
                    .ToList();
            }

            // 2 dela → prezime + ime
            if (delovi.Length == 2)
            {
                string prezime = delovi[0];
                string ime = delovi[1];

                return svi
                    .Where(p =>
                        SadrziBezObziraNaVelicinu(p.Prezime, prezime) &&
                        SadrziBezObziraNaVelicinu(p.Ime, ime))
                    .ToList();
            }

            // 3 dela → CK + ime + prezime
            string ck = delovi[0];
            string ime3 = delovi[1];
            string prezime3 = delovi[2];

            return svi
                .Where(p =>
                    SadrziBezObziraNaVelicinu(p.CK, ck) &&
                    SadrziBezObziraNaVelicinu(p.Ime, ime3) &&
                    SadrziBezObziraNaVelicinu(p.Prezime, prezime3))
                .ToList();
        }

        public void Prikazi()
        {
            List<Posetilac> posetioci = dao.Ucitaj();
            if (posetioci.Count == 0)
            {
                Console.WriteLine("Nema posetilaca");
                return;
            }
            for (int i = 0; i < posetioci.Count; i++)
            {
                Console.WriteLine((i + 1) + ". " + posetioci[i].ToString());
                Console.WriteLine("--------------------------------------");
            }
        }

        public List<Posetilac> PosetiociSaObeKnjigeNaListiZelja(Knjiga prva, Knjiga druga)
        {
            var svi = dao.Ucitaj();

            return svi
                .Where(p =>
                    p.ListaZelja.Any(k => k.ISBN == prva.ISBN) &&
                    p.ListaZelja.Any(k => k.ISBN == druga.ISBN))
                .ToList();
        }

        public List<Posetilac> PosetiociKupiliPrvuNeDrugu(Knjiga prva, Knjiga druga)
        {
            KupovinaDAO kupDao = new KupovinaDAO();
            var kupovine = kupDao.Ucitaj();
            var svi = dao.Ucitaj();

            return svi
                .Where(p =>
                    kupovine.Any(k => k.Pos.CK == p.CK && k.Knj.ISBN == prva.ISBN) &&
                    !kupovine.Any(k => k.Pos.CK == p.CK && k.Knj.ISBN == druga.ISBN))
                .ToList();
        }

        public List<Posetilac> FiltrirajRezultate(List<Posetilac> lista, string tekst)
        {
            if (string.IsNullOrWhiteSpace(tekst))
                return lista;

            tekst = tekst.ToLower();

            return lista.Where(p =>
                p.CK.ToLower().Contains(tekst) ||
                p.Ime.ToLower().Contains(tekst) ||
                p.Prezime.ToLower().Contains(tekst) ||
                p.Status.ToString().ToLower().Contains(tekst)
            ).ToList();
        }
    }
}
