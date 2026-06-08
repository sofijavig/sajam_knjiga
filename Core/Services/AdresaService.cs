using System;
using System.Collections.Generic;
using Core.Models;
using Core.DAO;

namespace Core.Services
{
    public class AdresaService
    {
        private readonly AdreseDAO adresaDAO = new AdreseDAO();

        //ne proveravam jel postoji adresa jer sto ne bi mogli da zive na istoj adresi

        // Dodavanje adrese
        public void AddAdresa(Adresa adresa)
        {
            adresaDAO.Add(adresa);
        }

        // Dohvatanje svih adresa
        public List<Adresa> GetAll()
        {
            return adresaDAO.Load();
        }

        // Dohvatanje adrese po ulici
        public Adresa? GetByUlica(string ulica)
        {
            return adresaDAO.Load().Find(a => a.Ulica.Equals(ulica, StringComparison.OrdinalIgnoreCase));
        } //Adresa je cela posebna ne znam sta bih stavil za kljuce

        // Update adrese
        public bool UpdateAdresa(string staraUlica, Adresa novaAdresa) //ovde je ulica kljuc za apdejtovanje adrese
        {
            var adrese = adresaDAO.Load();
            var adresa = adrese.Find(a => a.Ulica.Equals(staraUlica, StringComparison.OrdinalIgnoreCase));//lambda funkcija,poredi trenutni element sa novim bez obzira na mala velika slova
            if (adresa == null)
                return false;

            adresa.Ulica = novaAdresa.Ulica;
            adresa.Broj = novaAdresa.Broj;
            adresa.Grad = novaAdresa.Grad;
            adresa.Drzava = novaAdresa.Drzava;

            adresaDAO.Save(adrese);
            return true;
        }

        // Brisanje adrese
        public bool DeleteAdresa(string ulica)
        {
            var adrese = adresaDAO.Load();
            var adresa = adrese.Find(a => a.Ulica.Equals(ulica, StringComparison.OrdinalIgnoreCase)); //brise se po ulici
            if (adresa == null)
                return false;

            adrese.Remove(adresa); //sklanjase iz liste
            adresaDAO.Save(adrese); //zatim i iz fajla
            return true;
        }
    }
}
