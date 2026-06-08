using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Models;

namespace Core.DAO
{
    public class AutorDAO
    {
        private readonly string fajl;

        public AutorDAO()
        {
            fajl = Core.Utils.DataPaths.File("autori.txt");
        }
        private readonly AdreseDAO adresaDAO = new();
        private readonly KnjigaDAO knjigaDAO = new();

        public void Add(Autor autor)
        {
            if (autor == null) throw new ArgumentNullException(nameof(autor));

            autor.Validate();

            var list = Load();
            list.Add(autor);
            Save(list);

            // Minimalno: čuvamo i adresu + knjige (kao što si htela)
            adresaDAO.Add(autor.Ad);

            foreach (var knjiga in autor.Knjige)
            {
                // da ne duplira knjige ako već postoji u fajlu
                if (knjigaDAO.GetByISBN(knjiga.ISBN) == null)
                    knjigaDAO.Add(knjiga);
            }
        }

        public List<Autor> Load()
        {
            var autori = new List<Autor>();

            if (!File.Exists(fajl))
                return autori;

            foreach (var line in File.ReadAllLines(fajl))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Format:
                // LK|Ime|Prezime|DatRodj|BrojTelefona|Mail|God_Iskustva|Ulica|Broj|Grad|Drzava|knjigeTxt
                // knjigeTxt: ISBN:Naziv;ISBN2:Naziv2;...
                var s = line.Split('|');
                if (s.Length < 11) continue; // nema ni adresu komplet

                if (!DateTime.TryParse(s[3], out var datRodj)) continue;
                if (!int.TryParse(s[6], out var godIsk)) continue;
                if (!int.TryParse(s[8], out var brojUlice)) continue;

                var autor = new Autor
                {
                    LK = s[0],
                    Ime = s[1],
                    Prezime = s[2],
                    DatRodj = datRodj,
                    BrojTelefona = s[4],
                    Mail = s[5],
                    God_Iskustva = godIsk,
                    Ad = new Adresa
                    {
                        Ulica = s[7],
                        Broj = brojUlice,
                        Grad = s[9],
                        Drzava = s[10]
                    },
                    Knjige = new List<Knjiga>()
                };

                // Knjige su u s[11] ako postoje
                if (s.Length >= 12 && !string.IsNullOrWhiteSpace(s[11]))
                {
                    var listaTokena = s[11].Split(';', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var token in listaTokena)
                    {
                        var deo = token.Split(':');
                        if (deo.Length < 1) continue;

                        var isbn = deo[0].Trim();
                        if (string.IsNullOrWhiteSpace(isbn)) continue;

                        var knjiga = knjigaDAO.GetByISBN(isbn);
                        if (knjiga != null && !autor.Knjige.Any(k => k.ISBN == knjiga.ISBN))
                            autor.Knjige.Add(knjiga);
                    }
                }

                autori.Add(autor);
            }

            return autori;
        }

        public void Save(List<Autor> autori)
        {
            var lines = new List<string>();

            foreach (var a in autori)
            {
                // KNJIGE: koristi ; da ne razbije ceo red
                string knjigeTxt = string.Join(";", a.Knjige.Select(k => $"{k.ISBN}:{k.Naziv}"));

                string line =
                    $"{a.LK}|{a.Ime}|{a.Prezime}|{a.DatRodj}|{a.BrojTelefona}|{a.Mail}|{a.God_Iskustva}|" +
                    $"{a.Ad.Ulica}|{a.Ad.Broj}|{a.Ad.Grad}|{a.Ad.Drzava}|{knjigeTxt}";

                lines.Add(line);
            }

            File.WriteAllLines(fajl, lines);
        }
    }
}
