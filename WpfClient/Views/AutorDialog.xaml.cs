using Core.Models;
using Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfClient.Views;

namespace WpfClient
{
    public partial class AutorDialog : Window
    {
        public Autor? Result { get; private set; }

        private readonly bool _isEdit;
        private readonly Autor? _existing;
        private readonly List<Knjiga> _allKnjige;
        private readonly ObservableCollection<Knjiga> _knjigeAutora = new();
        private readonly KnjigaService _knjigaService = new KnjigaService();

        public AutorDialog(List<Knjiga> allKnjige, Autor? existing = null)
        {
            InitializeComponent();

            _allKnjige = allKnjige ?? new List<Knjiga>();
            _existing = existing;
            _isEdit = existing != null;

            DgKnjigeAutora.ItemsSource = _knjigeAutora;

            if (_isEdit)
            {
                Title = T("EditAuthorTitle");
                LoadExisting(existing!);
                TbLK.IsEnabled = false;
            }
            else
            {
                Title = T("AddAuthorTitle");

                if (DpDatRodj.SelectedDate == null)
                    DpDatRodj.SelectedDate = DateTime.Today;

                TbGodIsk.Text = "0";
                TbBroj.Text = "1";
            }

            UpdateConfirmEnabled();
        }

        // --- Localization helper ---
        private static string T(string key)
        {
            // vraća key ako ne postoji (da odmah vidiš šta fali)
            return Application.Current?.TryFindResource(key) as string ?? key;
        }

        private void ShowError(string message, Control? focus = null)
        {
            new ErrorDialog(message) { Owner = this }.ShowDialog();
            if (focus != null)
            {
                focus.Focus();
                if (focus is TextBox tb) tb.SelectAll();
            }
        }

        private void LoadExisting(Autor a)
        {
            TbLK.Text = a.LK ?? string.Empty;
            TbIme.Text = a.Ime ?? string.Empty;
            TbPrezime.Text = a.Prezime ?? string.Empty;
            DpDatRodj.SelectedDate = a.DatRodj;

            TbMail.Text = a.Mail ?? string.Empty;
            TbTelefon.Text = a.BrojTelefona ?? string.Empty;
            TbGodIsk.Text = a.God_Iskustva.ToString();

            TbUlica.Text = a.Ad?.Ulica ?? string.Empty;
            TbBroj.Text = (a.Ad?.Broj ?? 0).ToString();
            TbGrad.Text = a.Ad?.Grad ?? string.Empty;
            TbDrzava.Text = a.Ad?.Drzava ?? string.Empty;

            _knjigeAutora.Clear();

            var knjigeKojeJePisao = _allKnjige
                .Where(k => k != null
                    && k.Autori != null
                    && k.Autori.Any(x =>
                        !string.IsNullOrWhiteSpace(x?.LK) &&
                        string.Equals(x.LK.Trim(), (a.LK ?? "").Trim(), StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var k in knjigeKojeJePisao)
                _knjigeAutora.Add(k);
        }

        private void AnyChanged(object sender, EventArgs e) => UpdateConfirmEnabled();

        private void UpdateConfirmEnabled()
        {
            bool godOk = int.TryParse(TbGodIsk.Text.Trim(), out int gi) && gi >= 0;
            bool brojOk = int.TryParse(TbBroj.Text.Trim(), out int broj) && broj > 0;

            var mail = (TbMail.Text ?? string.Empty).Trim();
            bool mailOk = !string.IsNullOrWhiteSpace(mail) && mail.Contains('@');

            bool ok =
                !string.IsNullOrWhiteSpace(TbLK.Text) &&
                !string.IsNullOrWhiteSpace(TbIme.Text) &&
                !string.IsNullOrWhiteSpace(TbPrezime.Text) &&
                DpDatRodj.SelectedDate != null &&
                mailOk &&
                !string.IsNullOrWhiteSpace(TbTelefon.Text) &&
                godOk &&
                !string.IsNullOrWhiteSpace(TbUlica.Text) &&
                brojOk &&
                !string.IsNullOrWhiteSpace(TbGrad.Text) &&
                !string.IsNullOrWhiteSpace(TbDrzava.Text) &&
                _knjigeAutora.Count >= 1;

            BtnPotvrdi.IsEnabled = ok;
        }

        private void BtnPotvrdi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string lk = TbLK.Text.Trim();
                string ime = TbIme.Text.Trim();
                string prezime = TbPrezime.Text.Trim();

                if (string.IsNullOrWhiteSpace(lk)) { ShowError(T("Err_Author_LkRequired"), TbLK); return; }
                if (string.IsNullOrWhiteSpace(ime)) { ShowError(T("Err_Author_FirstNameRequired"), TbIme); return; }
                if (string.IsNullOrWhiteSpace(prezime)) { ShowError(T("Err_Author_LastNameRequired"), TbPrezime); return; }
                if (DpDatRodj.SelectedDate == null) { ShowError(T("Err_Author_BirthDateRequired"), DpDatRodj); return; }

                var mail = TbMail.Text.Trim();
                if (string.IsNullOrWhiteSpace(mail) || !mail.Contains('@')) { ShowError(T("Err_Author_EmailInvalid"), TbMail); return; }

                if (string.IsNullOrWhiteSpace(TbTelefon.Text)) { ShowError(T("Err_Author_PhoneRequired"), TbTelefon); return; }

                if (!int.TryParse(TbGodIsk.Text.Trim(), out int gi) || gi < 0) { ShowError(T("Err_Author_YearsInvalid"), TbGodIsk); return; }

                if (string.IsNullOrWhiteSpace(TbUlica.Text)) { ShowError(T("Err_Author_StreetRequired"), TbUlica); return; }

                if (!int.TryParse(TbBroj.Text.Trim(), out int broj) || broj <= 0) { ShowError(T("Err_Author_NumberInvalid"), TbBroj); return; }

                if (string.IsNullOrWhiteSpace(TbGrad.Text)) { ShowError(T("Err_Author_CityRequired"), TbGrad); return; }

                if (string.IsNullOrWhiteSpace(TbDrzava.Text)) { ShowError(T("Err_Author_CountryRequired"), TbDrzava); return; }

                var knjige = _knjigeAutora.ToList();
                if (knjige.Count < 1) { ShowError(T("Err_Author_MinOneBook"), DgKnjigeAutora); return; }

                var ad = new Adresa
                {
                    Ulica = TbUlica.Text.Trim(),
                    Broj = broj,
                    Grad = TbGrad.Text.Trim(),
                    Drzava = TbDrzava.Text.Trim()
                };

                ad.validate();

                var autor = new Autor
                {
                    LK = _isEdit ? _existing!.LK : lk,
                    Ime = ime,
                    Prezime = prezime,
                    DatRodj = DpDatRodj.SelectedDate.Value,
                    Mail = mail,
                    BrojTelefona = TbTelefon.Text.Trim(),
                    God_Iskustva = gi,
                    Ad = ad,
                    Knjige = knjige
                };

                autor.Validate();

                Result = autor;
                _knjigaService.PostaviKnjigeAutoru(autor.LK, knjige);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
                e.Handled = true;
                return;
            }

            if (e.Key != Key.Enter) return;

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (BtnPotvrdi.IsEnabled)
                {
                    BtnPotvrdi_Click(BtnPotvrdi, new RoutedEventArgs());
                    e.Handled = true;
                }
                return;
            }

            if (Keyboard.FocusedElement is DataGrid || Keyboard.FocusedElement is ListBox)
                return;

            if (Keyboard.FocusedElement is TextBox tb && tb.AcceptsReturn)
                return;

            if (Keyboard.FocusedElement is DatePicker dp && dp.IsDropDownOpen)
            {
                dp.IsDropDownOpen = false;
                e.Handled = true;
                return;
            }

            e.Handled = true;
            if (Keyboard.FocusedElement is UIElement el)
                el.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void BtnDodajKnjigu_Click(object sender, RoutedEventArgs e)
        {
            var postojeci = new HashSet<string>(
                _knjigeAutora.Where(k => k != null).Select(k => (k.ISBN ?? "").Trim()),
                StringComparer.OrdinalIgnoreCase);

            var kandidati = (_allKnjige ?? new List<Knjiga>())
                .Where(k => k != null && !string.IsNullOrWhiteSpace(k.ISBN) && !postojeci.Contains(k.ISBN.Trim()))
                .ToList();

            if (kandidati.Count == 0)
            {
                ShowError(T("Err_NoBooksToAdd"), DgKnjigeAutora);
                return;
            }

            var dlg = new SelectKnjigaDialog(kandidati) { Owner = this };
            if (dlg.ShowDialog() == true && dlg.Selected != null)
            {
                _knjigeAutora.Add(dlg.Selected);
                UpdateConfirmEnabled();
            }
        }

        private void BtnUkloniKnjigu_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgKnjigeAutora.SelectedItems.Cast<Knjiga>().ToList();

            if (selected.Count == 0)
            {
                ShowError(T("Err_SelectAtLeastOneBook"), DgKnjigeAutora);
                return;
            }

            if (_knjigeAutora.Count - selected.Count < 1)
            {
                new ErrorDialog(T("Err_Author_MinOneBook")) { Owner = this }.ShowDialog();
                return;
            }

            var details = string.Join("\n",
                selected.Select(k => string.Format(
                    T("ConfirmDetails_BookLine"),
                    k.ISBN,
                    k.Naziv)));

            var confirm = new ConfirmDialog2(
                T("Confirm_RemoveBooks_Title"),
                string.Format(T("Confirm_RemoveBooks_Message"), selected.Count),
                details
            )
            { Owner = this };

            if (confirm.ShowDialog() == true)
            {
                foreach (var k in selected)
                    _knjigeAutora.Remove(k);

                UpdateConfirmEnabled();
            }
        }


        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}