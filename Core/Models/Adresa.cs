namespace Core.Models
{
    public class Adresa
    {
        public string Ulica { get; set; } = string.Empty;
        public int Broj { get; set; }
        public string Grad { get; set; } = string.Empty;
        public string Drzava { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Ulica} {Broj}, {Grad} {Drzava}";
        }

        public void validate() { //provere za greske
            if (string.IsNullOrEmpty(Ulica)) 
                throw new ArgumentException("Morate uneti ulicu");
            if (Broj <= 0)
                throw new ArgumentException("Broj mora biti veci od 0");
            if (string.IsNullOrEmpty(Grad))
                throw new ArgumentException("Morate uneti grad");
            if (string.IsNullOrEmpty(Drzava))
                throw new ArgumentException("Morate uneti drzavu");
        }
    }
}
