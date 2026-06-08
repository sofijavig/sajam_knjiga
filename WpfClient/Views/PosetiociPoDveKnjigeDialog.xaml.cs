using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace WpfClient.Views
{
    public partial class PosetiociPoDveKnjigeDialog : Window
    {
        private List<Posetilac> _sviPosetioci;
        private List<Knjiga> _sveKnjige;

        private Knjiga _prva;
        private Knjiga _druga;

        public PosetiociPoDveKnjigeDialog(List<Knjiga> sveKnjige, List<Posetilac> sviPosetioci)
        {
            InitializeComponent();

            _sveKnjige = sveKnjige ?? new List<Knjiga>();
            _sviPosetioci = sviPosetioci ?? new List<Posetilac>();

            CbPrvaKnjiga.ItemsSource = _sveKnjige;
            CbDrugaKnjiga.ItemsSource = _sveKnjige;

            if (_sveKnjige.Count >= 2)
            {
                CbPrvaKnjiga.SelectedIndex = 0;
                CbDrugaKnjiga.SelectedIndex = 1;
            }

            // inicijalni podnaslov (opciono)
            TxtSub.Text = "";
        }

        private string R(string key, string fallback)
            => (TryFindResource(key) as string) ?? fallback;

        private void BtnPrikaziPosetioce_Click(object sender, RoutedEventArgs e)
        {
            _prva = CbPrvaKnjiga.SelectedItem as Knjiga;
            _druga = CbDrugaKnjiga.SelectedItem as Knjiga;

            if (_prva == null || _druga == null)
            {
                MessageBox.Show(
                    R("Err_SelectBothBooks", "Please select both books."),
                    R("Error_WindowTitle", "Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            PrikaziPosetioce();
        }

        private void PrikaziPosetioce()
        {
            var obeKnjige = new List<Posetilac>();
            var kupiliPrvuNeDrugu = new List<Posetilac>();

            var isbn1 = (_prva?.ISBN ?? "").Trim();
            var isbn2 = (_druga?.ISBN ?? "").Trim();

            foreach (var p in _sviPosetioci)
            {
                bool naWishListiPrva = p.ListaZelja?.Any(k => string.Equals((k?.ISBN ?? "").Trim(), isbn1, StringComparison.OrdinalIgnoreCase)) ?? false;
                bool naWishListiDruga = p.ListaZelja?.Any(k => string.Equals((k?.ISBN ?? "").Trim(), isbn2, StringComparison.OrdinalIgnoreCase)) ?? false;

                bool kupioPrvu = p.Kupljene?.Any(k => string.Equals((k?.Knj?.ISBN ?? "").Trim(), isbn1, StringComparison.OrdinalIgnoreCase)) ?? false;
                bool kupioDrugu = p.Kupljene?.Any(k => string.Equals((k?.Knj?.ISBN ?? "").Trim(), isbn2, StringComparison.OrdinalIgnoreCase)) ?? false;

                if (naWishListiPrva && naWishListiDruga)
                    obeKnjige.Add(p);
                else if (kupioPrvu && !kupioDrugu)
                    kupiliPrvuNeDrugu.Add(p);
            }

            DgObeKnjige.ItemsSource = obeKnjige;
            DgKupiliPrvu.ItemsSource = kupiliPrvuNeDrugu;

            var fmt = R("VisitorsTwoBooks_SubFormat", "First book: {0}, Second book: {1}");
            TxtSub.Text = string.Format(fmt, _prva?.Naziv ?? "", _druga?.Naziv ?? "");

            // reset pretrage kad osvežiš prikaz
            TbSearch.Text = "";
        }

        private void TbSearch_KeyUp(object sender, KeyEventArgs e)
        {
            string filter = (TbSearch.Text ?? "").Trim().ToLowerInvariant();

            if (DgObeKnjige.ItemsSource is List<Posetilac> obe)
            {
                DgObeKnjige.ItemsSource = string.IsNullOrWhiteSpace(filter)
                    ? obe
                    : obe.Where(p =>
                        (p.Ime ?? "").ToLowerInvariant().Contains(filter) ||
                        (p.Prezime ?? "").ToLowerInvariant().Contains(filter) ||
                        (p.CK ?? "").ToLowerInvariant().Contains(filter))
                    .ToList();
            }

            if (DgKupiliPrvu.ItemsSource is List<Posetilac> kupili)
            {
                DgKupiliPrvu.ItemsSource = string.IsNullOrWhiteSpace(filter)
                    ? kupili
                    : kupili.Where(p =>
                        (p.Ime ?? "").ToLowerInvariant().Contains(filter) ||
                        (p.Prezime ?? "").ToLowerInvariant().Contains(filter) ||
                        (p.CK ?? "").ToLowerInvariant().Contains(filter))
                    .ToList();
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TbSearch.Text = string.Empty;
            TbSearch_KeyUp(null, null);
        }
    }
}