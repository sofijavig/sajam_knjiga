using System;
using System.Collections.Generic;
using System.Linq;
using Core.Models;
using Core.DAO;

namespace Core.Services
{
    public class AutorService
    {
        private readonly AutorDAO autorDAO = new AutorDAO();

        private static string Norm(string s) => (s ?? string.Empty).Trim();

        public bool AutorPostoji(string lk)
        {
            var key = Norm(lk);
            var autori = autorDAO.Load();
            return autori.Any(a => Norm(a.LK).Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        public void AddAutor(Autor autor)
        {
            if (autor == null) throw new ArgumentNullException(nameof(autor));

            autor.LK = Norm(autor.LK);

            if (AutorPostoji(autor.LK))
                throw new InvalidOperationException("Autor sa ovom ličnom kartom već postoji!");

            autor.Validate();
            autorDAO.Add(autor);
        }

        public List<Autor> GetAll()
        {
            return autorDAO.Load();
        }

        public Autor? GetByLK(string lk)
        {
            var key = Norm(lk);
            return autorDAO.Load().FirstOrDefault(a => Norm(a.LK).Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        public bool UpdateAutor(string lk, Autor noviAutor)
        {
            if (noviAutor == null) throw new ArgumentNullException(nameof(noviAutor));

            var key = Norm(lk);
            var autori = autorDAO.Load();

            var autor = autori.FirstOrDefault(a => Norm(a.LK).Equals(key, StringComparison.OrdinalIgnoreCase));
            if (autor == null)
                return false;

            // LK je ključ -> ne menjamo ga kroz update
            noviAutor.LK = autor.LK;

            noviAutor.Validate();

            autor.Ime = noviAutor.Ime;
            autor.Prezime = noviAutor.Prezime;
            autor.DatRodj = noviAutor.DatRodj;
            autor.Ad = noviAutor.Ad;
            autor.BrojTelefona = noviAutor.BrojTelefona;
            autor.Mail = noviAutor.Mail;
            autor.God_Iskustva = noviAutor.God_Iskustva;

            foreach (var knjiga in noviAutor.Knjige)
            {
                if (!autor.Knjige.Any(k => k.ISBN == knjiga.ISBN))
                    autor.Knjige.Add(knjiga);
            }

            autorDAO.Save(autori);
            return true;
        }

        public bool DeleteAutor(string lk)
        {
            var key = Norm(lk);
            var autori = autorDAO.Load();

            var autor = autori.FirstOrDefault(a => Norm(a.LK).Equals(key, StringComparison.OrdinalIgnoreCase));
            if (autor == null)
                return false;

            autori.Remove(autor);
            autorDAO.Save(autori);
            return true;
        }
    }
}
