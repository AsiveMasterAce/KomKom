using KomKom.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace KomKom.Services
{
    public class TaskSchedulerService
    {    // This class can be expanded in the future to include scheduling logic
        // For now, it serves as a placeholder for task scheduling functionalities
        private readonly NotificationService _notificationService;
        private readonly DispatcherTimer _checker;
        private readonly TaskRepository _repo;
        public TaskSchedulerService(NotificationService notificationService, TaskRepository repo)
        {
            _checker = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _notificationService = notificationService;
            _repo = repo;
            _checker.Tick += (s, e) => CheckTasks();
            _checker.Start();

        }
        private async Task CheckTasks()
        {
            var pending = await _repo.GetPendingTasks();
            foreach (var task in pending)
            {
                // notify and mark complete
                _notificationService.ShowToast(task.Title, $"Scheduled for {task.StartTime:t}");
                task.Completed = true;
                _repo.UpdateTaskAsync(task);
            }
        }
    }
}
