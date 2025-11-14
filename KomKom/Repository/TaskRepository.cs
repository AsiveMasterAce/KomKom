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
        private readonly Data.ApplicationDbContext _context;

        public TaskRepository(Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Models.ScheduledTask>> GetAllTasksAsync()
        {
            return await _context.Tasks.OrderBy(t => t.StartTime).AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<ScheduledTask>> GetPendingTasks()
        {
            return await _context.Tasks
                .Where(t => !t.Completed && t.StartTime >= DateTime.Now)
                .OrderBy(t => t.StartTime)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddTaskAsync(ScheduledTask task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTaskAsync(ScheduledTask task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTaskAsync(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

    }
}
