using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomKom.Models
{
    public class ScheduledTask
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Category { get; set; } = "Normal";
        public bool Completed { get; set; }

        [NotMapped]
        public bool IsImportant => HasTag(Category, "important");

        private static bool HasTag(string? category, string tag)
        {
            if (string.IsNullOrWhiteSpace(category))
                return false;

            return category.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(item => string.Equals(item, tag, StringComparison.OrdinalIgnoreCase));
        }
    }
}
