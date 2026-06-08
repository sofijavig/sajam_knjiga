using System;
using System.Windows;
using System.Windows.Controls;
using Core.Models;

namespace WpfClient.Views
{
    public partial class KupovinaDialog : Window
    {
        public Kupovina? Result { get; private set; }

        private readonly Posetilac _pos;
        private readonly Knjiga _knj;

        public KupovinaDialog(Posetilac pos, Knjiga knj)
        {
            InitializeComponent();

            _pos = pos;
            _knj = knj;

            TbIsbn.Text = _knj.ISBN;
            TbNaziv.Text = _knj.Naziv;

            DpDatum.SelectedDate = DateTime.Today;
            CbOcena.SelectedIndex = 0;

            UpdateConfirmEnabled();
        }

        private void CbOcena_Changed(object sender, SelectionChangedEventArgs e) => UpdateConfirmEnabled();
        private void DpDatum_Changed(object sender, SelectionChangedEventArgs e) => UpdateConfirmEnabled();

        private void UpdateConfirmEnabled()
        {
            BtnPotvrdi.IsEnabled = (CbOcena.SelectedItem != null) && (DpDatum.SelectedDate != null);
        }

        private int GetOcena()
        {
            var txt = (CbOcena.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "1";
            return int.Parse(txt);
        }

        private void BtnPotvrdi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DpDatum.SelectedDate == null) return;

                var kup = new Kupovina
                {
                    Pos = _pos,
                    Knj = _knj,
                    DatKup = DpDatum.SelectedDate.Value,
                    Ocena = GetOcena(),
                    Komentar = ""
                };

                kup.Validacija();

                Result = kup;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                var caption = (TryFindResource("Error_WindowTitle") as string) ?? "Error";
                MessageBox.Show(ex.Message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}