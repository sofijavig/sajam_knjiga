using Core.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Core.DAO
{
    public class KupovinaDAO
    {
        private readonly string fajl;

        public KupovinaDAO()
        {
            fajl = Core.Utils.DataPaths.File("kupovine.txt");
        }

        public List<Kupovina> Ucitaj()
        {
            List<Kupovina> kupovine = new List<Kupovina>();

            if (!File.Exists(fajl))
                return kupovine;

            string[] linije = File.ReadAllLines(fajl);

            // 🔥 UČITAVAMO prave objekte
            PosetilacDAO posDAO = new PosetilacDAO();
            KnjigaDAO knjDAO = new KnjigaDAO();

            List<Posetilac> sviPosetioci = posDAO.Ucitaj();
            List<Knjiga> sveKnjige = knjDAO.Load();

            foreach (var linija in linije)
            {
                if (string.IsNullOrWhiteSpace(linija))
                    continue;

                string[] delovi = linija.Split('|');
                if (delovi.Length < 4)
                    continue;

                string ck = delovi[0];
                string isbn = delovi[1];

                Posetilac p = sviPosetioci.FirstOrDefault(x => x.CK == ck);
                Knjiga k = sveKnjige.FirstOrDefault(x => x.ISBN == isbn);

                if (p == null || k == null)
                    continue;

                Kupovina kup = new Kupovina
                {
                    Pos = p,
                    Knj = k,
                    DatKup = DateTime.Parse(delovi[2]),
                    Ocena = int.Parse(delovi[3]),
                    Komentar = delovi.Length > 4 ? delovi[4] : ""
                };

                kupovine.Add(kup);
            }

            return kupovine;
        }

        public void Sacuvaj(List<Kupovina> kupovine)
        {
            List<string> linije = new List<string>();

            foreach (var kup in kupovine)
            {
                string tekst =
                    kup.Pos.CK + "|" +
                    kup.Knj.ISBN + "|" +
                    kup.DatKup.ToString("yyyy-MM-dd") + "|" +
                    kup.Ocena + "|" +
                    kup.Komentar;

                linije.Add(tekst);
            }

            File.WriteAllLines(fajl, linije);
        }
    }
}