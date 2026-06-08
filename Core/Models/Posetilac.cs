using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public enum StatusPosetioca{R, V}
    public class Posetilac
    {
        public string Ime { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public DateTime DatRodj { get; set; }
        public Adresa Ad { get; set; } = new Adresa();
        public string BrojTelefona { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;
        public string CK { get; set; } = string.Empty;
        public int GodC {  get; set; }
        public StatusPosetioca Status { get; set; }
        public double ProsecnaOcena { get; set; }
        public List<Kupovina> Kupljene {  get; set; }= new List<Kupovina>();
        public List<Knjiga> ListaZelja { get; set; } =new List<Knjiga>();

        public void DodajKnjiguNaListuZelja(Knjiga k)
        {
            if (k == null) return;

            if (ListaZelja == null)
                ListaZelja = new List<Knjiga>();

            bool postoji = ListaZelja.Any(x => x != null && x.ISBN == k.ISBN);
            if (!postoji)
                ListaZelja.Add(k);
        }
        public double IzracunajProsek()
        {
            int suma = 0;
            int broj = Kupljene.Count;
            if(broj == 0){
                ProsecnaOcena = 0;
                return 0;
            }
            for(int i=0; i<broj; i++){
                suma=suma+Kupljene[i].Ocena;
            }
            ProsecnaOcena = (double)suma/broj;
            return ProsecnaOcena;
        }

        public override string ToString()
        {
            string tekst = $"{Ime} {Prezime} ({CK}), {DatRodj}, {Ad}, {BrojTelefona}, {Mail}, trenutna godina clanstva: {GodC}, prosecna ocena recenzija: {ProsecnaOcena:F2}\n";
            tekst=tekst+ "Lista želja:\n";

            if (ListaZelja == null || ListaZelja.Count == 0)
                tekst=tekst+ " — prazna —\n";
            else
                for (int i = 0; i < ListaZelja.Count; i++)
                    tekst=tekst+ $"{i + 1}. {ListaZelja[i].Naziv} ({ListaZelja[i].ISBN})\n";
            tekst=tekst+ "Lista kupovina:\n";

            if (Kupljene == null || Kupljene.Count == 0)
                tekst=tekst+ " — nema kupovina —\n";
            else
                for (int i = 0; i < Kupljene.Count; i++)
                    tekst=tekst+ $"{i + 1}. {Kupljene[i].Knj.Naziv}\n";

            return tekst;
        }
        public void Validacija()
        {
            if (CK.Length < 4) throw new ArgumentException("CK nije ispravnog formata.");

            if (string.IsNullOrWhiteSpace(Ime))
                throw new ArgumentException("Ime je obavezno.");

            if (string.IsNullOrWhiteSpace(Prezime))
                throw new ArgumentException("Prezime je obavezno.");

            if (string.IsNullOrWhiteSpace(CK))
                throw new ArgumentException("Broj članske karte (CK) je obavezan.");
            if (CK[0]!='C')
                throw new ArgumentException("Broj članske karte (CK) nije ispravnog formata");
            if (CK[1] != 'K')
                throw new ArgumentException("Broj članske karte (CK) nije ispravnog formata");
            if (CK[2] != '-')
                throw new ArgumentException("Broj članske karte (CK) nije ispravnog formata");
            string[] delovi =   CK.Split('-');
            if (delovi[0] != "CK")
            {
                throw new ArgumentException("Broj članske karte (CK) nije ispravnog formata");
            }
            if (!int.TryParse(delovi[1], out int broj) || broj < 0)
            {
                throw new ArgumentException("Broj članske karte (CK) nije ispravnog formata");
            }
            if (!int.TryParse(delovi[2], out int br) || br < 1900)

            {
                throw new ArgumentException("Broj članske karte (CK) nije ispravnog formata");
            }
           

            if (DatRodj > DateTime.Now)
                throw new ArgumentException("Datum rođenja ne može biti u budućnosti.");

            if (GodC < 0)
                throw new ArgumentException("Godina članstva ne može biti negativna.");

            if (string.IsNullOrWhiteSpace(BrojTelefona))
                throw new ArgumentException("Broj telefona je obavezan.");

            if (string.IsNullOrWhiteSpace(Mail) || !Mail.Contains("@") || !Mail.Contains("."))
                throw new ArgumentException("Mejl mora biti u ispravnom formatu.");

            if (!BrojTelefona.All(char.IsDigit))
                throw new ArgumentException("Broj telefona mora sadržati samo cifre.");
               
        }

    }

}
