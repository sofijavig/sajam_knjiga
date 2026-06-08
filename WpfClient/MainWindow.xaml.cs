using Core.DAO;
using Core.Models;
using Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WpfClient.Views;

namespace WpfClient
{
    public partial class MainWindow : Window
    {
        private readonly KnjigaService _knjigaService = new KnjigaService();
        private readonly PosetilacServices _posetilacServices = new PosetilacServices();
        private readonly AutorService _autorService = new AutorService();
        private readonly IzdavacServices _izdavacServices = new IzdavacServices();

        private DispatcherTimer? _timer;

        private const int PageSize = 16;

        // AUTORI state
        private List<Autor> _autoriAll = new();
        private List<Autor> _autoriFiltered = new();
        private int _autoriPageIndex = 0;
        private string? _autoriSortMember = null;
        private ListSortDirection _autoriSortDir = ListSortDirection.Ascending;

        // KNJIGE state
        private List<Knjiga> _knjigeAll = new();
        private List<Knjiga> _knjigeFiltered = new();
        private int _knjigePageIndex = 0;
        private string? _knjigeSortMember = null;
        private ListSortDirection _knjigeSortDir = ListSortDirection.Ascending;

        // IZDAVAČI state
        private List<Izdavac> _izdavaciAll = new();
        private List<Izdavac> _izdavaciFiltered = new();

        // POSETIOCI state
        private List<Posetilac> _posetiociAll = new();
        private List<Posetilac> _posetiociFiltered = new();
        private int _posetiociPageIndex = 0;
        private string? _posetiociSortMember = null;
        private ListSortDirection _posetiociSortDir = ListSortDirection.Ascending;

        public MainWindow()
        {
            InitializeComponent();
            ApplyLocalizedGridHeaders();
            ApplyLocalizedStaticTexts(); // ✅ header/hint za izdavače + ostalo što je u CS

            StartClock();
            MainTabs.SelectionChanged += (s, e) => UpdateTabStatus();
            UpdateTabStatus();

            Width = SystemParameters.WorkArea.Width * 0.75;
            Height = SystemParameters.WorkArea.Height * 0.75;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            RegisterAccelerators();
            LoadAll();
        }

        // ---------------- LOCALIZATION HELPERS ----------------

        private string T(string key)
            => (Application.Current.TryFindResource(key) as string) ?? key;

        private string Tf(string key, params object[] args)
            => string.Format(T(key), args);

        private void ApplyLocalizedStaticTexts()
        {
            // Desni panel izdavača (kad ništa nije selektovano)
            if (TxtAutoriIzdavacaHeader != null)
                TxtAutoriIzdavacaHeader.Text = T("PublishersAuthorsHeader");

            if (TxtAutoriIzdavacaHint != null)
                TxtAutoriIzdavacaHint.Text = T("PublishersAuthorsHint");
        }

        // ---------------- ACCELERATORS ----------------
        private void RegisterAccelerators()
        {
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => BtnDodaj_Click(this, new RoutedEventArgs())),
                new KeyGesture(Key.N, ModifierKeys.Control)));

            InputBindings.Add(new KeyBinding(new RelayCommand(_ => BtnIzmeni_Click(this, new RoutedEventArgs())),
                new KeyGesture(Key.E, ModifierKeys.Control)));

            InputBindings.Add(new KeyBinding(new RelayCommand(_ => BtnObrisi_Click(this, new RoutedEventArgs())),
                new KeyGesture(Key.Delete)));

            InputBindings.Add(new KeyBinding(new RelayCommand(_ => BtnOsvezi_Click(this, new RoutedEventArgs())),
                new KeyGesture(Key.R, ModifierKeys.Control)));

            InputBindings.Add(new KeyBinding(new RelayCommand(_ =>
            {
                TbSearch?.Focus();
                TbSearch?.SelectAll();
            }), new KeyGesture(Key.F, ModifierKeys.Control)));

            InputBindings.Add(new KeyBinding(new RelayCommand(_ => Close()),
                new KeyGesture(Key.Q, ModifierKeys.Control)));
        }

        private sealed class RelayCommand : ICommand
        {
            private readonly Action<object?> _execute;
            private readonly Func<object?, bool>? _canExecute;

            public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
            {
                _execute = execute;
                _canExecute = canExecute;
            }

            public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
            public void Execute(object? parameter) => _execute(parameter);

            public event EventHandler? CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }
        }

        // ---------------- HELPERS ----------------

        private static (int Godina, int Broj) ParseCkKljuc(string? ck)
        {
            if (string.IsNullOrWhiteSpace(ck)) return (0, 0);

            var parts = ck.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) return (0, 0);

            int broj = 0;
            int godina = 0;

            int.TryParse(parts[1], out broj);
            int.TryParse(parts[2], out godina);

            return (godina, broj);
        }

        private void ShowInfo(string message)
        {
            var dlg = new InfoDialog(message) { Owner = this };
            dlg.ShowDialog();
        }

        private bool ConfirmDelete(string title, string message, string details)
        {
            var dlg = new ConfirmDialog(title, message, details) { Owner = this };
            return dlg.ShowDialog() == true;
        }

        private string ActiveTabHeader()
        {
            var header = (MainTabs.SelectedItem as TabItem)?.Header?.ToString() ?? "";

            return header switch
            {
                // Visitors
                "Visitors" => "Posetioci",
                "Посетиоци" => "Posetioci",

                // Authors
                "Authors" => "Autori",
                "Аутори" => "Autori",

                // Books
                "Books" => "Knjige",
                "Књиге" => "Knjige",

                // Publishers
                "Publishers" => "Izdavači",
                "Издавачи" => "Izdavači",

                _ => header
            };
        }

        private void ApplyLocalizedGridHeaders()
        {
            // POSETIOCI
            if (dgPosetioci?.Columns?.Count >= 5)
            {
                dgPosetioci.Columns[0].Header = T("ColVisitorsCardNumber");
                dgPosetioci.Columns[1].Header = T("ColFirstName");
                dgPosetioci.Columns[2].Header = T("ColLastName");
                dgPosetioci.Columns[3].Header = T("ColAddress");
                dgPosetioci.Columns[4].Header = T("ColStatus");
            }

            // AUTORI
            if (dgAutori?.Columns?.Count >= 5)
            {
                dgAutori.Columns[0].Header = T("ColFirstName");
                dgAutori.Columns[1].Header = T("ColLastName");
                dgAutori.Columns[2].Header = T("ColAuthorsIdCard");
                dgAutori.Columns[3].Header = T("ColAuthorsBirthDate");
                dgAutori.Columns[4].Header = T("ColEmail");
            }

            // KNJIGE
            if (dgKnjige?.Columns?.Count >= 5)
            {
                dgKnjige.Columns[0].Header = T("ColBooksISBN");
                dgKnjige.Columns[1].Header = T("ColBooksTitle");
                dgKnjige.Columns[2].Header = T("ColBooksPrice");
                dgKnjige.Columns[3].Header = T("ColBooksYear");
                dgKnjige.Columns[4].Header = T("ColBooksGenre");
            }

            // IZDAVACI (levo)
            if (dgIzdavaci?.Columns?.Count >= 1)
            {
                dgIzdavaci.Columns[0].Header = T("ColPublishersName");
            }

            // AUTORI IZDAVACA (desno)
            if (dgAutoriIzdavaca?.Columns?.Count >= 3)
            {
                dgAutoriIzdavaca.Columns[0].Header = T("ColFirstName");
                dgAutoriIzdavaca.Columns[1].Header = T("ColLastName");
                dgAutoriIzdavaca.Columns[2].Header = T("ColEmail");
            }
        }

        private void SetStatus(string text)
        {
            if (StatusLeft != null)
                StatusLeft.Text = text;
        }

        private void StartClock()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) =>
            {
                if (StatusRight != null)
                    StatusRight.Text = DateTime.Now.ToString("HH:mm dd.MM.yyyy");
            };
            _timer.Start();
        }

        private void UpdateTabStatus()
        {
            if (MainTabs.SelectedItem is TabItem tab && tab.Header != null && StatusLeft != null)
                StatusLeft.Text = $"{Title} - {tab.Header}";
        }

        // ---------------- LOAD ----------------
        private void LoadAll()
        {
            LoadKnjige();
            LoadPosetioci();
            LoadAutori();
            LoadIzdavaci();
        }

        private void LoadKnjige()
        {
            _knjigeAll = _knjigaService.GetAll() ?? new List<Knjiga>();
            _knjigeFiltered = _knjigeAll.ToList();
            _knjigePageIndex = 0;
            ApplyKnjigeView();
        }

        private void LoadPosetioci()
        {
            _posetiociAll = _posetilacServices.VratiSve() ?? new List<Posetilac>();
            _posetiociFiltered = _posetiociAll.ToList();
            _posetiociPageIndex = 0;
            ApplyPosetiociView();
        }

        private void LoadAutori()
        {
            _autoriAll = _autorService.GetAll() ?? new List<Autor>();
            _autoriFiltered = _autoriAll.ToList();
            _autoriPageIndex = 0;
            ApplyAutoriView();
        }

        private void LoadIzdavaci()
        {
            _izdavaciAll = _izdavacServices.VratiSve();
            _izdavaciFiltered = new List<Izdavac>(_izdavaciAll);

            dgIzdavaci.ItemsSource = null;
            dgIzdavaci.ItemsSource = _izdavaciFiltered;

            dgAutoriIzdavaca.ItemsSource = null;

            _izdavacServices.SyncSveIzKnjiga();
            _izdavaciAll = _izdavacServices.VratiSve();

            // reset desnog panela tekstova (lokalizovano)
            ApplyLocalizedStaticTexts();
        }

        // Klik na izdavača -> prikaži njegove autore desno
        private void DgIzdavaci_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgIzdavaci.SelectedItem is not Izdavac izd)
                {
                    dgAutoriIzdavaca.ItemsSource = null;

                    if (TxtAutoriIzdavacaHeader != null)
                        TxtAutoriIzdavacaHeader.Text = T("PublishersAuthorsHeader");

                    if (TxtAutoriIzdavacaHint != null)
                        TxtAutoriIzdavacaHint.Text = T("PublishersAuthorsHint");

                    return;
                }

                var sviAutori = _autorService.GetAll() ?? new List<Autor>();

                var lks = (izd.SpisakAutora ?? new List<Autor>())
                    .Where(a => !string.IsNullOrWhiteSpace(a.LK))
                    .Select(a => a.LK.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var autoriZaPrikaz = sviAutori
                    .Where(a => a != null && lks.Contains((a.LK ?? "").Trim()))
                    .OrderBy(a => a.Prezime)
                    .ThenBy(a => a.Ime)
                    .ToList();

                dgAutoriIzdavaca.ItemsSource = null;
                dgAutoriIzdavaca.ItemsSource = autoriZaPrikaz;

                if (TxtAutoriIzdavacaHeader != null)
                    TxtAutoriIzdavacaHeader.Text = Tf("PublishersAuthorsHeaderFor", izd.Naziv ?? "");

                if (TxtAutoriIzdavacaHint != null)
                {
                    TxtAutoriIzdavacaHint.Text = autoriZaPrikaz.Count == 0
                        ? T("PublishersAuthorsNone")
                        : Tf("PublishersAuthorsTotal", autoriZaPrikaz.Count);
                }
            }
            catch (Exception ex)
            {
                ShowInfo(Tf("ErrShowPublishersAuthors", ex.Message));
            }
        }

        // ---------------- MENU ----------------
        private void MenuNew_Click(object sender, RoutedEventArgs e) => BtnDodaj_Click(sender, e);

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var knjige = _knjigaService.GetAll() ?? new List<Knjiga>();
                new Core.DAO.KnjigaDAO().Save(knjige);

                _izdavacServices.SyncSveIzKnjiga();
                LoadAll();

                ShowInfo(T("MsgAppSaved"));
                SetStatus(T("StatusSaved"));
            }
            catch (Exception ex)
            {
                ShowInfo(Tf("ErrSave", ex.Message));
                SetStatus(T("StatusSaveError"));
            }
        }

        private void MenuOpenPosetioci_Click(object sender, RoutedEventArgs e) => MainTabs.SelectedIndex = 0;
        private void MenuOpenAutori_Click(object sender, RoutedEventArgs e) => MainTabs.SelectedIndex = 1;
        private void MenuOpenKnjige_Click(object sender, RoutedEventArgs e) => MainTabs.SelectedIndex = 2;
        private void MenuOpenIzdavaci_Click(object sender, RoutedEventArgs e) => MainTabs.SelectedIndex = 3;

        private void MenuExit_Click(object sender, RoutedEventArgs e) => Close();

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutWindow
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            about.ShowDialog(); // modalno otvara AboutWindow
        }

        // ---------------- TOOLBAR ----------------
        private void BtnOsvezi_Click(object sender, RoutedEventArgs e)
        {
            var tab = ActiveTabHeader();

            if (tab == "Knjige") { LoadKnjige(); SetStatus(T("StatusBooksRefreshed")); }
            else if (tab == "Posetioci") { LoadPosetioci(); SetStatus(T("StatusVisitorsRefreshed")); }
            else if (tab == "Autori") { LoadAutori(); SetStatus(T("StatusAuthorsRefreshed")); }
            else if (tab == "Izdavači") { LoadIzdavaci(); SetStatus(T("StatusPublishersRefreshed")); }
        }

        private void BtnDodaj_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tab = ActiveTabHeader();

                if (tab == "Knjige")
                {
                    var autori = _autorService.GetAll() ?? new List<Autor>();
                    var dlg = new KnjigaDialog(autori, null) { Owner = this };
                    if (dlg.ShowDialog() == true && dlg.Result != null)
                    {
                        _knjigaService.AddKnjiga(dlg.Result);
                        LoadKnjige();
                        LoadIzdavaci();
                        SetStatus(T("StatusBookAdded"));
                    }
                    return;
                }

                if (tab == "Posetioci")
                {
                    var knjige = _knjigaService.GetAll();
                    var dlg = new PosetilacDialog(knjige) { Owner = this };

                    if (dlg.ShowDialog() == true && dlg.Result != null)
                    {
                        _posetilacServices.Dodaj(dlg.Result);
                        LoadPosetioci();
                        SetStatus(T("StatusVisitorAdded"));
                    }
                    return;
                }

                if (tab == "Autori")
                {
                    var knjige = _knjigaService.GetAll();
                    var dlg = new AutorDialog(knjige) { Owner = this };

                    if (dlg.ShowDialog() == true && dlg.Result != null)
                    {
                        _autorService.AddAutor(dlg.Result);
                        _knjigaService.PostaviKnjigeAutoru(dlg.Result.LK, dlg.Result.Knjige);
                        LoadAutori();
                        LoadKnjige();
                        SetStatus(T("StatusAuthorAdded"));
                    }
                    return;
                }

                if (tab == "Izdavači")
                {
                    ShowInfo(T("MsgPublishersDerivedFromBooks"));
                    return;
                }
            }
            catch (Exception ex)
            {
                ShowInfo(Tf("ErrAdd", ex.Message));
                SetStatus(T("StatusAddError"));
            }
        }

        private void BtnIzmeni_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tab = ActiveTabHeader();

                if (tab == "Knjige")
                {
                    if (dgKnjige.SelectedItem is not Knjiga selektovana)
                    {
                        ShowInfo(T("MsgSelectBookFromTable"));
                        return;
                    }

                    var autori = _autorService.GetAll() ?? new List<Autor>();
                    var dlg = new KnjigaDialog(autori, selektovana) { Owner = this };
                    if (dlg.ShowDialog() == true && dlg.Result != null)
                    {
                        var ok = _knjigaService.UpdateKnjiga(selektovana.ISBN, dlg.Result);
                        if (!ok) ShowInfo(T("MsgBookNotFound"));

                        LoadKnjige();
                        LoadIzdavaci();
                        SetStatus(T("StatusBookUpdated"));
                    }
                    return;
                }

                if (tab == "Posetioci")
                {
                    if (dgPosetioci.SelectedItem is not Posetilac p)
                    {
                        ShowInfo(T("MsgSelectVisitorFromTable"));
                        return;
                    }

                    var knjige = _knjigaService.GetAll();
                    var dlg = new PosetilacDialog(knjige, p) { Owner = this };
                    if (dlg.ShowDialog() == true && dlg.Result != null)
                    {
                        _posetilacServices.Izmeni(p.CK, dlg.Result);
                        LoadPosetioci();
                        SetStatus(T("StatusVisitorUpdated"));
                    }
                    return;
                }

                if (tab == "Autori")
                {
                    if (dgAutori.SelectedItem is not Autor a)
                    {
                        ShowInfo(T("MsgSelectAuthorFromTable"));
                        return;
                    }

                    var knjige = _knjigaService.GetAll();
                    var dlg = new AutorDialog(knjige, a) { Owner = this };

                    if (dlg.ShowDialog() == true && dlg.Result != null)
                    {
                        var ok = _autorService.UpdateAutor(a.LK, dlg.Result);
                        if (!ok) ShowInfo(T("MsgAuthorNotFound"));

                        _knjigaService.PostaviKnjigeAutoru(a.LK, dlg.Result.Knjige);
                        LoadAutori();
                        LoadKnjige();
                        SetStatus(T("StatusAuthorUpdated"));
                    }
                    return;
                }

                if (tab == "Izdavači")
                {
                    ShowInfo(T("MsgPublisherEditViaBook"));
                    return;
                }
            }
            catch (Exception ex)
            {
                ShowInfo(Tf("ErrUpdate", ex.Message));
                SetStatus(T("StatusUpdateError"));
            }
        }

        private void BtnObrisi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tab = ActiveTabHeader();

                if (tab == "Knjige")
                {
                    if (dgKnjige.SelectedItem is not Knjiga k)
                    {
                        ShowInfo(T("MsgSelectBookFromTable"));
                        return;
                    }

                    if (!ConfirmDelete(
                        T("DlgDeleteBookTitle"),
                        T("DlgDeleteBookMessage"),
                        Tf("DlgDeleteBookDetails", k.Naziv ?? "", k.ISBN ?? "")))
                        return;

                    _knjigaService.DeleteKnjiga(k.ISBN);
                    LoadKnjige();
                    LoadIzdavaci();
                    SetStatus(T("StatusBookDeleted"));
                    return;
                }

                if (tab == "Posetioci")
                {
                    if (dgPosetioci.SelectedItem is not Posetilac p)
                    {
                        ShowInfo(T("MsgSelectVisitorFromTable"));
                        return;
                    }

                    if (!ConfirmDelete(
                        T("DlgDeleteVisitorTitle"),
                        T("DlgDeleteVisitorMessage"),
                        Tf("DlgDeleteVisitorDetails", p.Ime ?? "", p.Prezime ?? "", p.CK ?? "")))
                        return;

                    _posetilacServices.Obrisi(p.CK);
                    LoadPosetioci();
                    SetStatus(T("StatusVisitorDeleted"));
                    return;
                }

                if (tab == "Autori")
                {
                    if (dgAutori.SelectedItem is not Autor a)
                    {
                        ShowInfo(T("MsgSelectAuthorFromTable"));
                        return;
                    }

                    if (!ConfirmDelete(
                        T("DlgDeleteAuthorTitle"),
                        T("DlgDeleteAuthorMessage"),
                        Tf("DlgDeleteAuthorDetails", a.Ime ?? "", a.Prezime ?? "", a.LK ?? "")))
                        return;

                    _autorService.DeleteAutor(a.LK);
                    _knjigaService.PostaviKnjigeAutoru(a.LK, new List<Knjiga>());
                    LoadAutori();
                    LoadKnjige();
                    SetStatus(T("StatusAuthorDeleted"));
                    return;
                }

                if (tab == "Izdavači")
                {
                    ShowInfo(T("MsgPublishersDerivedDeleteHint"));
                    return;
                }
            }
            catch (Exception ex)
            {
                ShowInfo(Tf("ErrDelete", ex.Message));
                SetStatus(T("StatusDeleteError"));
            }
        }

        // ---------------- SEARCH HELPERS ----------------
        private static string[] ParseQueryParts(string q, int maxParts)
        {
            if (string.IsNullOrWhiteSpace(q)) return Array.Empty<string>();

            return q.Split(',')
                    .Select(p => (p ?? "").Trim())
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Take(maxParts)
                    .ToArray();
        }

        private static bool ContainsCI(string? source, string part)
        {
            source ??= "";
            return source.Contains(part, StringComparison.OrdinalIgnoreCase);
        }

        // ---------------- SEARCH ----------------
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            var q = (TbSearch.Text ?? "").Trim();
            var tab = ActiveTabHeader();

            if (string.IsNullOrWhiteSpace(q))
            {
                if (tab == "Knjige")
                {
                    _knjigeFiltered = _knjigeAll.ToList();
                    _knjigePageIndex = 0;
                    ApplyKnjigeView();
                    return;
                }
                if (tab == "Autori")
                {
                    _autoriFiltered = _autoriAll.ToList();
                    _autoriPageIndex = 0;
                    ApplyAutoriView();
                    return;
                }
                if (tab == "Izdavači")
                {
                    _izdavaciFiltered = _izdavaciAll
                        .Where(i => (i.Naziv ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)
                                 || (i.Sifra ?? "").Contains(q, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    dgIzdavaci.ItemsSource = null;
                    dgIzdavaci.ItemsSource = _izdavaciFiltered;

                    dgAutoriIzdavaca.ItemsSource = null;
                    ApplyLocalizedStaticTexts();
                    return;
                }
                if (tab == "Posetioci")
                {
                    _posetiociFiltered = _posetiociAll.ToList();
                    _posetiociPageIndex = 0;
                    ApplyPosetiociView();
                    return;
                }

                BtnOsvezi_Click(sender, e);
                return;
            }

            if (tab == "Knjige")
            {
                var term = q.Trim();

                _knjigeFiltered = _knjigeAll
                    .Where(k => ContainsCI(k.Naziv, term) || ContainsCI(k.ISBN, term))
                    .ToList();

                _knjigePageIndex = 0;
                ApplyKnjigeView();
                SetStatus(T("StatusSearchBooks"));
                return;
            }

            if (tab == "Posetioci")
            {
                _posetiociFiltered = _posetilacServices.Pretrazi(q);
                _posetiociPageIndex = 0;
                ApplyPosetiociView();
                SetStatus(T("StatusSearchVisitors"));
                return;
            }

            if (tab == "Autori")
            {
                var parts = ParseQueryParts(q, 2);

                if (parts.Length == 0)
                {
                    _autoriFiltered = _autoriAll.ToList();
                }
                else if (parts.Length == 1)
                {
                    string p0 = parts[0];
                    _autoriFiltered = _autoriAll.Where(a => ContainsCI(a.Prezime, p0)).ToList();
                }
                else
                {
                    string p0 = parts[0];
                    string p1 = parts[1];
                    _autoriFiltered = _autoriAll.Where(a => ContainsCI(a.Prezime, p0) && ContainsCI(a.Ime, p1)).ToList();
                }

                _autoriPageIndex = 0;
                ApplyAutoriView();
                SetStatus(T("StatusSearchAuthors"));
                return;
            }

            if (tab == "Izdavači")
            {
                _izdavaciFiltered = _izdavaciAll
                    .Where(i =>
                        ((i.Naziv ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                        ((i.Sifra ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                dgIzdavaci.ItemsSource = null;
                dgIzdavaci.ItemsSource = _izdavaciFiltered;

                dgAutoriIzdavaca.ItemsSource = null;
                ApplyLocalizedStaticTexts();
                return;
            }
        }

        private void TbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BtnSearch_Click(sender, e);
        }

        // ---------------- APPLY VIEW  ----------------

        private void ApplyPosetiociView()
        {
            IEnumerable<Posetilac> q = _posetiociFiltered;

            q = (_posetiociSortMember, _posetiociSortDir) switch
            {
                ("CK", ListSortDirection.Ascending) => q.OrderBy(p => ParseCkKljuc(p.CK)),
                ("CK", ListSortDirection.Descending) => q.OrderByDescending(p => ParseCkKljuc(p.CK)),

                ("Ime", ListSortDirection.Ascending) => q.OrderBy(p => p.Ime),
                ("Ime", ListSortDirection.Descending) => q.OrderByDescending(p => p.Ime),

                ("Prezime", ListSortDirection.Ascending) => q.OrderBy(p => p.Prezime),
                ("Prezime", ListSortDirection.Descending) => q.OrderByDescending(p => p.Prezime),

                ("GodC", ListSortDirection.Ascending) => q.OrderBy(p => p.GodC),
                ("GodC", ListSortDirection.Descending) => q.OrderByDescending(p => p.GodC),

                ("Status", ListSortDirection.Ascending) => q.OrderBy(p => p.Status),
                ("Status", ListSortDirection.Descending) => q.OrderByDescending(p => p.Status),

                ("ProsecnaOcena", ListSortDirection.Ascending) => q.OrderBy(p => p.ProsecnaOcena),
                ("ProsecnaOcena", ListSortDirection.Descending) => q.OrderByDescending(p => p.ProsecnaOcena),

                _ => q
            };

            int total = q.Count();
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            _posetiociPageIndex = Math.Clamp(_posetiociPageIndex, 0, totalPages - 1);

            var page = q.Skip(_posetiociPageIndex * PageSize).Take(PageSize).ToList();
            dgPosetioci.ItemsSource = page;

            if (TxtPosetiociPage != null)
                TxtPosetiociPage.Text = Tf("PageOfFormat", _posetiociPageIndex + 1, totalPages);
            if (BtnPosetiociPrev != null) BtnPosetiociPrev.IsEnabled = _posetiociPageIndex > 0;
            if (BtnPosetiociNext != null) BtnPosetiociNext.IsEnabled = _posetiociPageIndex < totalPages - 1;
        }

        private void ApplyAutoriView()
        {
            IEnumerable<Autor> q = _autoriFiltered;

            q = (_autoriSortMember, _autoriSortDir) switch
            {
                ("Ime", ListSortDirection.Ascending) => q.OrderBy(a => a.Ime),
                ("Ime", ListSortDirection.Descending) => q.OrderByDescending(a => a.Ime),

                ("Prezime", ListSortDirection.Ascending) => q.OrderBy(a => a.Prezime),
                ("Prezime", ListSortDirection.Descending) => q.OrderByDescending(a => a.Prezime),

                ("Mail", ListSortDirection.Ascending) => q.OrderBy(a => a.Mail),
                ("Mail", ListSortDirection.Descending) => q.OrderByDescending(a => a.Mail),

                _ => q
            };

            int total = q.Count();
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            _autoriPageIndex = Math.Clamp(_autoriPageIndex, 0, totalPages - 1);

            var page = q.Skip(_autoriPageIndex * PageSize).Take(PageSize).ToList();
            dgAutori.ItemsSource = page;

            if (TxtAutoriPage != null)
                TxtAutoriPage.Text = Tf("PageOfFormat", _autoriPageIndex + 1, totalPages);

            if (BtnAutoriPrev != null) BtnAutoriPrev.IsEnabled = _autoriPageIndex > 0;
            if (BtnAutoriNext != null) BtnAutoriNext.IsEnabled = _autoriPageIndex < totalPages - 1;
        }

        private void ApplyKnjigeView()
        {
            IEnumerable<Knjiga> q = _knjigeFiltered;

            q = (_knjigeSortMember, _knjigeSortDir) switch
            {
                ("ISBN", ListSortDirection.Ascending) => q.OrderBy(k => k.ISBN),
                ("ISBN", ListSortDirection.Descending) => q.OrderByDescending(k => k.ISBN),

                ("Naziv", ListSortDirection.Ascending) => q.OrderBy(k => k.Naziv),
                ("Naziv", ListSortDirection.Descending) => q.OrderByDescending(k => k.Naziv),

                ("Cena", ListSortDirection.Ascending) => q.OrderBy(k => k.Cena),
                ("Cena", ListSortDirection.Descending) => q.OrderByDescending(k => k.Cena),

                ("GodinaIzdanja", ListSortDirection.Ascending) => q.OrderBy(k => k.GodinaIzdanja),
                ("GodinaIzdanja", ListSortDirection.Descending) => q.OrderByDescending(k => k.GodinaIzdanja),

                ("Zanr", ListSortDirection.Ascending) => q.OrderBy(k => k.Zanr),
                ("Zanr", ListSortDirection.Descending) => q.OrderByDescending(k => k.Zanr),

                _ => q
            };

            int total = q.Count();
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            _knjigePageIndex = Math.Clamp(_knjigePageIndex, 0, totalPages - 1);

            var page = q.Skip(_knjigePageIndex * PageSize).Take(PageSize).ToList();
            dgKnjige.ItemsSource = page;

            if (TxtKnjigePage != null)
                TxtKnjigePage.Text = Tf("PageOfFormat", _knjigePageIndex + 1, totalPages);

            if (BtnKnjigePrev != null) BtnKnjigePrev.IsEnabled = _knjigePageIndex > 0;
            if (BtnKnjigeNext != null) BtnKnjigeNext.IsEnabled = _knjigePageIndex < totalPages - 1;
        }

        // ---------------- SORTIRANJE  ----------------

        private void DgPosetioci_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            string member = e.Column.SortMemberPath;
            if (string.IsNullOrWhiteSpace(member))
                return;

            if (_posetiociSortMember == member)
                _posetiociSortDir = _posetiociSortDir == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            else
            {
                _posetiociSortMember = member;
                _posetiociSortDir = ListSortDirection.Ascending;
            }

            foreach (var col in dgPosetioci.Columns)
                col.SortDirection = null;

            e.Column.SortDirection = _posetiociSortDir;

            _posetiociPageIndex = 0;
            ApplyPosetiociView();
        }

        private void DgAutori_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            string member = e.Column.SortMemberPath;
            if (string.IsNullOrWhiteSpace(member))
                return;

            if (_autoriSortMember == member)
                _autoriSortDir = _autoriSortDir == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            else
            {
                _autoriSortMember = member;
                _autoriSortDir = ListSortDirection.Ascending;
            }

            foreach (var col in dgAutori.Columns)
                col.SortDirection = null;

            e.Column.SortDirection = _autoriSortDir;

            _autoriPageIndex = 0;
            ApplyAutoriView();
        }

        private void DgKnjige_Sorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;

            string member = e.Column.SortMemberPath;
            if (string.IsNullOrWhiteSpace(member))
                return;

            if (_knjigeSortMember == member)
                _knjigeSortDir = _knjigeSortDir == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            else
            {
                _knjigeSortMember = member;
                _knjigeSortDir = ListSortDirection.Ascending;
            }

            foreach (var col in dgKnjige.Columns)
                col.SortDirection = null;

            e.Column.SortDirection = _knjigeSortDir;

            _knjigePageIndex = 0;
            ApplyKnjigeView();
        }

        // ---------------- DUGMAD ZA PAGINACIJU ----------------
        private void BtnAutoriPrev_Click(object sender, RoutedEventArgs e)
        {
            _autoriPageIndex--;
            ApplyAutoriView();
        }

        private void BtnAutoriNext_Click(object sender, RoutedEventArgs e)
        {
            _autoriPageIndex++;
            ApplyAutoriView();
        }

        private void BtnKnjigePrev_Click(object sender, RoutedEventArgs e)
        {
            _knjigePageIndex--;
            ApplyKnjigeView();
        }

        private void BtnKnjigeNext_Click(object sender, RoutedEventArgs e)
        {
            _knjigePageIndex++;
            ApplyKnjigeView();
        }

        private void BtnPosetiociPrev_Click(object sender, RoutedEventArgs e)
        {
            _posetiociPageIndex--;
            ApplyPosetiociView();
        }

        private void BtnPosetiociNext_Click(object sender, RoutedEventArgs e)
        {
            _posetiociPageIndex++;
            ApplyPosetiociView();
        }

        // ---------------- FUNKCIONALNOSTI ----------------

        // Autori sa liste želja posetioca
        private void MenuWishAuthors_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ActiveTabHeader() != "Posetioci")
                {
                    ShowInfo(T("MsgFeatureOnlyVisitorsTab"));
                    return;
                }

                if (dgPosetioci.SelectedItem is not Posetilac p)
                {
                    ShowInfo(T("MsgMarkVisitorInTable"));
                    return;
                }

                var wishIsbns = new HashSet<string>(
                    (p.ListaZelja ?? new List<Knjiga>())
                        .Where(k => k != null)
                        .Select(k => (k.ISBN ?? "").Trim()),
                    StringComparer.OrdinalIgnoreCase);

                if (wishIsbns.Count == 0)
                {
                    ShowInfo(T("MsgVisitorHasNoWishlist"));
                    return;
                }

                var sviAutori = _autorService.GetAll() ?? new List<Autor>();

                var autoriZaPrikaz = sviAutori
                    .Where(a => a != null && a.Knjige != null && a.Knjige.Any(k => wishIsbns.Contains((k.ISBN ?? "").Trim())))
                    .GroupBy(a => (a.LK ?? "").Trim(), StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .OrderBy(a => a.Prezime)
                    .ThenBy(a => a.Ime)
                    .ToList();

                var header = Tf("WishlistAuthorsHeader", p.Ime ?? "", p.Prezime ?? "", p.CK ?? "");
                var dlg = new AutoriSaListeZeljaDialog(header, autoriZaPrikaz) { Owner = this };
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowInfo(Tf("ErrGeneric", ex.Message));
            }
        }

        private void BtnPostaviSefa_Click(object sender, RoutedEventArgs e)
        {
            if (dgIzdavaci.SelectedItem is not Izdavac izd)
            {
                new ErrorDialog(T("MsgSelectPublisher")) { Owner = this }.ShowDialog();
                return;
            }

            if (dgAutoriIzdavaca.SelectedItem is not Autor autor)
            {
                new ErrorDialog(T("MsgSelectAuthorInTable")) { Owner = this }.ShowDialog();
                return;
            }

            var confirm = new ConfirmDialog2(
                T("DlgSetBossTitle"),
                T("DlgSetBossMessage"),
                Tf("DlgSetBossDetails", izd.Naziv ?? "", autor.Ime ?? "", autor.Prezime ?? "", autor.LK ?? "")
            )
            { Owner = this };

            if (confirm.ShowDialog() != true)
                return;

            try
            {
                _izdavacServices.PostaviSefa(izd.Sifra, autor);

                new InfoDialog(T("MsgBossSetSuccess")) { Owner = this }.ShowDialog();

                LoadIzdavaci();
            }
            catch (Exception ex)
            {
                new ErrorDialog(ex.Message) { Owner = this }.ShowDialog();
            }
        }

        private void EnsureIzdavaciSeededFromData()
        {
            var postojeci = _izdavacServices.VratiSve() ?? new List<Izdavac>();
            if (postojeci.Count > 0) return;

            var knjige = _knjigaService.GetAll() ?? new List<Knjiga>();
            var autori = _autorService.GetAll() ?? new List<Autor>();

            var naziviIzdavaca = knjige
                .Select(k => (k?.Izdavac ?? "").Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();

            int nextNum = 1;

            foreach (var naziv in naziviIzdavaca)
            {
                var isbns = knjige
                    .Where(k => k != null && string.Equals((k.Izdavac ?? "").Trim(), naziv, StringComparison.OrdinalIgnoreCase))
                    .Select(k => (k.ISBN ?? "").Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (isbns.Count == 0) continue;

                var autoriIzdavaca = autori
                    .Where(a => a != null && a.Knjige != null && a.Knjige.Any(k => isbns.Contains((k.ISBN ?? "").Trim(), StringComparer.OrdinalIgnoreCase)))
                    .GroupBy(a => (a.LK ?? "").Trim(), StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();

                var sef = autoriIzdavaca.FirstOrDefault(a => a.God_Iskustva >= 5);
                if (sef == null)
                    continue;

                var novi = new Izdavac
                {
                    Sifra = $"IZD{nextNum:000}",
                    Naziv = naziv,
                    Sef = sef,
                    SpisakAutora = autoriIzdavaca
                        .Where(a => !string.IsNullOrWhiteSpace(a.LK))
                        .Select(a => new Autor { LK = a.LK })
                        .ToList(),
                    SpisakKnjiga = isbns.Select(x => new Knjiga { ISBN = x }).ToList()
                };

                try
                {
                    _izdavacServices.Dodaj(novi);
                    nextNum++;
                }
                catch
                {
                    // preskoči
                }
            }
        }

        private void MenuPosetiociZaAutora_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ActiveTabHeader() != "Autori")
                {
                    ShowInfo(T("MsgFeatureOnlyAuthorsTab"));
                    return;
                }

                if (dgAutori.SelectedItem is not Autor a)
                {
                    ShowInfo(T("MsgSelectAuthorInTable"));
                    return;
                }

                var autorIsbns = new HashSet<string>(
                    (a.Knjige ?? new List<Knjiga>())
                        .Where(k => k != null)
                        .Select(k => (k.ISBN ?? "").Trim()),
                    StringComparer.OrdinalIgnoreCase);

                if (autorIsbns.Count == 0)
                {
                    ShowInfo(T("MsgAuthorHasNoBooks"));
                    return;
                }

                var sviPosetioci = _posetilacServices.VratiSve() ?? new List<Posetilac>();

                var posetioci = sviPosetioci
                    .Where(p => p != null && p.ListaZelja != null &&
                                p.ListaZelja.Any(k => k != null && autorIsbns.Contains((k.ISBN ?? "").Trim())))
                    .GroupBy(p => (p.CK ?? "").Trim(), StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .OrderBy(p => p.Prezime)
                    .ThenBy(p => p.Ime)
                    .ToList();

                var title = Tf("DlgVisitorsForAuthorTitle", a.Ime ?? "", a.Prezime ?? "");
                var dlg = new PosetiociZaAutoraDialog(title, posetioci) { Owner = this };
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowInfo(Tf("ErrGeneric", ex.Message));
            }
        }

        private void SetLang(string dictPath)
        {
            var newDict = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/{dictPath}", UriKind.Absolute)
            };

            var merged = Application.Current.Resources.MergedDictionaries;

            for (int i = merged.Count - 1; i >= 0; i--)
            {
                var src = merged[i].Source?.ToString() ?? "";
                if (src.Contains("/Resources/Strings.", StringComparison.OrdinalIgnoreCase))
                    merged.RemoveAt(i);
            }

            merged.Add(newDict);

            ApplyLocalizedGridHeaders();
            ApplyLocalizedStaticTexts();
            UpdateTabStatus();

            ApplyPosetiociView();
            ApplyAutoriView();
            ApplyKnjigeView();
        }

        private void BtnKnjigeIzdavaca_Click(object sender, RoutedEventArgs e)
        {
            string T(string key, string fallback)
                => (TryFindResource(key) as string) ?? fallback;

            // 1️⃣ Nije selektovan izdavač
            if (dgIzdavaci.SelectedItem is not Izdavac izd)
            {
                new InfoDialog(
                    T("Err_SelectPublisher", "Izaberite izdavača.")
                )
                { Owner = this }
                .ShowDialog();

                return;
            }

            var knjDao = new KnjigaDAO();
            var sve = knjDao.Load();

            var rez = sve
                .Where(k => !string.IsNullOrWhiteSpace(k.Izdavac) &&
                            k.Izdavac.Trim()
                             .Equals(izd.Naziv.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();

            // 2️⃣ Nema knjiga
            if (rez.Count == 0)
            {
                new InfoDialog(
                    T("Info_NoBooksForPublisher", "Nema knjiga za ovog izdavača.")
                )
                { Owner = this }
                .ShowDialog();

                return;
            }

            // 3️⃣ Ima knjiga
            new IzdavacKnjigeDialog(izd.Naziv, rez)
            {
                Owner = this
            }
            .ShowDialog();
        }
        private void BtnSrLat_Click(object sender, RoutedEventArgs e) => SetLang("Resources/Strings.sr.xaml");
        private void BtnSrCir_Click(object sender, RoutedEventArgs e) => SetLang("Resources/Strings.sr-cirilica.xaml");
        private void BtnEn_Click(object sender, RoutedEventArgs e) => SetLang("Resources/Strings.en.xaml");

        private void BtnPosetiociPoDveKnjige_Click(object sender, RoutedEventArgs e)
        {
            
            var sveKnjige = _knjigaService.GetAll();
            var sviPosetioci = _posetilacServices.VratiSve();

            var dialog = new PosetiociPoDveKnjigeDialog(sveKnjige, sviPosetioci)
            {
                Owner = this
            };
            dialog.ShowDialog();
        }

    }
}