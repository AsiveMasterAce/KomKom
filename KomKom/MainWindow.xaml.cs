using KomKom.Data;
using KomKom.Repository;
using KomKom.Services;
using KomKom.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace KomKom
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TaskRepository _repo;
        private readonly TaskSchedulerService _scheduler;
        private readonly TimerService _timer;

        public MainViewModel ViewModel { get; }

        public MainWindow() : this(GetDefaultDbPath())
        {
        }

        // Internal constructor that accepts a database path
        private MainWindow(string dbPath)
        {
            InitializeComponent();

            // Ensure the folder exists
            var folder = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // Create DbContext options with SQLite
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;


            // Initialize repository, services, and ViewModel
            _repo = new TaskRepository(new ApplicationDbContext());
            _timer = new TimerService();
            _scheduler = new TaskSchedulerService(new NotificationService(), _repo);
            ViewModel = new MainViewModel(_repo, _timer, _scheduler);

            // Set DataContext for data binding
            this.DataContext = ViewModel;
        }

        // Helper to get default database path
        private static string GetDefaultDbPath()
        {
            var dataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "KomKom",
                "Data"
            );

            return Path.Combine(dataPath, "tasks.db");
        }

    }
}