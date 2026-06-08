using System;
using System.Collections.Generic;
using System.IO;
using Core.Models;

namespace Core.DAO
{
    public class AdreseDAO
    {
        private readonly string fajl;

        public AdreseDAO()
        {
            fajl = Core.Utils.DataPaths.File("adrese.txt");
        }

        public void Add(Adresa adresa) //metoda za dodavanje adrese
        {
            var list = Load();
            list.Add(adresa);
            Save(list);
        }

        public List<Adresa> Load()
        {
            var list = new List<Adresa>();
            if (!File.Exists(fajl)) //provera da li postoji fajl
                return list;

            foreach (var line in File.ReadAllLines(fajl)) //pravimo kako cemo svaku liniju u fajlu
            {
                var s = line.Split('|'); //odvojili element | da bi se i lakse parsiralo
                list.Add(new Adresa
                {
                    Ulica = s[0],
                    Broj = int.Parse(s[1]),
                    Grad = s[2],
                    Drzava = s[3]
                });
            }
            return list;
        }

        public void Save(List<Adresa> adrese)
        {
            var lines = new List<string>();
            foreach (var a in adrese)
                lines.Add($"{a.Ulica}|{a.Broj}|{a.Grad}|{a.Drzava}");

            File.WriteAllLines(fajl, lines);
        }
    }
}