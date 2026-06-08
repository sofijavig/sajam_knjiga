using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Izdavac
    {
        public string Sifra { get; set; } = string.Empty;
        public string Naziv { get; set; }=string.Empty;
        public Autor Sef {  get; set; }=new Autor();
        public List<Autor> SpisakAutora { get; set; }=new List<Autor>();
        public List<Knjiga> SpisakKnjiga { get; set; } =new List<Knjiga>();
        public override string ToString()
        {
            string spisakA = "";
            if (SpisakAutora == null || SpisakAutora.Count == 0)
                spisakA = "nema autora";
            else
                for (int i = 0; i < SpisakAutora.Count; i++)
                {
                    spisakA=spisakA+ SpisakAutora[i].Ime + " " + SpisakAutora[i].Prezime;
                    if (i < SpisakAutora.Count - 1)
                        spisakA=spisakA+ ", ";
                }
            string spisakK = "";
            if (SpisakKnjiga == null || SpisakKnjiga.Count == 0)
                spisakK = "nema knjiga";
            else
                for (int i = 0; i < SpisakKnjiga.Count; i++)
                {
                    spisakK=spisakK+ SpisakKnjiga[i].Naziv;
                    if (i < SpisakKnjiga.Count - 1)
                        spisakK=spisakK+ ", ";
                }
            string tekst= $"{Sifra}, {Naziv}, sef: {Sef.Ime} {Sef.Prezime}, spisak autora: {spisakA}, spisak knjiga: {spisakK}";
            return tekst;
        }
        public void Validacija()
        {
            if (string.IsNullOrWhiteSpace(Sifra))
                throw new ArgumentException("Šifra izdavača je obavezna.");

            if (string.IsNullOrWhiteSpace(Naziv))
                throw new ArgumentException("Naziv izdavača je obavezan.");

            if (Sef == null)
                throw new ArgumentException("Izdavač mora imati šefa.");

            if (Sef.God_Iskustva < 5)
                throw new ArgumentException("Šef izdavača mora imati najmanje 5 godina iskustva.");
        }


    }
}
