using System.Windows;

namespace WpfClient
{
    public partial class ConfirmDialog : Window
    {
        public ConfirmDialog(string title, string message, string details = "")
        {
            InitializeComponent();
            TbTitle.Text = title;
            TbMessage.Text = message;
            TbDetails.Text = details;
            TbDetails.Visibility = string.IsNullOrWhiteSpace(details) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
