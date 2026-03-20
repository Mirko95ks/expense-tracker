using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExpenseTracker.ViewModels
{
    public class TransactionFilterViewModel
    {
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        public TransactionType? Type { get; set; }

        public int? CategoryId { get; set; }

        public List<SelectListItem> TypeOptions { get; set; } = new();

        public List<SelectListItem> CategoryOptions { get; set; } = new();
    }
}