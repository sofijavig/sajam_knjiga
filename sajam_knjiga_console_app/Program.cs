using System;
using System.Collections.Generic;
using Core.Models;
using Core.Services;

class Program
{
    static AutorService autorService = new AutorService();
    static KnjigaService knjigaService = new KnjigaService();
    static AdresaService adresaService = new AdresaService();
    static PosetilacServices posetilacService = new PosetilacServices();
    static IzdavacServices izdavacService = new IzdavacServices();
    static KupovinaServices kupovinaService = new KupovinaServices();

    static void Main()
    {
        while (true)
        {
            Console.WriteLine("\n--- GLAVNI MENI ---");
            Console.WriteLine("1. Autor");
            Console.WriteLine("2. Knjiga");
            Console.WriteLine("3. Adresa");
            Console.WriteLine("4. Posetilac");
            Console.WriteLine("5. Izdavac");
            Console.WriteLine("6. Kupovina");
            Console.WriteLine("7. Izlaz");
            Console.Write("Izbor: ");
            string izbor = Console.ReadLine();

            switch (izbor)
            {
                case "1": AutorMeni(); break;
                case "2": KnjigaMeni(); break;
                case "3": AdresaMeni(); break;
                case "4": PosetilacMeni(); break;
                case "5": IzdavacMeni(); break;
                case "6": KupovinaMeni(); break;
                case "7": return;
                default: Console.WriteLine("Nepoznata opcija!"); break;
            }
        }
    }

    // ---------------- AUTOR ----------------
    static void AutorMeni()
    {
        while (true)
        {
            Console.WriteLine("\n--- MENI AUTOR ---");
            Console.WriteLine("1. Dodaj autora");
            Console.WriteLine("2. Prikazi sve autore");
            Console.WriteLine("3. Izmeni autora");
            Console.WriteLine("4. Obrisi autora");
            Console.WriteLine("5. Pretraga autora po LK");
            Console.WriteLine("6. Povratak na glavni meni");
            Console.Write("Izbor: ");
            string izbor = Console.ReadLine();

            switch (izbor)
            {
                case "1": DodajAutora(); break;
                case "2": PrikaziSveAutore(); break;
                case "3": IzmeniAutora(); break;
                case "4": ObrisiAutora(); break;
                case "5": PretragaAutora(); break;
                case "6": return;
                default: Console.WriteLine("Nepoznata opcija!"); break;
            }
        }
    }

    static void DodajAutora()
    {
        try
        {
            Autor noviAutor = new Autor();
            Console.Write("Ime: "); noviAutor.Ime = Console.ReadLine();
            Console.Write("Prezime: "); noviAutor.Prezime = Console.ReadLine();
            Console.Write("Datum rodjenja (yyyy-MM-dd): "); noviAutor.DatRodj = DateTime.Parse(Console.ReadLine());
            Console.Write("Broj telefona: "); noviAutor.BrojTelefona = Console.ReadLine();
            Console.Write("Mail: "); noviAutor.Mail = Console.ReadLine();
            Console.Write("Licna karta (LK): "); noviAutor.LK = Console.ReadLine();
            Console.Write("Godine iskustva: "); noviAutor.God_Iskustva = int.Parse(Console.ReadLine());

            Console.Write("Ulica: "); noviAutor.Ad.Ulica = Console.ReadLine();
            Console.Write("Broj: "); noviAutor.Ad.Broj = int.Parse(Console.ReadLine());
            Console.Write("Grad: "); noviAutor.Ad.Grad = Console.ReadLine();
            Console.Write("Drzava: "); noviAutor.Ad.Drzava = Console.ReadLine();

            Console.Write("Koliko knjiga dodati? "); int brKnjiga = int.Parse(Console.ReadLine());
            for (int i = 0; i < brKnjiga; i++)
            {
                Knjiga knjiga = new Knjiga();
                Console.Write($"ISBN knjige {i + 1}: "); knjiga.ISBN = Console.ReadLine();
                Console.Write($"Naziv knjige {i + 1}: "); knjiga.Naziv = Console.ReadLine();
                noviAutor.Knjige.Add(knjiga);
            }

            noviAutor.Validate();
            if (autorService.AutorPostoji(noviAutor.LK))
            {
                Console.WriteLine("Autor sa ovom licnom kartom već postoji!");
                return;
            }

            autorService.AddAutor(noviAutor);
            Console.WriteLine("Autor dodat!");
        }
        catch (Exception ex) { Console.WriteLine($"Greska: {ex.Message}"); }
    }

    static void PrikaziSveAutore()
    {
        var autori = autorService.GetAll();
        foreach (var a in autori) Console.WriteLine(a);
    }

    static void IzmeniAutora()
    {
        Console.Write("Unesite LK autora koji zelite izmeniti: ");
        string lk = Console.ReadLine();
        var autor = autorService.GetByLK(lk);
        if (autor == null) { Console.WriteLine("Autor nije pronadjen."); return; }

        try
        {
            Autor noviAutor = new Autor();
            Console.Write("Novo ime: "); noviAutor.Ime = Console.ReadLine();
            Console.Write("Novo prezime: "); noviAutor.Prezime = Console.ReadLine();
            Console.Write("Novi datum rodjenja (yyyy-MM-dd): "); noviAutor.DatRodj = DateTime.Parse(Console.ReadLine());
            Console.Write("Novi broj telefona: "); noviAutor.BrojTelefona = Console.ReadLine();
            Console.Write("Novi mail: "); noviAutor.Mail = Console.ReadLine();
            Console.Write("Nova LK: "); noviAutor.LK = Console.ReadLine();
            Console.Write("Nove godine iskustva: "); noviAutor.God_Iskustva = int.Parse(Console.ReadLine());

            Console.Write("Ulica: "); noviAutor.Ad.Ulica = Console.ReadLine();
            Console.Write("Broj: "); noviAutor.Ad.Broj = int.Parse(Console.ReadLine());
            Console.Write("Grad: "); noviAutor.Ad.Grad = Console.ReadLine();
            Console.Write("Drzava: "); noviAutor.Ad.Drzava = Console.ReadLine();

            Console.Write("Koliko knjiga dodati? "); int brKnjiga = int.Parse(Console.ReadLine());
            for (int i = 0; i < brKnjiga; i++)
            {
                Knjiga knjiga = new Knjiga();
                Console.Write($"ISBN knjige {i + 1}: "); knjiga.ISBN = Console.ReadLine();
                Console.Write($"Naziv knjige {i + 1}: "); knjiga.Naziv = Console.ReadLine();
                noviAutor.Knjige.Add(knjiga);
            }

            noviAutor.Validate();
            autorService.UpdateAutor(lk, noviAutor);
            Console.WriteLine("Autor izmenjen!");
        }
        catch (Exception ex) { Console.WriteLine($"Greska: {ex.Message}"); }
    }

    static void ObrisiAutora()
    {
        Console.Write("Unesite LK autora koji zelite obrisati: ");
        string lk = Console.ReadLine();
        if (autorService.DeleteAutor(lk)) Console.WriteLine("Autor obrisan!");
        else Console.WriteLine("Autor nije pronadjen.");
    }

    static void PretragaAutora()
    {
        Console.Write("Unesite LK: ");
        string lk = Console.ReadLine();
        var autor = autorService.GetByLK(lk);
        if (autor != null) Console.WriteLine(autor);
        else Console.WriteLine("Autor nije pronadjen.");
    }

    // ---------------- KNJIGA ----------------
    static void KnjigaMeni()
    {
        while (true)
        {
            Console.WriteLine("\n--- MENI KNJIGA ---");
            Console.WriteLine("1. Dodaj knjigu");
            Console.WriteLine("2. Prikazi sve knjige");
            Console.WriteLine("3. Izmeni knjigu");
            Console.WriteLine("4. Obrisi knjigu");
            Console.WriteLine("5. Povratak na glavni meni");
            Console.Write("Izbor: ");
            string izbor = Console.ReadLine();

            switch (izbor)
            {
                case "1":
                    try
                    {
                        Knjiga k = new Knjiga();
                        Console.Write("ISBN: "); k.ISBN = Console.ReadLine();
                        Console.Write("Naziv: "); k.Naziv = Console.ReadLine();
                        Console.Write("Zanr: "); k.Zanr = Console.ReadLine();
                        Console.Write("Godina izdanja: "); k.GodinaIzdanja = int.Parse(Console.ReadLine());
                        Console.Write("Cena: "); k.Cena = int.Parse(Console.ReadLine());
                        Console.Write("Broj strana: "); k.BrojStrana = int.Parse(Console.ReadLine());
                        do { 
                            Autor a = new Autor();
                            Console.Write("Ime autora: "); a.Ime = Console.ReadLine();
                            Console.Write("Prezime autora: "); a.Prezime = Console.ReadLine();
                            k.Autori.Add(a);
                            Console.Write("Dodati jos jednog autora? (d/n): ");
                        } while (Console.ReadLine().ToLower() == "d");
                        Console.Write("Izdavac: "); k.Izdavac = Console.ReadLine();
                        k.Validate();
                        if (!knjigaService.KnjigaPostoji(k.ISBN)) { knjigaService.AddKnjiga(k); Console.WriteLine("Knjiga dodata!"); }
                        else Console.WriteLine("Knjiga sa ovim ISBN već postoji!");
                    }
                    catch (Exception ex) { Console.WriteLine($"Greska: {ex.Message}"); }
                    break;
                case "2": foreach (var kn in knjigaService.GetAll()) Console.WriteLine(kn); break;
                case "3":
                    try
                    {
                        Console.Write("Unesite ISBN knjige za izmenu: ");
                        string isbn = Console.ReadLine();
                        var knjiga = knjigaService.GetByISBN(isbn);
                        if (knjiga == null) { Console.WriteLine("Knjiga nije pronadjena."); break; }
                        Console.Write("Novi naziv: "); knjiga.Naziv = Console.ReadLine();
                        knjiga.Validate();
                        knjigaService.UpdateKnjiga(isbn, knjiga);
                        Console.WriteLine("Knjiga izmenjena!");
                    }
                    catch (Exception ex) { Console.WriteLine($"Greska: {ex.Message}"); }
                    break;
                case "4":
                    Console.Write("Unesite ISBN knjige za brisanje: ");
                    string isbnBr = Console.ReadLine();
                    if (knjigaService.DeleteKnjiga(isbnBr)) Console.WriteLine("Knjiga obrisana!");
                    else Console.WriteLine("Knjiga nije pronadjena.");
                    break;
                case "5": return;
                default: Console.WriteLine("Nepoznata opcija!"); break;
            }
        }
    }

    // ---------------- ADRESA ----------------
    static void AdresaMeni()
    {
        while (true)
        {
            Console.WriteLine("\n--- MENI ADRESA ---");
            Console.WriteLine("1. Dodaj adresu");
            Console.WriteLine("2. Prikazi sve adrese");
            Console.WriteLine("3. Izmeni adresu");
            Console.WriteLine("4. Obrisi adresu");
            Console.WriteLine("5. Povratak na glavni meni");
            Console.Write("Izbor: ");
            string izbor = Console.ReadLine();

            switch (izbor)
            {
                case "1":
                    try
                    {
                        Adresa a = new Adresa();
                        Console.Write("Ulica: "); a.Ulica = Console.ReadLine();
                        Console.Write("Broj: "); a.Broj = int.Parse(Console.ReadLine());
                        Console.Write("Grad: "); a.Grad = Console.ReadLine();
                        Console.Write("Drzava: "); a.Drzava = Console.ReadLine();
                        a.validate();
                        adresaService.AddAdresa(a);
                        Console.WriteLine("Adresa dodata!");
                    }
                    catch (Exception ex) { Console.WriteLine($"Greska: {ex.Message}"); }
                    break;
                case "2": foreach (var adr in adresaService.GetAll()) Console.WriteLine(adr); break;
                case "3":
                    try
                    {
                        Console.Write("Unesite ulicu adrese koju zelite izmeniti: ");
                        string ulica = Console.ReadLine();
                        var adresa = adresaService.GetByUlica(ulica);
                        if (adresa == null) { Console.WriteLine("Adresa nije pronadjena."); break; }
                        Console.Write("Nova ulica: "); adresa.Ulica = Console.ReadLine();
                        Console.Write("Novi broj: "); adresa.Broj = int.Parse(Console.ReadLine());
                        Console.Write("Novi grad: "); adresa.Grad = Console.ReadLine();
                        Console.Write("Nova drzava: "); adresa.Drzava = Console.ReadLine();
                        adresa.validate();
                        adresaService.UpdateAdresa(ulica, adresa);
                        Console.WriteLine("Adresa izmenjena!");
                    }
                    catch (Exception ex) { Console.WriteLine($"Greska: {ex.Message}"); }
                    break;
                case "4":
                    Console.Write("Unesite ulicu adrese za brisanje: ");
                    string ulicaBr = Console.ReadLine();
                    if (adresaService.DeleteAdresa(ulicaBr)) Console.WriteLine("Adresa obrisana!");
                    else Console.WriteLine("Adresa nije pronadjena.");
                    break;
                case "5": return;
                default: Console.WriteLine("Nepoznata opcija!"); break;
            }
        }
    }


    // ---------------- Posetilac meni ----------------
    static void PosetilacMeni()
    {
        while (true)
        {
            Console.WriteLine("\n--- MENI POSETILAC ---");
            Console.WriteLine("1. Dodaj posetioca");
            Console.WriteLine("2. Prikazi sve posetioce");
            Console.WriteLine("3. Izmeni posetioca");
            Console.WriteLine("4. Obrisi posetioca");
            Console.WriteLine("5. Povratak na glavni meni");
            Console.Write("Izbor: ");
            string izbor = Console.ReadLine();

            switch (izbor)
            {
                case "1":
                    try
                    {
                        Posetilac p = new Posetilac();
                        Console.Write("Ime: "); p.Ime = Console.ReadLine();
                        Console.Write("Prezime: "); p.Prezime = Console.ReadLine();
                        Console.Write("Datum rodjenja (yyyy-MM-dd): "); p.DatRodj = DateTime.Parse(Console.ReadLine());
                        Console.Write("Broj telefona: "); p.BrojTelefona = Console.ReadLine();
                        Console.Write("Mail: "); p.Mail = Console.ReadLine();
                        Console.Write("Broj clanske karte (CK): "); p.CK = Console.ReadLine();
                        Console.Write("Godina clanstva: "); p.GodC = int.Parse(Console.ReadLine());
                        Console.Write("Status (R/V): "); p.Status = (StatusPosetioca)Enum.Parse(typeof(StatusPosetioca), Console.ReadLine());
                        p.Validacija();
                        posetilacService.Dodaj(p);
                        Console.WriteLine("Posetilac dodat!");
                    }
                    catch (Exception ex) { Console.WriteLine($"Greska: {ex.Message}"); }
                    break;

                case "2": posetilacService.Prikazi(); break;

                case "3":
                    try
                    {
                        Console.Write("Unesite CK posetioca za izmenu: ");
                        string ck = Console.ReadLine();
                        Posetilac novi = new Posetilac();
                        Console.Write("Novo ime: "); novi.Ime = Console.ReadLine();
                        Console.Write("Novo prezime: "); novi.Prezime = Console.ReadLine();
                        Console.Write("Novi datum rodjenja (yyyy-MM-dd): "); novi.DatRodj = DateTime.Parse(Console.ReadLine());
                        Console.Write("Novi broj telefona: "); novi.BrojTelefona = Console.ReadLine();
                        Console.Write("Novi mail: "); novi.Mail = Console.ReadLine();
                        Console.Write("Nova CK: "); novi.CK = Console.ReadLine();
                        Console.Write("Nova godina clanstva: "); novi.GodC = int.Parse(Console.ReadLine());
                        Console.Write("Novi status (R/V): "); novi.Status = (StatusPosetioca)Enum.Parse(typeof(StatusPosetioca), Console.ReadLine());
                        novi.Validacija();
                        posetilacService.Izmeni(ck, novi);
                        Console.WriteLine("Posetilac izmenjen!");
                    }
                    catch (Exception ex) { Console.WriteLine($"Greska: {ex.Message}"); }
                    break;

                case "4":
                    Console.Write("Unesite CK posetioca za brisanje: ");
                    string ckBr = Console.ReadLine();
                    try
                    {
                        posetilacService.Obrisi(ckBr);
                        Console.WriteLine("Posetilac obrisan!");
                    }
                    catch (Exception ex) { Console.WriteLine($"Greska: {ex.Message}"); }
                    break;

                case "5": return;
                default: Console.WriteLine("Nepoznata opcija!"); break;
            }
        }
    }

    // ---------------- Izdavac meni ----------------
    static void IzdavacMeni()
    {
        while (true)
        {
            Console.WriteLine("\n--- MENI IZDAVAC ---");
            Console.WriteLine("1. Dodaj izdavaca");
            Console.WriteLine("2. Prikazi sve izdavace");
            Console.WriteLine("3. Izmeni izdavaca");
            Console.WriteLine("4. Obrisi izdavaca");
            Console.WriteLine("5. Povratak na glavni meni");
            Console.Write("Izbor: ");
            string izbor = Console.ReadLine();

            switch (izbor)
            {
                case "1":
                    try
                    {
                        Izdavac izd = new Izdavac();
                        Console.Write("Sifra: "); izd.Sifra = Console.ReadLine();
                        Console.Write("Naziv: "); izd.Naziv = Console.ReadLine();
                     
                        Autor sef = new Autor();
                        Console.Write("Ime sef-a: "); sef.Ime = Console.ReadLine();
                        Console.Write("Prezime sef-a: "); sef.Prezime = Console.ReadLine();
                        Console.Write("LK sef-a: "); sef.LK = Console.ReadLine();
                        Console.Write("Godine iskustva sef-a: "); sef.God_Iskustva = int.Parse(Console.ReadLine());
                    
                        izd.Sef = sef;
                        izd.Validacija();

                        izdavacService.Dodaj(izd);
                        Console.WriteLine("Izdavac dodat!");
                    }
                    catch (Exception ex) { Console.WriteLine($"Greska: {ex.Message}"); }
                    break;

                case "2": izdavacService.Prikazi(); break;

                case "3":
                    try
                    {
                        Console.Write("Unesite sifru izdavaca za izmenu: ");
                        string sifra = Console.ReadLine();
                        Izdavac novi = new Izdavac();
                        Console.Write("Novo ime izdavaca: "); novi.Naziv = Console.ReadLine();
                        novi.Validacija();
                        izdavacService.Izmeni(sifra, novi);
                        Console.WriteLine("Izdavac izmenjen!");
                    }
                    catch (Exception ex) { Console.WriteLine($"Greska: {ex.Message}"); }
                    break;

                case "4":
                    Console.Write("Unesite sifru izdavaca za brisanje: ");
                    string sifraBr = Console.ReadLine();
                    try
                    {
                        izdavacService.Obrisi(sifraBr);
                        Console.WriteLine("Izdavac obrisan!");
                    }
                    catch (Exception ex) { Console.WriteLine($"Greska: {ex.Message}"); }
                    break;

                case "5": return;
                default: Console.WriteLine("Nepoznata opcija!"); break;
            }
        }
    }

    // ---------------- Kupovina meni ----------------
    static void KupovinaMeni()
    {
        while (true)
        {
            Console.WriteLine("\n--- MENI KUPPOVINA ---");
            Console.WriteLine("1. Dodaj kupovinu");
            Console.WriteLine("2. Prikazi sve kupovine");
            Console.WriteLine("3. Izmeni kupovinu");
            Console.WriteLine("4. Obrisi kupovinu");
            Console.WriteLine("5. Povratak na glavni meni");
            Console.Write("Izbor: ");
            string izbor = Console.ReadLine();

            switch (izbor)
            {
                case "1":
                    try
                    {
                        Kupovina k = new Kupovina();
                        Console.Write("CK posetioca: "); k.Pos.CK = Console.ReadLine();
                        Console.Write("ISBN knjige: "); k.Knj.ISBN = Console.ReadLine();
                        Console.Write("Datum kupovine (yyyy-MM-dd): "); k.DatKup = DateTime.Parse(Console.ReadLine());
                        Console.Write("Ocena (1-5): "); k.Ocena = int.Parse(Console.ReadLine());
                        Console.Write("Komentar: "); k.Komentar = Console.ReadLine();
                        k.Validacija();
                        kupovinaService.Dodaj(k);
                        Console.WriteLine("Kupovina dodata!");
                    }
                    catch (Exception ex) { Console.WriteLine($"Greska: {ex.Message}"); }
                    break;

                case "2": kupovinaService.Prikazi(); break;

                case "3":
                    try
                    {
                        Console.Write("CK posetioca: "); string ck = Console.ReadLine();
                        Console.Write("ISBN knjige: "); string isbn = Console.ReadLine();
                        Console.Write("Datum kupovine (yyyy-MM-dd): "); DateTime dat = DateTime.Parse(Console.ReadLine());
                        Kupovina nova = new Kupovina();
                        Console.Write("Nova ocena: "); nova.Ocena = int.Parse(Console.ReadLine());
                        Console.Write("Novi komentar: "); nova.Komentar = Console.ReadLine();
                        nova.Validacija();
                        kupovinaService.Izmeni(ck, isbn, dat, nova);
                        Console.WriteLine("Kupovina izmenjena!");
                    }
                    catch (Exception ex) { Console.WriteLine($"Greska: {ex.Message}"); }
                    break;

                case "4":
                    try
                    {
                        Console.Write("CK posetioca: "); string ck = Console.ReadLine();
                        Console.Write("ISBN knjige: "); string isbn = Console.ReadLine();
                        Console.Write("Datum kupovine (yyyy-MM-dd): "); DateTime dat = DateTime.Parse(Console.ReadLine());
                        kupovinaService.Obrisi(ck, isbn, dat);
                        Console.WriteLine("Kupovina obrisana!");
                    }
                    catch (Exception ex) { Console.WriteLine($"Greska: {ex.Message}"); }
                    break;

                case "5": return;
                default: Console.WriteLine("Nepoznata opcija!"); break;
            }
        }
    }
}

