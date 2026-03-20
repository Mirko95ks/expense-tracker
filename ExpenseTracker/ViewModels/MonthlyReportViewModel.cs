namespace ExpenseTracker.ViewModels
{
    public class MonthlyReportViewModel
    {
        public List<MonthlyReportItemViewModel> Items { get; set; } = new();

        public decimal TotalIncome => Items.Sum(x => x.TotalIncome);

        public decimal TotalExpenses => Items.Sum(x => x.TotalExpenses);

        public decimal TotalBalance => TotalIncome - TotalExpenses;
    }
}