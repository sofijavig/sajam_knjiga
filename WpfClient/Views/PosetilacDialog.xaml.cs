using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.Models;
using Core.Services;
using Core.DAO;

namespace WpfClient.Views
{
    public partial class PosetilacDialog : Window
    {
        public Posetilac? Result { get; private set; }

        private readonly bool _isEdit;
        private readonly Posetilac? _existing;
        private readonly List<Knjiga> _allKnjige;

        private readonly KupovinaServices _kupovinaServices = new KupovinaServices();
        private readonly List<Kupovina> _pendingAdded = new();
        private readonly List<Kupovina> _pendingRemoved = new();
        private readonly HashSet<string> _originalKeys = new(StringComparer.OrdinalIgnoreCase);

        public PosetilacDialog(List<Knjiga> allKnjige, Posetilac? existing = null)
        {
            InitializeComponent();

            _allKnjige = allKnjige ?? new List<Knjiga>();
            _existing = existing;
            _isEdit = existing != null;

            if (_isEdit)
            {
                Title = R("EditVisitorTitle", "Edit visitor");
                LoadExisting(existing!);
                TbCK.IsEnabled = false;
                LoadKupovineFromFile(existing!.CK);
                _originalKeys.Clear();
                foreach (var kup in CurrentKupljeneList())
                    _originalKeys.Add(KupKey(kup));
            }
            else
            {
                Title = R("AddVisitorTitle", "Add visitor");

                if (DpDatRodj.SelectedDate == null)
                    DpDatRodj.SelectedDate = DateTime.Today;

                if (CbStatus.Items.Count > 0 && CbStatus.SelectedIndex < 0)
                    CbStatus.SelectedIndex = 0;

                DgKupljene.ItemsSource = new List<Kupovina>();
                DgZelje.ItemsSource = new List<Knjiga>();
            }

            RefreshAvailableBooks();
            UpdateConfirmEnabled();
            UpdateKupljeneSummary();
        }

        private string R(string key, string fallback)
            => (TryFindResource(key) as string) ?? fallback;

        private static string KupKey(Kupovina k)
        {
            var ck = k?.Pos?.CK ?? "";
            var isbn = k?.Knj?.ISBN ?? "";
            return $"{ck}|{isbn}|{k.DatKup:yyyy-MM-dd}";
        }

        private void LoadKupovineFromFile(string ck)
        {
            var kupDao = new KupovinaDAO();
            var sve = kupDao.Ucitaj();

            var njegove = sve
                .Where(x => x?.Pos != null && string.Equals(x.Pos.CK, ck, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.DatKup)
                .ToList();

            var knjLookup = _allKnjige
                .Where(k => k != null && !string.IsNullOrWhiteSpace(k.ISBN))
                .GroupBy(k => k!.ISBN.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First()!, StringComparer.OrdinalIgnoreCase);

            foreach (var kup in njegove)
            {
                if (_existing != null)
                    kup.Pos = _existing;

                var isbn = (kup.Knj?.ISBN ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(isbn) && knjLookup.TryGetValue(isbn, out var pravaKnj))
                    kup.Knj = pravaKnj;
            }

            if (_existing != null)
                _existing.Kupljene = njegove;

            DgKupljene.ItemsSource = njegove;
            DgKupljene.Items.Refresh();
            UpdateKupljeneSummary();
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

        private void LoadExisting(Posetilac p)
        {
            TbCK.Text = p.CK ?? string.Empty;
            TbIme.Text = p.Ime ?? string.Empty;
            TbPrezime.Text = p.Prezime ?? string.Empty;
            DpDatRodj.SelectedDate = p.DatRodj;

            TbTelefon.Text = p.BrojTelefona ?? string.Empty;
            TbMail.Text = p.Mail ?? string.Empty;

            TbUlica.Text = p.Ad?.Ulica ?? string.Empty;
            TbBroj.Text = p.Ad?.Broj.ToString() ?? string.Empty;
            TbGrad.Text = p.Ad?.Grad ?? string.Empty;
            TbDrzava.Text = p.Ad?.Drzava ?? string.Empty;

            SetStatusSelection(p.Status);

            p.Kupljene ??= new List<Kupovina>();
            p.ListaZelja ??= new List<Knjiga>();

            DgKupljene.ItemsSource = p.Kupljene;
            DgZelje.ItemsSource = p.ListaZelja;
        }

        private void SetStatusSelection(StatusPosetioca status)
        {
            var regular = R("Posetilac_StatusRegular", "Regular");
            var vip = R("Posetilac_StatusVip", "VIP");

            foreach (var obj in CbStatus.Items)
            {
                if (obj is ComboBoxItem cbi)
                {
                    var txt = (cbi.Content?.ToString() ?? "").Trim();

                    if ((txt.Equals(regular, StringComparison.OrdinalIgnoreCase) && status == StatusPosetioca.R) ||
                        (txt.Equals(vip, StringComparison.OrdinalIgnoreCase) && status == StatusPosetioca.V))
                    {
                        CbStatus.SelectedItem = cbi;
                        return;
                    }
                }
            }

            if (CbStatus.Items.Count > 0)
                CbStatus.SelectedIndex = 0;
        }

        private StatusPosetioca GetSelectedStatus()
        {
            var regular = R("Posetilac_StatusRegular", "Regular");
            var vip = R("Posetilac_StatusVip", "VIP");

            var txt = (CbStatus.SelectedItem as ComboBoxItem)?.Content?.ToString()?.Trim() ?? regular;
            return txt.Equals(vip, StringComparison.OrdinalIgnoreCase) ? StatusPosetioca.V : StatusPosetioca.R;
        }

        private void AnyChanged(object sender, TextChangedEventArgs e) => UpdateConfirmEnabled();
        private void AnyChanged(object sender, SelectionChangedEventArgs e) => UpdateConfirmEnabled();

        private void UpdateConfirmEnabled()
        {
            var mail = (TbMail.Text ?? "").Trim();
            bool mailOk = !string.IsNullOrWhiteSpace(mail) && mail.Contains('@');

            bool brojOk = int.TryParse((TbBroj.Text ?? "").Trim(), out int br) && br > 0;

            BtnPotvrdi.IsEnabled =
                !string.IsNullOrWhiteSpace(TbCK.Text) &&
                !string.IsNullOrWhiteSpace(TbIme.Text) &&
                !string.IsNullOrWhiteSpace(TbPrezime.Text) &&
                DpDatRodj.SelectedDate != null &&
                !string.IsNullOrWhiteSpace(TbTelefon.Text) &&
                mailOk &&
                CbStatus.SelectedItem != null &&
                !string.IsNullOrWhiteSpace(TbUlica.Text) &&
                brojOk &&
                !string.IsNullOrWhiteSpace(TbGrad.Text) &&
                !string.IsNullOrWhiteSpace(TbDrzava.Text);
        }

        private List<Knjiga> CurrentZeljeList()
        {
            if (_isEdit && _existing != null)
            {
                _existing.ListaZelja ??= new List<Knjiga>();
                return _existing.ListaZelja;
            }

            if (DgZelje.ItemsSource is List<Knjiga> l)
                return l;

            var created = new List<Knjiga>();
            DgZelje.ItemsSource = created;
            return created;
        }

        private List<Kupovina> CurrentKupljeneList()
        {
            if (_isEdit && _existing != null)
            {
                _existing.Kupljene ??= new List<Kupovina>();
                return _existing.Kupljene;
            }

            if (DgKupljene.ItemsSource is List<Kupovina> l)
                return l;

            var created = new List<Kupovina>();
            DgKupljene.ItemsSource = created;
            return created;
        }

        private void RefreshGrids()
        {
            DgZelje.Items.Refresh();
            DgKupljene.Items.Refresh();
        }

        private void RefreshAvailableBooks()
        {
            var zelja = CurrentZeljeList();
            var kupljene = CurrentKupljeneList();

            var zeljeIsbn = new HashSet<string>(
                zelja.Where(k => k != null).Select(k => (k!.ISBN ?? "").Trim()),
                StringComparer.OrdinalIgnoreCase);

            var kupljeneIsbn = new HashSet<string>(
                kupljene.Where(k => k?.Knj != null).Select(k => (k!.Knj!.ISBN ?? "").Trim()),
                StringComparer.OrdinalIgnoreCase);

            var available = _allKnjige
                .Where(k => k != null)
                .Where(k => !zeljeIsbn.Contains((k!.ISBN ?? "").Trim()) && !kupljeneIsbn.Contains((k.ISBN ?? "").Trim()))
                .ToList();

            CbSveKnjige.ItemsSource = available;
            CbSveKnjige.SelectedIndex = available.Count > 0 ? 0 : -1;
        }

        private void UpdateKupljeneSummary()
        {
            var kupljene = CurrentKupljeneList();

            if (kupljene.Count == 0)
            {
                TbProsecnaOcena.Text = R("Posetilac_SummaryAvgPlaceholder", "Average rating: -");
                TbPotroseno.Text = R("Posetilac_SummarySpentPlaceholder", "Spent: -");
                return;
            }

            double avg = kupljene.Average(k => k.Ocena);
            int sum = kupljene.Sum(k => k.Knj?.Cena ?? 0);

            TbProsecnaOcena.Text = string.Format(R("Posetilac_SummaryAvgFormat", "Average rating: {0:0.0}"), avg);
            TbPotroseno.Text = string.Format(R("Posetilac_SummarySpentFormat", "Spent: {0} RSD"), sum);
        }

        private void RefreshKupljeneUI()
        {
            DgKupljene.Items.Refresh();
            RefreshAvailableBooks();
            UpdateKupljeneSummary();
        }

        private void BtnAutoriSaZelja_Click(object sender, RoutedEventArgs e)
        {
            var zelje = CurrentZeljeList();
            if (zelje == null || zelje.Count == 0)
            {
                ShowError(R("Err_WishListEmpty", "Wish list is empty."));
                return;
            }

            var wishIsbns = new HashSet<string>(
                zelje.Select(k => (k?.ISBN ?? "").Trim()),
                StringComparer.OrdinalIgnoreCase);

            var autori = _allKnjige
                .Where(k => k != null && wishIsbns.Contains((k!.ISBN ?? "").Trim()))
                .SelectMany(k => k!.Autori ?? new List<Autor>())
                .Where(a => a != null)
                .GroupBy(a => (a!.LK ?? $"{a.Ime}|{a.Prezime}").Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First()!)
                .OrderBy(a => a.Prezime)
                .ThenBy(a => a.Ime)
                .ToList();

            if (autori.Count == 0)
            {
                ShowError(R("Err_NoAuthorsToShow", "No authors to show."));
                return;
            }

            string header = $"{TbIme.Text} {TbPrezime.Text} ({TbCK.Text})";
            var dlg = new AutoriSaListeZeljaDialog(header, autori) { Owner = this };
            dlg.ShowDialog();
        }

        private void BtnDodajZelju_Click(object sender, RoutedEventArgs e)
        {
            if (CbSveKnjige.SelectedItem is not Knjiga k)
            {
                ShowError(R("Err_SelectBookFromList", "Select a book from the list."));
                return;
            }

            var zelja = CurrentZeljeList();
            if (zelja.Any(x => string.Equals((x?.ISBN ?? ""), k.ISBN, StringComparison.OrdinalIgnoreCase)))
            {
                ShowError(R("Err_BookAlreadyInWishList", "That book is already in the wish list."));
                return;
            }

            zelja.Add(k);
            RefreshGrids();
            RefreshAvailableBooks();
        }

        private void BtnObrisiZelju_Click(object sender, RoutedEventArgs e)
        {
            if (DgZelje.SelectedItem is not Knjiga k)
            {
                ShowError(R("Err_SelectWishBookRow", "Select a book from the wish list table."));
                return;
            }

            var dlg = new ConfirmDialog2(
                R("Confirm_RemoveWishBook_Title", "Remove book"),
                R("Confirm_RemoveWishBook_Message", "Are you sure you want to remove this book from the wish list?"),
                $"{k.Naziv} ({k.ISBN})"
            )
            { Owner = this };

            if (dlg.ShowDialog() != true)
                return;

            var zelja = CurrentZeljeList();
            zelja.RemoveAll(x => string.Equals((x?.ISBN ?? ""), k.ISBN, StringComparison.OrdinalIgnoreCase));

            RefreshGrids();
            RefreshAvailableBooks();
        }

        private void BtnKupovina_Click(object sender, RoutedEventArgs e)
        {
            if (DgZelje.SelectedItem is not Knjiga k)
            {
                ShowError(R("Err_SelectWishBookRow", "Select a book from the wish list table."));
                return;
            }

            var pos = _existing ?? new Posetilac
            {
                CK = (TbCK.Text ?? "").Trim(),
                Ime = (TbIme.Text ?? "").Trim(),
                Prezime = (TbPrezime.Text ?? "").Trim()
            };

            var dlg = new KupovinaDialog(pos, k) { Owner = this };
            if (dlg.ShowDialog() != true || dlg.Result == null)
                return;

            var kupljene = CurrentKupljeneList();
            var zelja = CurrentZeljeList();

            if (kupljene.Any(x => x?.Knj != null &&
                                  string.Equals((x.Knj.ISBN ?? ""), k.ISBN, StringComparison.OrdinalIgnoreCase)))
            {
                ShowError(R("Err_BookAlreadyPurchased", "That book is already in purchased."));
                return;
            }

            kupljene.Add(dlg.Result);
            _pendingAdded.Add(dlg.Result);

            zelja.RemoveAll(x => string.Equals((x?.ISBN ?? ""), k.ISBN, StringComparison.OrdinalIgnoreCase));

            RefreshGrids();
            RefreshKupljeneUI();
        }

        private void BtnPonistiKupovinu_Click(object sender, RoutedEventArgs e)
        {
            if (DgKupljene.SelectedItem is not Kupovina kup)
            {
                ShowError(R("Err_SelectPurchasedRow", "Select a purchase from the purchased table."));
                return;
            }

            var dlg = new ConfirmDialog2(
                R("Confirm_CancelPurchase_Title", "Cancel purchase"),
                R("Confirm_CancelPurchase_Message", "Are you sure you want to cancel the purchase?"),
                $"{kup.Knj?.Naziv} ({kup.Knj?.ISBN})"
            )
            { Owner = this };

            if (dlg.ShowDialog() != true)
                return;

            // pending logika (tvoja)
            var idxNew = _pendingAdded.FindIndex(x => ReferenceEquals(x, kup));
            if (idxNew >= 0)
                _pendingAdded.RemoveAt(idxNew);
            else
            {
                if (_originalKeys.Contains(KupKey(kup)))
                    _pendingRemoved.Add(kup);
            }

            // ukloni iz kupljenih (UI)
            CurrentKupljeneList().Remove(kup);

            // ✅ vrati u listu želja (preko kup.Pos)
            if (kup.Pos != null && kup.Knj != null)
            {
                bool postoji = kup.Pos.ListaZelja.Any(x => x != null && x.ISBN == kup.Knj.ISBN);
                if (!postoji)
                    kup.Pos.ListaZelja.Add(kup.Knj);

                // ✅ snimi posetioci.txt (DAO instanca)
                var posDao = new PosetilacDAO();
                var svi = posDao.Ucitaj();

                var p = svi.FirstOrDefault(x => x.CK == kup.Pos.CK);
                if (p != null)
                {
                    p.ListaZelja = kup.Pos.ListaZelja;
                    posDao.Sacuvaj(svi);
                }
            }

            // refresh kupljenih (tvoje)
            RefreshKupljeneUI();

            // potpuno osveži ceo prozor
            DataContext = null;
            DataContext = kup.Pos;
        }
        private void BtnPotvrdi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string ck = (TbCK.Text ?? "").Trim();
                string ime = (TbIme.Text ?? "").Trim();
                string prezime = (TbPrezime.Text ?? "").Trim();
                string telefon = (TbTelefon.Text ?? "").Trim();
                string mail = (TbMail.Text ?? "").Trim();

                string ulica = (TbUlica.Text ?? "").Trim();
                string grad = (TbGrad.Text ?? "").Trim();
                string drzava = (TbDrzava.Text ?? "").Trim();

                if (string.IsNullOrWhiteSpace(ck)) { ShowError(R("Err_Visitor_CkRequired", "Card number is required."), TbCK); return; }
                if (string.IsNullOrWhiteSpace(ime)) { ShowError(R("Err_Visitor_FirstNameRequired", "First name is required."), TbIme); return; }
                if (string.IsNullOrWhiteSpace(prezime)) { ShowError(R("Err_Visitor_LastNameRequired", "Last name is required."), TbPrezime); return; }
                if (DpDatRodj.SelectedDate == null) { ShowError(R("Err_Visitor_BirthDateRequired", "Birth date is required."), DpDatRodj); return; }
                if (string.IsNullOrWhiteSpace(telefon)) { ShowError(R("Err_Visitor_PhoneRequired", "Phone is required."), TbTelefon); return; }
                if (string.IsNullOrWhiteSpace(mail) || !mail.Contains('@')) { ShowError(R("Err_Visitor_EmailInvalid", "Email is invalid."), TbMail); return; }

                if (string.IsNullOrWhiteSpace(ulica)) { ShowError(R("Err_Visitor_StreetRequired", "Street is required."), TbUlica); return; }
                if (!int.TryParse((TbBroj.Text ?? "").Trim(), out int brojUlice) || brojUlice <= 0) { ShowError(R("Err_Visitor_NumberInvalid", "Street number must be > 0."), TbBroj); return; }
                if (string.IsNullOrWhiteSpace(grad)) { ShowError(R("Err_Visitor_CityRequired", "City is required."), TbGrad); return; }
                if (string.IsNullOrWhiteSpace(drzava)) { ShowError(R("Err_Visitor_CountryRequired", "Country is required."), TbDrzava); return; }

                var status = GetSelectedStatus();

                var finalKupljene = CurrentKupljeneList();
                var finalZelje = CurrentZeljeList();

                var p = new Posetilac
                {
                    CK = _isEdit ? _existing!.CK : ck,
                    Ime = ime,
                    Prezime = prezime,
                    DatRodj = DpDatRodj.SelectedDate.Value,
                    BrojTelefona = telefon,
                    Mail = mail,
                    Status = status,
                    Ad = new Adresa
                    {
                        Ulica = ulica,
                        Broj = brojUlice,
                        Grad = grad,
                        Drzava = drzava
                    },
                    Kupljene = finalKupljene,
                    ListaZelja = finalZelje
                };

                foreach (var kup in p.Kupljene)
                    kup.Pos = p;

                p.Validacija();

                foreach (var kup in _pendingRemoved)
                    _kupovinaServices.Obrisi(p.CK, kup.Knj!.ISBN, kup.DatKup);

                foreach (var kup in _pendingAdded)
                {
                    kup.Pos = p;
                    _kupovinaServices.Dodaj(kup);
                }

                Result = p;
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

            if (Keyboard.FocusedElement is ComboBox cb && cb.IsDropDownOpen)
            {
                cb.IsDropDownOpen = false;
                e.Handled = true;
                return;
            }

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
    }
}