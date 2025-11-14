using KomKom.Data;
using KomKom.Repository;
using KomKom.Services;
using KomKom.ViewModels;
using Microsoft.EntityFrameworkCore;
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
        public MainViewModel DataContext { get; }
        public MainWindow()
        {
            InitializeComponent();

            var dataPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "TaskSchedulerApp");
            var dbPath = System.IO.Path.Combine(dataPath, "tasks.db");
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            _repo = new TaskRepository(new ApplicationDbContext(options));
            _timer = new TimerService();
            _scheduler = new TaskSchedulerService(new NotificationService(), _repo);
            DataContext = new MainViewModel(_repo, _timer, _scheduler);
        }

    }
}