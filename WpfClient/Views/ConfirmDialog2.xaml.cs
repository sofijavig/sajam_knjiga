using System.Windows;
using System.Windows.Input;

namespace WpfClient.Views
{
    public partial class ConfirmDialog2 : Window
    {
        public ConfirmDialog2(string title, string message, string details = "")
        {
            InitializeComponent();

            var fallbackTitle = Application.Current.Resources["Confirm2_TitleDefault"] as string ?? "Confirmation";

            TxtTitle.Text = string.IsNullOrWhiteSpace(title) ? fallbackTitle : title;
            TxtMessage.Text = message ?? string.Empty;
            TxtDetails.Text = details ?? string.Empty;

            TxtDetails.Visibility = string.IsNullOrWhiteSpace(details)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
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
                DialogResult = true;
                Close();
                e.Handled = true;
            }
        }
    }
}