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
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SQLitePCL.Batteries_V2.Init(); // important for SQLite on .NET Framework

            using (var db = new ApplicationDbContext())
            {
                await db.Database.EnsureCreatedAsync();
            }
        }
    }

}
