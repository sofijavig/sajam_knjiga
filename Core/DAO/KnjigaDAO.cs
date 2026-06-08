using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Models;

namespace Core.DAO
{
    public class KnjigaDAO
    {
        private readonly string fajl;

        public KnjigaDAO()
        {
            fajl = Core.Utils.DataPaths.File("knjige.txt");
        }

        // ISBN|Naziv|Zanr|GodinaIzdanja|Cena|BrojStrana|Izdavac|AutorLK
        public void Add(Knjiga knjiga)
        {
            var list = Load();
            list.Add(knjiga);
            Save(list);
        }

        public List<Knjiga> Load()
        {
            var list = new List<Knjiga>();
            if (!File.Exists(fajl))
                return list;

            foreach (var line in File.ReadAllLines(fajl))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var s = line.Split('|');

                if (s.Length != 7 && s.Length != 8) continue;

                if (!int.TryParse(s[3], out int godina)) continue;
                if (!int.TryParse(s[4], out int cena)) continue;
                if (!int.TryParse(s[5], out int brojStrana)) continue;

                var k = new Knjiga
                {
                    ISBN = s[0],
                    Naziv = s[1],
                    Zanr = s[2],
                    GodinaIzdanja = godina,
                    Cena = cena,
                    BrojStrana = brojStrana,
                    Izdavac = s[6],
                    Autori = new List<Autor>()
                };

                // 8. polje: CSV LK-ova
                if (s.Length == 8)
                {
                    var csv = (s[7] ?? "").Trim();
                    if (!string.IsNullOrWhiteSpace(csv))
                    {
                        var lks = csv.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var lkRaw in lks)
                        {
                            var lk = lkRaw.Trim();
                            if (lk == "") continue;
                            k.Autori.Add(new Autor { LK = lk });
                        }
                    }
                }

                list.Add(k);
            }

            return list;
        }

        public void Save(List<Knjiga> knjige)
        {
            var lines = new List<string>();

            foreach (var k in knjige)
            {
                string autoriCsv = "";
                if (k.Autori != null && k.Autori.Count > 0)
                {
                    autoriCsv = string.Join(",",
                        k.Autori
                         .Where(a => a != null && !string.IsNullOrWhiteSpace(a.LK))
                         .Select(a => a.LK.Trim()));
                }

                lines.Add($"{k.ISBN}|{k.Naziv}|{k.Zanr}|{k.GodinaIzdanja}|{k.Cena}|{k.BrojStrana}|{k.Izdavac}|{autoriCsv}");
            }

            File.WriteAllLines(fajl, lines);
        }

        public Knjiga? GetByISBN(string isbn)
        {
            return Load().FirstOrDefault(k => k.ISBN == isbn);
        }
    }
}
