using System;
using System.Collections.Generic;
using System.Linq;
using Core.Models;
using Core.DAO;

namespace Core.Services
{
    public class KnjigaService
    {
        private readonly KnjigaDAO knjigaDAO = new KnjigaDAO();
        private readonly AutorService _autorService = new AutorService();

        private static string NormISBN(string isbn) =>
            (isbn ?? string.Empty).Trim();

        public bool KnjigaPostoji(string isbn)
        {
            var key = NormISBN(isbn);
            var knjige = knjigaDAO.Load();
            return knjige.Any(k => NormISBN(k.ISBN).Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        public void AddKnjiga(Knjiga knjiga)
        {
            if (knjiga == null)
                throw new ArgumentNullException(nameof(knjiga));

            knjiga.ISBN = NormISBN(knjiga.ISBN);

            if (KnjigaPostoji(knjiga.ISBN))
                throw new InvalidOperationException("Knjiga sa ovim ISBN već postoji!");

            knjiga.Validate();
            knjigaDAO.Add(knjiga);

            // ✅ posle dodavanja knjige: sync izdavača
            new IzdavacServices().SyncSveIzKnjiga();
        }

        public List<Knjiga> GetAll()
        {
            var knjige = knjigaDAO.Load();

            foreach (var k in knjige)
            {
                if (k.Autori == null) { k.Autori = new List<Autor>(); continue; }

                var novi = new List<Autor>();

                foreach (var a in k.Autori)
                {
                    var lk = (a?.LK ?? "").Trim();
                    if (lk == "") continue;

                    var autor = _autorService.GetByLK(lk);
                    if (autor != null) novi.Add(autor);
                    else novi.Add(new Autor { LK = lk }); // fallback
                }

                k.Autori = novi;
            }

            return knjige;
        }

        public Knjiga? GetByISBN(string isbn)
        {
            var key = NormISBN(isbn);
            return knjigaDAO.Load()
                .FirstOrDefault(k => NormISBN(k.ISBN).Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        public bool UpdateKnjiga(string isbn, Knjiga novaKnjiga)
        {
            if (novaKnjiga == null)
                throw new ArgumentNullException(nameof(novaKnjiga));

            var key = NormISBN(isbn);
            var knjige = knjigaDAO.Load();

            var knjiga = knjige.FirstOrDefault(k => NormISBN(k.ISBN).Equals(key, StringComparison.OrdinalIgnoreCase));
            if (knjiga == null)
                return false;

            novaKnjiga.ISBN = knjiga.ISBN;
            novaKnjiga.Validate();

            knjiga.Naziv = novaKnjiga.Naziv;
            knjiga.Zanr = novaKnjiga.Zanr;
            knjiga.GodinaIzdanja = novaKnjiga.GodinaIzdanja;
            knjiga.Cena = novaKnjiga.Cena;
            knjiga.BrojStrana = novaKnjiga.BrojStrana;
            knjiga.Izdavac = novaKnjiga.Izdavac;

            knjiga.Autori.Clear();
            if (novaKnjiga.Autori != null && novaKnjiga.Autori.Count > 0)
                knjiga.Autori.AddRange(novaKnjiga.Autori);

            knjigaDAO.Save(knjige);

            // ✅ posle izmene knjige: sync izdavača
            new IzdavacServices().SyncSveIzKnjiga();

            return true;
        }

        public bool DeleteKnjiga(string isbn)
        {
            var key = NormISBN(isbn);
            var knjige = knjigaDAO.Load();

            var knjiga = knjige.FirstOrDefault(k => NormISBN(k.ISBN).Equals(key, StringComparison.OrdinalIgnoreCase));
            if (knjiga == null)
                return false;

            knjige.Remove(knjiga);
            knjigaDAO.Save(knjige);

            // ✅ posle brisanja knjige: sync izdavača
            new IzdavacServices().SyncSveIzKnjiga();

            return true;
        }

        /// <summary>
        /// Postavi kompletan spisak knjiga za autora (dodaj/ukloni autora u knjige.txt)
        /// i onda sync izdavača, da se autori pojave/izbrišu u izdavaču.
        /// </summary>
        public void PostaviKnjigeAutoru(string lkAutora, List<Knjiga> noveKnjigeAutora)
        {
            if (string.IsNullOrWhiteSpace(lkAutora)) return;
            lkAutora = lkAutora.Trim();

            var sveKnjige = knjigaDAO.Load() ?? new List<Knjiga>();

            var targetIsbn = new HashSet<string>(
                (noveKnjigeAutora ?? new List<Knjiga>())
                    .Where(k => k != null && !string.IsNullOrWhiteSpace(k.ISBN))
                    .Select(k => k.ISBN.Trim()),
                StringComparer.OrdinalIgnoreCase);

            foreach (var k in sveKnjige)
            {
                if (k == null) continue;
                if (k.Autori == null) k.Autori = new List<Autor>();

                string isbnTrim = (k.ISBN ?? "").Trim();
                bool trebaDaIma = targetIsbn.Contains(isbnTrim);

                var postojeciAutor = k.Autori.FirstOrDefault(a =>
                    string.Equals((a?.LK ?? "").Trim(), lkAutora, StringComparison.OrdinalIgnoreCase));

                if (trebaDaIma)
                {
                    if (postojeciAutor == null)
                        k.Autori.Add(new Autor { LK = lkAutora });
                }
                else
                {
                    if (postojeciAutor != null)
                        k.Autori.Remove(postojeciAutor);
                }
            }

            knjigaDAO.Save(sveKnjige);

            // ✅ posle menjanja knjiga autoru: sync izdavača
            new IzdavacServices().SyncSveIzKnjiga();
        }
    }
}