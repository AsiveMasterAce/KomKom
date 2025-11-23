using KomKom.Helpers;
using KomKom.Models;
using KomKom.Repository;
using KomKom.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KomKom.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Tasks collection
        public ObservableCollection<ScheduledTask> Tasks { get; } = new ObservableCollection<ScheduledTask>();

        // Services
        private readonly TaskRepository _repo;
        private readonly TimerService _timer;
        private readonly TaskSchedulerService _scheduler;
        private readonly NotificationService _notification = new NotificationService();

        // Timer properties
        private TimeSpan _timerDisplay = TimeSpan.Zero;
        private bool _isRunning;

        public TimeSpan TimerDisplay
        {
            get => _timerDisplay;
            private set
            {
                _timerDisplay = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimerDisplayString));
            }
        }

        public string TimerDisplayString => $"{_timerDisplay.Minutes:D2}:{_timerDisplay.Seconds:D2}";

        // Commands
        public ICommand StartTimerCommand { get; }
        public ICommand PauseTimerCommand { get; }
        public ICommand IncreaseTimeCommand { get; }
        public ICommand DecreaseTimeCommand { get; }
        public ICommand AddTaskCommand { get; }

        public MainViewModel(TaskRepository repo, TimerService timer, TaskSchedulerService scheduler)
        {
            _repo = repo;
            _timer = timer;
            _scheduler = scheduler;

            // Hook timer events
            _timer.Tick += OnTimerTick;
            _timer.Finished += OnTimerFinished;

            // Initialize commands
            StartTimerCommand = new RelayCommand(o => StartTimer(), o => !_isRunning);
            PauseTimerCommand = new RelayCommand(o => PauseTimer(), o => _isRunning);
            IncreaseTimeCommand = new RelayCommand(o => AdjustTime(30));
            DecreaseTimeCommand = new RelayCommand(o => AdjustTime(-30));
            AddTaskCommand = new RelayCommand(o => ShowAddTaskDialog());

            // Load tasks
            _ = RefreshTasks();
        }

        // Timer event handlers
        private void OnTimerTick(TimeSpan remaining)
        {
            TimerDisplay = remaining;
        }

        private void OnTimerFinished()
        {
            _isRunning = false;
            OnPropertyChanged(nameof(StartTimerCommand));
            OnPropertyChanged(nameof(PauseTimerCommand));
            _notification.ShowToast("Timer", "Time's up!");
        }

        // Timer control methods
        private void StartTimer()
        {
            if (TimerDisplay == TimeSpan.Zero)
                TimerDisplay = TimeSpan.FromMinutes(25); // default if zero

            _isRunning = true;
            _timer.Start(TimerDisplay);
            OnPropertyChanged(nameof(StartTimerCommand));
            OnPropertyChanged(nameof(PauseTimerCommand));
        }

        private void PauseTimer()
        {
            _isRunning = false;
            _timer.Pause();
            OnPropertyChanged(nameof(StartTimerCommand));
            OnPropertyChanged(nameof(PauseTimerCommand));
        }

        private void AdjustTime(int seconds)
        {
            var newTime = TimerDisplay.Add(TimeSpan.FromSeconds(seconds));
            TimerDisplay = newTime.TotalSeconds < 0 ? TimeSpan.Zero : newTime;
        }

        // Tasks
        public async Task RefreshTasks()
        {
            var tasks = await _repo.GetAllTasksAsync();
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Tasks.Clear();
                foreach (var task in tasks)
                    Tasks.Add(task);
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

                _ = Task.Run(async () =>
                {
                    await _repo.AddTaskAsync(task);
                    await RefreshTasks();
                });
            }
        }
    }
}
