using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace WpfClient.Views
{
    public partial class SelectKnjigaDialog : Window
    {
        public Knjiga? Selected { get; private set; }

        private readonly List<Knjiga> _all;

        public SelectKnjigaDialog(List<Knjiga> knjige)
        {
            InitializeComponent();

            _all = knjige ?? new List<Knjiga>();
            DgKnjige.ItemsSource = _all;
        }

        private void BtnPotvrdi_Click(object sender, RoutedEventArgs e)
        {
            if (DgKnjige.SelectedItem is Knjiga k)
            {
                Selected = k;
                DialogResult = true;
                Close();
                return;
            }

            // ✅ localized
            var msg = (string)Application.Current.TryFindResource("Err_SelectBookFromList")
                      ?? "Select a book from the list.";

            var dlg = new InfoDialog(msg);
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        private void DgKnjige_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BtnPotvrdi_Click(sender, e);
        }

        private void TbFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var q = (TbFilter.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(q))
            {
                DgKnjige.ItemsSource = _all;
                return;
            }

            DgKnjige.ItemsSource = _all
                .Where(k =>
                    (k?.ISBN ?? "").Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    (k?.Naziv ?? "").Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private void BtnOcisti_Click(object sender, RoutedEventArgs e)
        {
            TbFilter.Text = "";
            TbFilter.Focus();
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

            if (e.Key == Key.Enter)
            {
                BtnPotvrdi_Click(sender, new RoutedEventArgs());
                e.Handled = true;
            }
        }
    }
}