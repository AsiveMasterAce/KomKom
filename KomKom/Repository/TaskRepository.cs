using KomKom.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomKom.Repository
{
    public class TaskRepository
    {
        private readonly string _dbPath;

        public TaskRepository(string dbPath)
        {
            _dbPath = dbPath;
        }

        private Data.ApplicationDbContext CreateContext()
        {
            return new Data.ApplicationDbContext(_dbPath);
        }

        public async Task<List<Models.ScheduledTask>> GetAllTasksAsync()
        {
            using var context = CreateContext();
            return await context.Tasks
                .AsNoTracking()
                .OrderBy(t => t.Completed)
                .ThenByDescending(t => t.DurationMinutes)
                .ThenBy(t => t.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<ScheduledTask>> GetPendingTasks()
        {
            return await Task.FromResult(Enumerable.Empty<ScheduledTask>());
        }

        public async Task AddTaskAsync(ScheduledTask task)
        {
            using var context = CreateContext();
            context.Tasks.Add(task);
            await context.SaveChangesAsync();
        }

        public async Task UpdateTaskAsync(ScheduledTask task)
        {
            using var context = CreateContext();
            context.Tasks.Update(task);
            await context.SaveChangesAsync();
        }

        public async Task DeleteTaskAsync(int taskId)
        {
            using var context = CreateContext();
            var task = await context.Tasks.FindAsync(taskId);
            if (task != null)
            {
                context.Tasks.Remove(task);
                await context.SaveChangesAsync();
            }
        }

    }
}
