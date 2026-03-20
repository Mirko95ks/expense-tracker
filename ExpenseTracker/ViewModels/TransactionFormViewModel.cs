using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Models;

namespace ExpenseTracker.ViewModels
{
    public class TransactionFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [Range(0.01, 9999999999999999.99)]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [StringLength(200)]
        public string? Description { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
    }
}