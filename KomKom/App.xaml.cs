using KomKom.Data;
using System;
using System.Configuration;
using System.Data;
using System.Windows;

namespace KomKom
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize SQLite for .NET Framework (required for some platforms)
            SQLitePCL.Batteries_V2.Init();

            // Launch MainWindow
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }

}
