using KomKom.Helpers;
using KomKom.Models;
using KomKom.Repository;
using KomKom.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Media;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KomKom.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public ObservableCollection<ScheduledTask> Tasks { get; } = new ObservableCollection<ScheduledTask>();

        private readonly TaskRepository _repo;
        private readonly TimerService _timer;
        private readonly NotificationService _notification = new NotificationService();

        public ObservableCollection<int> TimerOptions { get; } = new ObservableCollection<int>
        {
            5, 10, 15, 20, 25, 30, 45, 60
        };

        private TimeSpan _timerDisplay = TimeSpan.Zero;
        private TimeSpan _selectedDuration = TimeSpan.Zero;
        private int? _selectedTimerMinutes;
        private bool _isRunning;

        public bool IsTimerRunning
        {
            get => _isRunning;
            private set
            {
                if (_isRunning == value)
                    return;

                _isRunning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTimerSelectionEnabled));
                RefreshTimerCommandStates();
            }
        }

        public TimeSpan TimerDisplay
        {
            get => _timerDisplay;
            private set
            {
                if (_timerDisplay == value)
                    return;

                _timerDisplay = value < TimeSpan.Zero ? TimeSpan.Zero : value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimerDisplayString));
                OnPropertyChanged(nameof(TimerDisplaySafeString));
                RefreshTimerCommandStates();
            }
        }

        public TimeSpan SelectedDuration
        {
            get => _selectedDuration;
            private set
            {
                var safeValue = value < TimeSpan.Zero ? TimeSpan.Zero : value;
                if (_selectedDuration == safeValue)
                    return;

                _selectedDuration = safeValue;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedTimer));
                OnPropertyChanged(nameof(SelectedTimerPreviewText));
                OnPropertyChanged(nameof(SelectedTimerLabel));
                RefreshTimerCommandStates();
            }
        }

        public string TimerDisplayString => $"{(int)_timerDisplay.TotalMinutes:D2}:{_timerDisplay.Seconds:D2}";

        public string TimerDisplaySafeString => $"{(int)Math.Max(0, _timerDisplay.TotalMinutes):D2}:{Math.Max(0, _timerDisplay.Seconds):D2}";

        public int? SelectedTimerMinutes
        {
            get => _selectedTimerMinutes;
            set
            {
                if (_selectedTimerMinutes == value)
                    return;

                _selectedTimerMinutes = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedTimer));
                OnPropertyChanged(nameof(SelectedTimerPreviewText));
                OnPropertyChanged(nameof(SelectedTimerLabel));

                if (value.HasValue)
                {
                    SelectedDuration = TimeSpan.FromMinutes(value.Value);
                }
                else
                {
                    OnPropertyChanged(nameof(HasSelectedTimer));
                }

                RefreshTimerCommandStates();
            }
        }

        public bool HasSelectedTimer => SelectedDuration > TimeSpan.Zero;

        public string SelectedTimerLabel => HasSelectedTimer ? "Start time" : "Select a time";

        public string SelectedTimerPreviewText
        {
            get
            {
                if (!HasSelectedTimer)
                    return string.Empty;

                if (SelectedDuration.TotalSeconds < 60)
                    return $"You selected {SelectedDuration.Seconds} sec";

                if (SelectedDuration.Seconds == 0)
                    return $"You selected {(int)SelectedDuration.TotalMinutes} min";

                return $"You selected {(int)SelectedDuration.TotalMinutes}:{SelectedDuration.Seconds:D2}";
            }
        }

        public bool IsTimerSelectionEnabled => !_isRunning;

        private readonly RelayCommand _startTimerCommand;
        private readonly RelayCommand _pauseTimerCommand;
        private readonly RelayCommand _resetTimerCommand;

        public ICommand StartTimerCommand => _startTimerCommand;
        public ICommand PauseTimerCommand => _pauseTimerCommand;
        public ICommand ResetTimerCommand => _resetTimerCommand;
        public ICommand IncreaseTimeCommand { get; }
        public ICommand DecreaseTimeCommand { get; }
        public ICommand AddTaskCommand { get; }

        public MainViewModel(TaskRepository repo, TimerService timer)
        {
            _repo = repo;
            _timer = timer;

            _timer.Tick += OnTimerTick;
            _timer.Finished += OnTimerFinished;

            _startTimerCommand = new RelayCommand(_ => StartTimer(), _ => !IsTimerRunning && HasSelectedTimer);
            _pauseTimerCommand = new RelayCommand(_ => PauseTimer(), _ => IsTimerRunning);
            _resetTimerCommand = new RelayCommand(_ => ResetTimer(), _ => IsTimerRunning || TimerDisplay > TimeSpan.Zero || HasSelectedTimer);
            IncreaseTimeCommand = new RelayCommand(_ => AdjustSelection(TimeSpan.FromSeconds(30)));
            DecreaseTimeCommand = new RelayCommand(_ => AdjustSelection(TimeSpan.FromSeconds(-30)));
            AddTaskCommand = new RelayCommand(_ => ShowAddTaskDialog());

            _ = RefreshTasks();
        }

        private void OnTimerTick(TimeSpan remaining)
        {
            TimerDisplay = remaining;
        }

        private void OnTimerFinished()
        {
            IsTimerRunning = false;
            TimerDisplay = TimeSpan.Zero;
            SystemSounds.Exclamation.Play();
            _notification.ShowToast("Timer", "Time's up!");
        }

        private void OpenTaskDialog()
        {
            var dialog = new AddTaskDialog();
            dialog.Owner = System.Windows.Application.Current?.MainWindow;
            dialog.ShowDialog();
        }

        private void StartTimer()
        {
            if (!HasSelectedTimer)
                return;

            TimerDisplay = SelectedDuration;
            IsTimerRunning = true;
            _timer.Start(TimerDisplay);
        }

        private void PauseTimer()
        {
            IsTimerRunning = false;
            _timer.Pause();
        }

        private void ResetTimer()
        {
            IsTimerRunning = false;
            TimerDisplay = TimeSpan.Zero;
            SelectedDuration = TimeSpan.Zero;
            SelectedTimerMinutes = null;
            _timer.Reset(TimeSpan.Zero);
        }

        private void AdjustSelection(TimeSpan delta)
        {
            if (IsTimerRunning)
            {
                var adjustedRunningTime = TimerDisplay + delta;
                TimerDisplay = adjustedRunningTime < TimeSpan.Zero ? TimeSpan.Zero : adjustedRunningTime;
                _timer.SetTime(TimerDisplay);
                return;
            }

            SelectedTimerMinutes = null;
            SelectedDuration = SelectedDuration + delta;
        }

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

        private async void ShowAddTaskDialog()
        {
            var dialog = new AddTaskDialog();
            if (dialog.ShowDialog() == true)
            {
                var task = new ScheduledTask
                {
                    Title = dialog.TaskTitle,
                    StartTime = DateTime.Now,
                    DurationMinutes = dialog.TaskPriority,
                    Category = dialog.TaskTags,
                    Completed = false
                };

                await _repo.AddTaskAsync(task);
                await RefreshTasks();
            }
        }

        public ICommand IncreasePriorityCommand => new RelayCommand(async task => await ChangePriorityAsync(task as ScheduledTask, 1));
        public ICommand DecreasePriorityCommand => new RelayCommand(async task => await ChangePriorityAsync(task as ScheduledTask, -1));
        public ICommand ToggleTaskCompletedCommand => new RelayCommand(async task => await ToggleTaskCompletedAsync(task as ScheduledTask));

        private async Task ChangePriorityAsync(ScheduledTask task, int delta)
        {
            if (task == null)
                return;

            task.DurationMinutes = Math.Max(1, task.DurationMinutes + delta);
            await _repo.UpdateTaskAsync(task);
            await RefreshTasks();
        }

        private async Task ToggleTaskCompletedAsync(ScheduledTask task)
        {
            if (task == null)
                return;

            task.Completed = !task.Completed;
            await _repo.UpdateTaskAsync(task);
            await RefreshTasks();
        }

        private void RefreshTimerCommandStates()
        {
            _startTimerCommand.RaiseCanExecuteChanged();
            _pauseTimerCommand.RaiseCanExecuteChanged();
            _resetTimerCommand.RaiseCanExecuteChanged();
        }
    }
}
