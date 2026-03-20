using ExpenseTracker.Models;

namespace ExpenseTracker.ViewModels
{
    public class TransactionIndexViewModel
    {
        public TransactionFilterViewModel Filter { get; set; } = new();

        public List<Transaction> Transactions { get; set; } = new();

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;

        public bool HasNextPage => CurrentPage < TotalPages;
    }
}