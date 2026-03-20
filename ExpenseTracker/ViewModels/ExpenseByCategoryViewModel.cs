namespace ExpenseTracker.ViewModels
{
    public class ExpenseByCategoryViewModel
    {
        public string CategoryName { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }
    }
}