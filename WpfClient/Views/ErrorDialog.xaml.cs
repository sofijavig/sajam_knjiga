using System;
using System.Windows;
using System.Windows.Input;

namespace WpfClient.Views
{
    public partial class ErrorDialog : Window
    {
        public string DialogTitle { get; set; }
        public string DialogSubtitle { get; set; }
        public string Message { get; set; }
        public string? Details { get; set; }

        public Visibility DetailsVisibility
            => string.IsNullOrWhiteSpace(Details) ? Visibility.Collapsed : Visibility.Visible;

        private static string R(string key)
            => Application.Current.Resources[key] as string ?? key;

        public ErrorDialog()
        {
            DialogTitle = R("Error_Title");
            DialogSubtitle = R("Error_Subtitle");
            Message = R("Error_Unknown");
            Details = null;

            InitializeComponent();
            DataContext = this;
        }

        public ErrorDialog(string message, string? details = null,
                           string? title = null, string? subtitle = null)
        {
            DialogTitle = string.IsNullOrWhiteSpace(title)
                ? R("Error_Title")
                : title;

            DialogSubtitle = string.IsNullOrWhiteSpace(subtitle)
                ? R("Error_Subtitle")
                : subtitle;

            Message = string.IsNullOrWhiteSpace(message)
                ? R("Error_Unknown")
                : message;

            Details = details;

            InitializeComponent();
            DataContext = this;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = true;
                Close();
                e.Handled = true;
            }
        }
    }
}