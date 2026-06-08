using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace WpfClient.Views
{
    public partial class AutoriSaListeZeljaDialog : Window
    {
        private readonly List<Autor> _all;
        private List<Autor> _filtered;

        public AutoriSaListeZeljaDialog(string posetilacHeader, List<Autor> autori)
        {
            InitializeComponent();

            _all = autori ?? new List<Autor>();
            _filtered = _all.ToList();

            TxtSub.Text = string.Format(
                            (string)Application.Current.Resources["WishAuthorsVisitorFormat"],
                            posetilacHeader);
            DgAutori.ItemsSource = _filtered;
        }

        private void ApplyFilter()
        {
            var q = (TbSearch.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(q))
            {
                _filtered = _all.ToList();
                DgAutori.ItemsSource = _filtered;
                return;
            }

            _filtered = _all
                .Where(a =>
                    (a.Ime ?? "").Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    (a.Prezime ?? "").Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    (a.Mail ?? "").Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();

            DgAutori.ItemsSource = _filtered;
        }

        private void TbSearch_KeyUp(object sender, KeyEventArgs e) => ApplyFilter();

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TbSearch.Text = "";
            ApplyFilter();
            TbSearch.Focus();
        }
    }
}
