using KomKom.Helpers;
using KomKom.Models;
using KomKom.Repository;
using KomKom.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

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
        private readonly MediaPlayer _ringPlayer = new MediaPlayer();
        private double _timerSoundVolume = 1.0;

        public ObservableCollection<int> TimerOptions { get; } = new ObservableCollection<int>
        {
            5, 10, 15, 20, 25, 30, 45, 60
        };

        private TimeSpan _timerDisplay = TimeSpan.Zero;
        private TimeSpan _selectedDuration = TimeSpan.Zero;
        private TimeSpan _activeTimerDuration = TimeSpan.Zero;
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
                OnPropertyChanged(nameof(HasPausedTimerSession));
                OnPropertyChanged(nameof(TimerPrimaryActionText));
                OnPropertyChanged(nameof(TimerStatusText));
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
                OnPropertyChanged(nameof(TimerProgress));
                OnPropertyChanged(nameof(TimerProgressGeometry));
                OnPropertyChanged(nameof(HasPausedTimerSession));
                OnPropertyChanged(nameof(TimerStatusText));
                OnPropertyChanged(nameof(TimerPrimaryActionText));
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
                OnPropertyChanged(nameof(TimerPrimaryActionText));
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
                OnPropertyChanged(nameof(TimerPrimaryActionText));
                OnPropertyChanged(nameof(TimerStatusText));

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

        public bool HasActiveTimerSession => _activeTimerDuration > TimeSpan.Zero;

        public bool HasPausedTimerSession => HasActiveTimerSession && !IsTimerRunning && TimerDisplay > TimeSpan.Zero;

        public string TimerStatusText => IsTimerRunning ? "Running" : HasPausedTimerSession ? "Paused" : "Ready";

        public string TimerPrimaryActionText => IsTimerRunning ? "Pause" : HasActiveTimerSession ? "Resume" : "Start";

        public Brush TimerAccentBrush => new SolidColorBrush(Color.FromRgb(79, 70, 229));

        public Brush TimerTrackBrush => new SolidColorBrush(Color.FromRgb(229, 231, 235));

        public Brush TimerSurfaceBrush => new SolidColorBrush(Color.FromRgb(249, 250, 251));

        public Brush TimerStatusBrush => new SolidColorBrush(Color.FromRgb(107, 114, 128));

        public double TimerProgress
        {
            get
            {
                if (_activeTimerDuration <= TimeSpan.Zero)
                    return 0;

                var elapsed = _activeTimerDuration - TimerDisplay;
                var progress = elapsed.TotalSeconds / _activeTimerDuration.TotalSeconds;
                return Math.Max(0, Math.Min(1, progress));
            }
        }

        public Geometry TimerProgressGeometry => CreateTimerProgressGeometry(TimerProgress);

        public double TimerSoundVolume
        {
            get => _timerSoundVolume;
            set
            {
                var safeValue = Math.Max(0, Math.Min(1, value));
                if (Math.Abs(_timerSoundVolume - safeValue) < 0.0001)
                    return;

                _timerSoundVolume = safeValue;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimerSoundVolumePercent));
            }
        }

        public string TimerSoundVolumePercent => $"{(int)Math.Round(TimerSoundVolume * 100)}%";

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

        public bool IsTimerSelectionEnabled => !_isRunning && !HasActiveTimerSession;

        private readonly RelayCommand _startTimerCommand;
        private readonly RelayCommand _pauseTimerCommand;
        private readonly RelayCommand _resetTimerCommand;
        private readonly RelayCommand _primaryTimerCommand;

        public ICommand StartTimerCommand => _startTimerCommand;
        public ICommand PauseTimerCommand => _pauseTimerCommand;
        public ICommand ResetTimerCommand => _resetTimerCommand;
        public ICommand TimerPrimaryActionCommand => _primaryTimerCommand;
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
            _primaryTimerCommand = new RelayCommand(_ => ExecutePrimaryTimerAction(), _ => CanExecutePrimaryTimerAction());
            IncreaseTimeCommand = new RelayCommand(_ => AdjustSelection(TimeSpan.FromSeconds(30)));
            DecreaseTimeCommand = new RelayCommand(_ => AdjustSelection(TimeSpan.FromSeconds(-30)));
            AddTaskCommand = new RelayCommand(_ => ShowAddTaskDialog());

            _ = RefreshTasks();
        }

        private void OnTimerTick(TimeSpan remaining)
        {
            TimerDisplay = remaining;
            OnPropertyChanged(nameof(TimerAccentBrush));
            OnPropertyChanged(nameof(TimerTrackBrush));
            OnPropertyChanged(nameof(TimerSurfaceBrush));
            OnPropertyChanged(nameof(TimerStatusBrush));
        }

        private void OnTimerFinished()
        {
            IsTimerRunning = false;
            _activeTimerDuration = TimeSpan.Zero;
            TimerDisplay = TimeSpan.Zero;
            PlayTimerRing();
            _notification.ShowToast("Timer", "Time's up!");
            OnPropertyChanged(nameof(IsTimerSelectionEnabled));
            OnPropertyChanged(nameof(TimerPrimaryActionText));
            OnPropertyChanged(nameof(TimerStatusText));
            OnPropertyChanged(nameof(TimerProgress));
            OnPropertyChanged(nameof(TimerProgressGeometry));
            OnPropertyChanged(nameof(TimerAccentBrush));
            OnPropertyChanged(nameof(TimerTrackBrush));
            OnPropertyChanged(nameof(TimerSurfaceBrush));
            OnPropertyChanged(nameof(TimerStatusBrush));
            OnPropertyChanged(nameof(HasPausedTimerSession));
        }

        private void PlayTimerRing()
        {
            var ringPath = Path.Combine(AppContext.BaseDirectory, "Sounds", "ring Kom.mp3");

            if (!File.Exists(ringPath))
            {
                SystemSounds.Exclamation.Play();
                return;
            }

            try
            {
                _ringPlayer.Stop();
                _ringPlayer.Open(new Uri(ringPath, UriKind.Absolute));
                _ringPlayer.Volume = TimerSoundVolume;
                _ringPlayer.Play();
            }
            catch
            {
                SystemSounds.Exclamation.Play();
            }
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

            _activeTimerDuration = SelectedDuration;
            TimerDisplay = SelectedDuration;
            IsTimerRunning = true;
            _timer.Start(TimerDisplay);
        }

        private void PauseTimer()
        {
            IsTimerRunning = false;
            _timer.Pause();
        }

        private void ResumeTimer()
        {
            if (!HasActiveTimerSession || TimerDisplay <= TimeSpan.Zero)
                return;

            IsTimerRunning = true;
            _timer.Resume();
        }

        private void ResetTimer()
        {
            IsTimerRunning = false;
            _activeTimerDuration = TimeSpan.Zero;
            TimerDisplay = TimeSpan.Zero;
            SelectedDuration = TimeSpan.Zero;
            SelectedTimerMinutes = null;
            _timer.Reset(TimeSpan.Zero);
            OnPropertyChanged(nameof(TimerProgress));
            OnPropertyChanged(nameof(TimerProgressGeometry));
            OnPropertyChanged(nameof(TimerStatusText));
            OnPropertyChanged(nameof(TimerPrimaryActionText));
            OnPropertyChanged(nameof(HasPausedTimerSession));
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
        public ICommand ToggleTaskImportantCommand => new RelayCommand(async task => await ToggleTaskImportantAsync(task as ScheduledTask));
        public ICommand ToggleTaskCompletedCommand => new RelayCommand(async task => await ToggleTaskCompletedAsync(task as ScheduledTask));
        public ICommand DeleteTaskCommand => new RelayCommand(async task => await DeleteTaskAsync(task as ScheduledTask));

        private async Task ChangePriorityAsync(ScheduledTask? task, int delta)
        {
            if (task == null)
                return;

            task.DurationMinutes = Math.Max(1, task.DurationMinutes + delta);
            await _repo.UpdateTaskAsync(task);
            await RefreshTasks();
        }

        private async Task ToggleTaskCompletedAsync(ScheduledTask? task)
        {
            if (task == null)
                return;

            task.Completed = !task.Completed;
            await _repo.UpdateTaskAsync(task);
            await RefreshTasks();
        }

        private async Task DeleteTaskAsync(ScheduledTask? task)
        {
            if (task == null)
                return;

            await _repo.DeleteTaskAsync(task.Id);
            await RefreshTasks();
        }

        private async Task ToggleTaskImportantAsync(ScheduledTask? task)
        {
            if (task == null)
                return;

            task.Category = UpdateImportantTag(task.Category, !task.IsImportant);
            await _repo.UpdateTaskAsync(task);
            await RefreshTasks();
        }

        private static string UpdateImportantTag(string? category, bool isImportant)
        {
            var tags = string.IsNullOrWhiteSpace(category)
                ? new List<string>()
                : category.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            tags.RemoveAll(tag => string.Equals(tag, "Normal", StringComparison.OrdinalIgnoreCase));
            tags.RemoveAll(tag => string.Equals(tag, "important", StringComparison.OrdinalIgnoreCase));

            if (isImportant)
            {
                tags.Insert(0, "important");
            }

            if (tags.Count == 0)
                return isImportant ? "important" : "Normal";

            return string.Join(", ", tags);
        }

        private void ExecutePrimaryTimerAction()
        {
            if (IsTimerRunning)
            {
                PauseTimer();
                return;
            }

            if (HasActiveTimerSession && TimerDisplay > TimeSpan.Zero)
            {
                ResumeTimer();
                return;
            }

            StartTimer();
        }

        private bool CanExecutePrimaryTimerAction()
        {
            return HasSelectedTimer || HasActiveTimerSession;
        }

        private static Geometry CreateTimerProgressGeometry(double progress)
        {
            progress = Math.Max(0, Math.Min(1, progress));

            if (progress <= 0)
                return Geometry.Empty;

            if (progress >= 1)
                progress = 0.9999;

            const double center = 50;
            const double radius = 40;
            var startAngle = -90.0;
            var endAngle = startAngle + 360.0 * progress;

            var startPoint = PointOnCircle(center, center, radius, startAngle);
            var endPoint = PointOnCircle(center, center, radius, endAngle);
            var isLargeArc = endAngle - startAngle > 180;

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(startPoint, false, false);
                context.ArcTo(endPoint, new System.Windows.Size(radius, radius), 0, isLargeArc, SweepDirection.Clockwise, true, false);
            }

            geometry.Freeze();
            return geometry;
        }

        private static System.Windows.Point PointOnCircle(double centerX, double centerY, double radius, double angleDegrees)
        {
            var angleRadians = angleDegrees * Math.PI / 180.0;
            return new System.Windows.Point(
                centerX + radius * Math.Cos(angleRadians),
                centerY + radius * Math.Sin(angleRadians));
        }

        private void RefreshTimerCommandStates()
        {
            _startTimerCommand.RaiseCanExecuteChanged();
            _pauseTimerCommand.RaiseCanExecuteChanged();
            _resetTimerCommand.RaiseCanExecuteChanged();
            _primaryTimerCommand.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(TimerPrimaryActionText));
            OnPropertyChanged(nameof(TimerStatusText));
            OnPropertyChanged(nameof(TimerProgress));
            OnPropertyChanged(nameof(TimerProgressGeometry));
            OnPropertyChanged(nameof(TimerAccentBrush));
            OnPropertyChanged(nameof(TimerTrackBrush));
            OnPropertyChanged(nameof(TimerSurfaceBrush));
            OnPropertyChanged(nameof(TimerStatusBrush));
            OnPropertyChanged(nameof(HasPausedTimerSession));
            OnPropertyChanged(nameof(HasActiveTimerSession));
        }
    }
}
