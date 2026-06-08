using System.Windows;

namespace WpfClient.Views
{
    public partial class ConfrimDialogAutor : Window
    {
        public ConfrimDialogAutor()
        {
            InitializeComponent();
        }

        private void BtnPotvrdi_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
