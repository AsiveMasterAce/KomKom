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
        private static readonly string _dbPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "tasks.db");

        public ApplicationDbContext()
            : base(GetOptions())
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        private static DbContextOptions<ApplicationDbContext> GetOptions()
        {
            // Ensure the Data folder exists
            var dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseSqlite($"Data Source={_dbPath}");

            return builder.Options;
        }

        // DbSets
        public DbSet<ScheduledTask> Tasks { get; set; }
    }

}
