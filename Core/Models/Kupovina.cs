using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Kupovina
    {
        public Posetilac Pos { get; set; }
        public Knjiga Knj { get; set; } 
        public DateTime DatKup { get; set; }
        public int Ocena { get; set; }
        public string Komentar { get; set; }=string.Empty;
        public void Validacija()
        {
            if (Pos == null)
                throw new ArgumentException("Kupovina mora imati posetioca.");

            if (Knj == null)
                throw new ArgumentException("Kupovina mora imati knjigu.");

            if (DatKup > DateTime.Now)
                throw new ArgumentException("Datum kupovine ne može biti u budućnosti.");

            if (Ocena < 1 || Ocena > 7)
                throw new ArgumentException("Ocena mora biti između 1 i 7.");
        }
        public override string ToString()
        {
            string tekst = $"{Pos.Ime} {Pos.Prezime}, {Knj.Naziv}, datum kupovine: {DatKup}, ocena: {Ocena}";

            if (!string.IsNullOrWhiteSpace(Komentar))
            {
                tekst=tekst+ $", komentar: {Komentar}";
            }

            return tekst;
        }

    }
}
