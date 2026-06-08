using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.Models;

namespace WpfClient.Views
{
    public partial class KnjigaDialog : Window
    {
        public Knjiga? Result { get; private set; }

        private readonly bool _isEdit;
        private readonly Knjiga? _existing;

        // ✅ AUTORI
        private readonly List<Autor> _allAutori;
        private readonly List<Autor> _izabraniAutori = new List<Autor>();

        // ✅ novi konstruktor (prima listu autora)
        public KnjigaDialog(List<Autor> allAutori, Knjiga? existing = null)
        {
            InitializeComponent();
            LbAutori.ItemsSource = _izabraniAutori;
            UpdateAutorUi();
            _allAutori = allAutori ?? new List<Autor>();

            _existing = existing;
            _isEdit = existing != null;

            if (_isEdit)
            {
                Title = "Izmena knjige";
                LoadExisting(existing!);
                TbISBN.IsEnabled = false;
            }
            else
            {
                Title = "Dodavanje knjige";
                TbGodina.Text = DateTime.Now.Year.ToString();
                TbCena.Text = "0";
                TbBrojStrana.Text = "0";
            }

            UpdateAutorUi();
            UpdateConfirmEnabled();
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

        private void LoadExisting(Knjiga k)
        {
            TbISBN.Text = k.ISBN ?? string.Empty;
            TbNaziv.Text = k.Naziv ?? string.Empty;
            TbZanr.Text = k.Zanr ?? string.Empty;
            TbIzdavac.Text = k.Izdavac ?? string.Empty;

            TbCena.Text = k.Cena.ToString();
            TbGodina.Text = k.GodinaIzdanja.ToString();
            TbBrojStrana.Text = k.BrojStrana.ToString();

            _izabraniAutori.Clear();
            if (k.Autori != null)
            {
                foreach (var a in k.Autori)
                    if (a != null && !string.IsNullOrWhiteSpace(a.LK))
                        _izabraniAutori.Add(a);
            }
            UpdateAutorUi();

            RefreshAutoriList();
        }

        private void AnyChanged(object sender, EventArgs e) => UpdateConfirmEnabled();

        private void UpdateConfirmEnabled()
        {
            bool ok =
                !string.IsNullOrWhiteSpace(TbISBN.Text) &&
                !string.IsNullOrWhiteSpace(TbNaziv.Text) &&
                !string.IsNullOrWhiteSpace(TbZanr.Text) &&
                !string.IsNullOrWhiteSpace(TbIzdavac.Text) &&
                int.TryParse(TbGodina.Text.Trim(), out int god) && god > 0 &&
                int.TryParse(TbCena.Text.Trim(), out int cena) && cena >= 0 &&
                int.TryParse(TbBrojStrana.Text.Trim(), out int br) && br > 0;

            BtnPotvrdi.IsEnabled = ok && _izabraniAutori.Count >= 1;
        }

        // ✅ prikaz autora + enable/disable dugmadi
        private void UpdateAutorUi()
        {

            var allAutori = _allAutori ?? new List<Autor>();
            var izabrani = _izabraniAutori ?? new List<Autor>();
            LbAutori.ItemsSource = null;
            LbAutori.ItemsSource = izabrani;
            BtnRemoveAutor.IsEnabled = (LbAutori.SelectedItem is Autor);

            var izabraniLK = new HashSet<string>(
                izabrani.Select(a => (a.LK ?? "").Trim()),
                StringComparer.OrdinalIgnoreCase);

            bool imaZaDodavanje = allAutori.Any(a =>
                a != null &&
                !string.IsNullOrWhiteSpace(a.LK) &&
                !izabraniLK.Contains(a.LK.Trim()));

            BtnAddAutor.IsEnabled = imaZaDodavanje;

            // - aktivan samo ako je selektovan neki već dodat autor
            BtnRemoveAutor.IsEnabled = (LbAutori.SelectedItem is Autor);
        }
        private void BtnPotvrdi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string isbn = TbISBN.Text.Trim();
                string naziv = TbNaziv.Text.Trim();
                string zanr = TbZanr.Text.Trim();
                string izdavac = TbIzdavac.Text.Trim();

                if (string.IsNullOrWhiteSpace(isbn)) { ShowError("ISBN je obavezan.", TbISBN); return; }
                if (string.IsNullOrWhiteSpace(naziv)) { ShowError("Naziv je obavezan.", TbNaziv); return; }
                if (string.IsNullOrWhiteSpace(zanr)) { ShowError("Žanr je obavezan.", TbZanr); return; }
                if (string.IsNullOrWhiteSpace(izdavac)) { ShowError("Izdavač je obavezan.", TbIzdavac); return; }

                if (!int.TryParse(TbGodina.Text.Trim(), out int godina) || godina <= 0) { ShowError("Godina izdanja mora biti ceo broj (> 0).", TbGodina); return; }
                if (!int.TryParse(TbCena.Text.Trim(), out int cena) || cena < 0) { ShowError("Cena mora biti ceo broj (>= 0).", TbCena); return; }
                if (!int.TryParse(TbBrojStrana.Text.Trim(), out int brojStrana) || brojStrana <= 0) { ShowError("Broj strana mora biti ceo broj (> 0).", TbBrojStrana); return; }

                var k = new Knjiga
                {
                    ISBN = _isEdit ? _existing!.ISBN : isbn,
                    Naziv = naziv,
                    Zanr = zanr,
                    Izdavac = izdavac,
                    Cena = cena,
                    GodinaIzdanja = godina,
                    BrojStrana = brojStrana,

                    Autori = _izabraniAutori != null ? _izabraniAutori.ToList() : new List<Autor>()
                };

                Result = k;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void BtnAddAutor_Click(object sender, RoutedEventArgs e)
        {
            var izabraniLK = new HashSet<string>(
                _izabraniAutori.Select(a => (a.LK ?? "").Trim()),
                StringComparer.OrdinalIgnoreCase);

            var kandidati = _allAutori
                .Where(a => a != null && !string.IsNullOrWhiteSpace(a.LK) && !izabraniLK.Contains(a.LK.Trim()))
                .ToList();

            if (kandidati.Count == 0)
                return;

            var dlg = new SelectAutorDialog(kandidati) { Owner = this };
            if (dlg.ShowDialog() == true && dlg.SelectedAutor != null)
            {
                _izabraniAutori.Add(dlg.SelectedAutor);
                UpdateAutorUi();
            }
        }

        private void BtnRemoveAutor_Click(object sender, RoutedEventArgs e)
        {
            if (LbAutori.SelectedItem is not Autor a)
                return;

            if (_izabraniAutori.Count <= 1)
            {
                new ErrorDialog("Knjiga mora imati najmanje jednog autora.")
                { Owner = this }.ShowDialog();
                return;
            }

            var lk = (a.LK ?? "").Trim();
            if (lk == "") return;

            _izabraniAutori.RemoveAll(x =>
                x != null &&
                !string.IsNullOrWhiteSpace(x.LK) &&
                string.Equals(x.LK.Trim(), lk, StringComparison.OrdinalIgnoreCase));

            LbAutori.SelectedItem = null;
            UpdateAutorUi();
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

            if (Keyboard.FocusedElement is ComboBox cb && cb.IsDropDownOpen)
            {
                cb.IsDropDownOpen = false;
                e.Handled = true;
                return;
            }

            e.Handled = true;
            if (Keyboard.FocusedElement is UIElement el)
                el.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
        private void RefreshAutoriList()
        {
            LbAutori.ItemsSource = null;
            LbAutori.ItemsSource = _izabraniAutori;
            UpdateAutorUi();
        }
        private void LbAutori_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnRemoveAutor.IsEnabled = (LbAutori.SelectedItem is Autor);
        }
    }
}
