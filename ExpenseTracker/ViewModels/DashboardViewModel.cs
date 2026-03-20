using ExpenseTracker.Models;

namespace ExpenseTracker.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalIncome { get; set; }

        public decimal TotalExpenses { get; set; }

        public decimal Balance { get; set; }

        public List<Transaction> LatestTransactions { get; set; } = new();

        public List<ExpenseByCategoryViewModel> ExpensesByCategory { get; set; } = new();
    }
}