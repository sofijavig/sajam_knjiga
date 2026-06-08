using Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core.DAO
{
    public class IzdavacDAO
    {
        private readonly string fajl;

        public IzdavacDAO()
        {
            fajl = Core.Utils.DataPaths.File("izdavaci.txt");
        }

        public List<Izdavac> Ucitaj()
        {
            var izdavaci = new List<Izdavac>();
            if (!File.Exists(fajl))
                return izdavaci;

            var linije = File.ReadAllLines(fajl);
            for (int i = 0; i < linije.Length; i++)
            {
                if (string.IsNullOrEmpty(linije[i]))
                    continue;

                var delovi = linije[i].Split('|');
                if (delovi.Length < 8) continue;

                var izd = new Izdavac
                {
                    Sifra = delovi[0],
                    Naziv = delovi[1],
                    Sef = new Autor
                    {
                        Ime = delovi[2],
                        Prezime = delovi[3],
                        LK = delovi[4],
                        God_Iskustva = int.Parse(delovi[5])
                    },
                    SpisakAutora = new List<Autor>(),
                    SpisakKnjiga = new List<Knjiga>()
                };

                foreach (var lk in delovi[6].Split(','))
                {
                    if (!string.IsNullOrWhiteSpace(lk))
                        izd.SpisakAutora.Add(new Autor { LK = lk.Trim() });
                }

                foreach (var isbn in delovi[7].Split(','))
                {
                    if (!string.IsNullOrWhiteSpace(isbn))
                        izd.SpisakKnjiga.Add(new Knjiga { ISBN = isbn.Trim() });
                }

                // NE VALIDIRAJ U DAO
                izdavaci.Add(izd);
            }

            return izdavaci;
        }

        public void Sacuvaj(List<Izdavac> izdavaci)
        {
            var linije = new List<string>();

            foreach (var izd in izdavaci)
            {
                var autori = string.Join(",", izd.SpisakAutora.Select(a => a.LK));
                var knjige = string.Join(",", izd.SpisakKnjiga.Select(k => k.ISBN));

                var tekst = izd.Sifra + '|' + izd.Naziv + '|' + izd.Sef.Ime + '|' + izd.Sef.Prezime + '|' +
                            izd.Sef.LK + '|' + izd.Sef.God_Iskustva + '|' + autori + '|' + knjige;

                linije.Add(tekst);
            }

            File.WriteAllLines(fajl, linije);
        }
    }
}