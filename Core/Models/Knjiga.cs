using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Core.Models
{
    public class Knjiga
    {
        public string ISBN { get; set; } = string.Empty;
        public string Naziv { get; set; } = string.Empty;
        public string Zanr { get; set; } = string.Empty;
        public int GodinaIzdanja { get; set; }
        public int Cena { get; set; }
        public int BrojStrana { get; set; }
        public List<Autor> Autori { get; set; } = new();
        public string Izdavac { get; set; } = string.Empty;
        public List<Posetilac> Lista_Kupaca { get; set; } = new();
        public List<Posetilac> Lista_Zelja { get; set; } = new();

        public override string ToString()
        {
            return $"{Naziv} ({ISBN}) — {Izdavac} — {Zanr} — {GodinaIzdanja} — {Cena} RSD — {BrojStrana} str.";
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ISBN))
                throw new ArgumentException("ISBN mora postojati.", nameof(ISBN));

           
            var trimmed = ISBN.Trim();

            if (string.IsNullOrWhiteSpace(ISBN))
                throw new ArgumentException("ISBN mora postojati.", nameof(ISBN));

            if (!Regex.IsMatch(trimmed, @"^\d{3}-\d{3}$"))
                throw new ArgumentException("ISBN mora biti formata xxx-yyy (samo cifre).", nameof(ISBN));

            if (trimmed.Length < 4)
                throw new ArgumentException("ISBN je prekratak.", nameof(ISBN));

            if (string.IsNullOrWhiteSpace(Naziv))
                throw new ArgumentException("Naziv je obavezan.", nameof(Naziv));

            if (string.IsNullOrWhiteSpace(Izdavac))
                throw new ArgumentException("Izdavač je obavezan.", nameof(Izdavac));

            if (Cena <= 0)
                throw new ArgumentException("Cena mora biti veća od 0.", nameof(Cena));

            if (BrojStrana <= 0)
                throw new ArgumentException("Broj strana mora biti veći od 0.", nameof(BrojStrana));
        }
    }
}
