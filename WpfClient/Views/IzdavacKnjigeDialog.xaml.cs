using System.Collections.Generic;
using System.Windows;
using Core.Models;

namespace WpfClient.Views
{
    public partial class IzdavacKnjigeDialog : Window
    {
        public IzdavacKnjigeDialog(string nazivIzdavaca, List<Knjiga> knjige)
        {
            InitializeComponent();

            // Title: npr. "Knjige izdavača: Laguna"
            Title = $"{Title}: {nazivIzdavaca}";

            dgKnjige.ItemsSource = knjige ?? new List<Knjiga>();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}