using System;
using System.Collections.Generic;
using System.Linq;
using Core.DAO;
using Core.Models;

namespace Core.Services
{
    public class KupovinaServices
    {
        private readonly KupovinaDAO dao = new KupovinaDAO();

        public void Dodaj(Kupovina kupovina)
        {
            // Učitaj posetioce i pronađi pravog
            PosetilacDAO posDao = new PosetilacDAO();
            List<Posetilac> posetioci = posDao.Ucitaj();

            Posetilac p = posetioci.FirstOrDefault(x => x.CK == kupovina.Pos.CK);
            if (p == null)
                throw new ArgumentException("Ne postoji posetilac sa unetim CK.");
            p.Validacija();

            // Učitaj knjige i pronađi pravu
            KnjigaDAO knjDao = new KnjigaDAO();
            List<Knjiga> knjige = knjDao.Load();

            Knjiga k = knjige.FirstOrDefault(x => x.ISBN == kupovina.Knj.ISBN);
            if (k == null)
                throw new ArgumentException("Ne postoji knjiga sa unetim ISBN.");
            k.Validate();

            // Veži prave reference
            kupovina.Pos = p;
            kupovina.Knj = k;

            // Validacija kupovine
            kupovina.Validacija();

            // Provera duplikata u fajlu kupovina
            List<Kupovina> kupovine = dao.Ucitaj();
            bool postoji = kupovine.Any(x =>
                x.Pos.CK == kupovina.Pos.CK &&
                x.Knj.ISBN == kupovina.Knj.ISBN &&
                x.DatKup == kupovina.DatKup);

            if (postoji)
                throw new ArgumentException("Ova kupovina već postoji.");

            // Dodaj u listu kupovina i snimi kupovine.txt
            kupovine.Add(kupovina);
            dao.Sacuvaj(kupovine);

            // Ažuriraj posetioca (prosek) i snimi posetioci.txt
            if (p.Kupljene == null) p.Kupljene = new List<Kupovina>();
            p.Kupljene.Add(kupovina);
            p.IzracunajProsek();
            posDao.Sacuvaj(posetioci);
        }

        public void Izmeni(string ckIzmeni, string isbnIzmeni, DateTime datumIzmeni, Kupovina izmenjena)
        {
            List<Kupovina> kupovine = dao.Ucitaj();

            PosetilacDAO posDao = new PosetilacDAO();
            List<Posetilac> posetioci = posDao.Ucitaj();
            Posetilac p = posetioci.FirstOrDefault(x => x.CK == ckIzmeni);
            if (p == null)
                throw new ArgumentException("Ne postoji posetilac sa unetim CK.");

            KnjigaDAO knjDao = new KnjigaDAO();
            List<Knjiga> knjige = knjDao.Load();
            Knjiga k = knjige.FirstOrDefault(x => x.ISBN == isbnIzmeni);
            if (k == null)
                throw new ArgumentException("Ne postoji knjiga sa unetim ISBN.");

            izmenjena.Pos = p;
            izmenjena.Knj = k;

            izmenjena.Validacija();

            bool zamenjeno = false;
            for (int i = 0; i < kupovine.Count; i++)
            {
                if (kupovine[i].Pos.CK == ckIzmeni &&
                    kupovine[i].Knj.ISBN == isbnIzmeni &&
                    kupovine[i].DatKup == datumIzmeni)
                {
                    kupovine[i] = izmenjena;
                    zamenjeno = true;
                    break;
                }
            }

            if (!zamenjeno)
                throw new ArgumentException("Ne postoji ova kupovina");

            dao.Sacuvaj(kupovine);


            p.Kupljene = kupovine.Where(x => x.Pos.CK == p.CK).ToList();
            p.IzracunajProsek();
            posDao.Sacuvaj(posetioci);
        }

        public void Obrisi(string ckObrisi, string isbnObrisi, DateTime datumObrisi)
        {
            List<Kupovina> kupovine = dao.Ucitaj();

            bool obrisana = false;
            for (int i = 0; i < kupovine.Count; i++)
            {
                if (kupovine[i].Pos.CK == ckObrisi &&
                    kupovine[i].Knj.ISBN == isbnObrisi &&
                    kupovine[i].DatKup == datumObrisi)
                {
                    kupovine.RemoveAt(i);
                    obrisana = true;
                    break;
                }
            }

            if (!obrisana)
                throw new ArgumentException("Ne postoji ova kupovina");

            dao.Sacuvaj(kupovine);

            PosetilacDAO posDAO = new PosetilacDAO();
            List<Posetilac> posetioci = posDAO.Ucitaj();

            Posetilac p = posetioci.FirstOrDefault(x => x.CK == ckObrisi);
            if (p != null)
            {
                p.Kupljene = kupovine.Where(x => x.Pos.CK == p.CK).ToList();
                p.IzracunajProsek();
                posDAO.Sacuvaj(posetioci);
            }
        }

        public List<Kupovina> VratiSve()
        {
            return dao.Ucitaj();
        }

        public void Prikazi()
        {
            List<Kupovina> kupovine = dao.Ucitaj();
            if (kupovine.Count == 0)
            {
                Console.WriteLine("Nema kupovina");
                return;
            }

            for (int i = 0; i < kupovine.Count; i++)
            {
                Console.WriteLine((i + 1) + ". " + kupovine[i].ToString());
                Console.WriteLine("--------------------------------------");
            }
        }
    }
}