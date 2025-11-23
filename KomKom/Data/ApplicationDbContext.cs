using KomKom.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace KomKom.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly string _dbPath;
        public ApplicationDbContext()
        {
            var dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            _dbPath = Path.Combine(dataFolder, "tasks.db");
        }

        public ApplicationDbContext(string dbPath)
        {
            _dbPath = dbPath;

            // Ensure directory exists
            var folder = Path.GetDirectoryName(_dbPath);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_dbPath}");
        }

        // DbSets
        public DbSet<ScheduledTask> Tasks { get; set; }
    }

}
