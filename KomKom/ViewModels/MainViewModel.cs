using KomKom.Helpers;
using KomKom.Models;
using KomKom.Repository;
using KomKom.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KomKom.ViewModels
{
    public class MainViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public ObservableCollection<ScheduledTask> Tasks { get; } = new ObservableCollection<ScheduledTask>();


        private readonly TaskRepository _repo;
        private readonly TimerService _timer;
        private readonly TaskSchedulerService _scheduler;


        private TimeSpan _timerDisplay = TimeSpan.Zero;
        public TimeSpan TimerDisplay { get => _timerDisplay; set { _timerDisplay = value; OnPropertyChanged(); } }

        public string TimerDisplayString => $"{_timerDisplay.Minutes:D2}:{_timerDisplay.Seconds:D2}";

        public ICommand StartTimerCommand { get; }
        public ICommand PauseTimerCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand IncreaseTimeCommand { get; }
        public ICommand DecreaseTimeCommand { get; }
        public MainViewModel(TaskRepository repo, TimerService timer, TaskSchedulerService scheduler)
        {
            _repo = repo;
            _timer = timer;
            _scheduler = scheduler;



            _timer.Tick += t => TimerDisplay = t;
            _timer.Finished += () => new NotificationService().ShowToast("Timer", "Time's up!");


            StartTimerCommand = new RelayCommand((p) => _timer.Start(TimeSpan.FromMinutes(25)));
            PauseTimerCommand = new RelayCommand((p) => _timer.Pause());

            AddTaskCommand = new RelayCommand((p) => ShowAddTaskDialog());

            IncreaseTimeCommand = new RelayCommand((p) =>
            {
                TimerDisplay = TimerDisplay.Add(TimeSpan.FromSeconds(30));
            });

            DecreaseTimeCommand = new RelayCommand((p) =>
            {
                if (TimerDisplay.TotalSeconds > 30)
                {
                    TimerDisplay = TimerDisplay.Subtract(TimeSpan.FromSeconds(30));
                }
            });

            RefreshTasks();
        }
        public async Task RefreshTasks()
        {
            var tasks = await _repo.GetAllTasksAsync();
            // Update on UI thread
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Tasks.Clear();
                foreach (var task in tasks)
                {
                    Tasks.Add(task);
                }
            });
        }

        private void ShowAddTaskDialog()
        {
            var dialog = new AddTaskDialog();
            if (dialog.ShowDialog() == true)
            {
                var task = new ScheduledTask
                {
                    Title = dialog.TaskTitle,
                    StartTime = dialog.TaskStartTime,
                    DurationMinutes = dialog.TaskDuration
                };

                System.Threading.Tasks.Task.Run(async () =>
                {
                    await _repo.AddTaskAsync(task);
                    await RefreshTasks();
                });
            }
        }
    }
}
