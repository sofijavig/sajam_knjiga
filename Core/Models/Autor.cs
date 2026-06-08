using System;
using System.Collections.Generic;

namespace Core.Models
{
    public class Autor
    {
        public string Ime { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public DateTime DatRodj { get; set; }
        public Adresa Ad { get; set; } = new Adresa();
        public string BrojTelefona { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;
        public string LK { get; set; } = string.Empty;
        public int God_Iskustva { get; set; }
        public List<Knjiga> Knjige { get; set; } = new List<Knjiga>();

        public override string ToString()
        {
            return $"{Ime} {Prezime} ({LK}), Knjige: {string.Join(", ", Knjige)}, {DatRodj}, {Ad}, {BrojTelefona}, {Mail}";
        }
        public string DisplayName => $"{Ime} {Prezime}";

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Ime))
                throw new ArgumentException("Unesite ime autora.");

            if (string.IsNullOrWhiteSpace(Prezime))
                throw new ArgumentException("Unesite prezime autora.");

            if (string.IsNullOrWhiteSpace(LK))
                throw new ArgumentException("Lična karta (LK) je obavezna.");

            if (!LK.All(char.IsDigit))
                throw new ArgumentException("Lična karta mora sadržati samo cifre.");

            if (LK.Length != 8)
                throw new ArgumentException("Lična karta mora imati tačno 8 cifara.");

            if (Ad == null)
                throw new ArgumentException("Adresa je obavezna.");

            if (string.IsNullOrWhiteSpace(BrojTelefona))
                throw new ArgumentException("Broj telefona je obavezan.");

            if (DatRodj > DateTime.Now)
                throw new ArgumentException("Datum rođenja ne može biti u budućnosti.");

            if (string.IsNullOrWhiteSpace(Mail) || !Mail.Contains("@") || !Mail.Contains("."))
                throw new ArgumentException("Mejl mora biti u ispravnom formatu.");

            if (God_Iskustva < 0)
                throw new ArgumentException("Godine iskustva ne mogu biti negativne.");

            if (!BrojTelefona.All(char.IsDigit))
                throw new ArgumentException("Broj telefona mora sadržati samo cifre.");

            if (BrojTelefona.Length != 10)
                throw new ArgumentException("Broj telefona mora imati tačno 10 cifara.");
        }
    }
}
