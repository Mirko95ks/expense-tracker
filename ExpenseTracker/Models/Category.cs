using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public CategoryType Type { get; set; }

        public string? UserId { get; set; }

        public ICollection<Transaction>? Transactions { get; set; }
    }
}