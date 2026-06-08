using System.Windows;

namespace WpfClient
{
    public partial class InfoDialog : Window
    {
        public InfoDialog(string message)
        {
            InitializeComponent();
            TxtMessage.Text = message;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
