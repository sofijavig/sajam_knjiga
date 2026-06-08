using System;
using System.IO;
using System.Windows;

namespace WpfClient
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // UVEK radi iz bin foldera (gde je exe)
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            // osiguraj Data folder
            Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Data"));
        }
    }
}