using System.Collections.Generic;
using System.Windows;
using Core.Models;

namespace WpfClient.Views
{
    public partial class SelectAutorDialog : Window
    {
        public Autor? SelectedAutor { get; private set; }

        public SelectAutorDialog(List<Autor> autori)
        {
            InitializeComponent();
            LbAutori.ItemsSource = autori ?? new List<Autor>();
        }

        private string R(string key, string fallback)
            => (TryFindResource(key) as string) ?? fallback;

        private void BtnPotvrdi_Click(object sender, RoutedEventArgs e)
        {
            if (LbAutori.SelectedItem is not Autor a)
            {
                new ErrorDialog(R("Err_SelectAuthorFromList", "Select an author from the list."))
                { Owner = this }.ShowDialog();
                return;
            }

            SelectedAutor = a;
            DialogResult = true;
            Close();
        }
    }
}