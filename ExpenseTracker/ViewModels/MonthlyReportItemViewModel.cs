using System.Globalization;

namespace ExpenseTracker.ViewModels
{
    public class MonthlyReportItemViewModel
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public string MonthName =>
    $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(new CultureInfo("sr-Latn-RS").DateTimeFormat.GetMonthName(Month))} {Year}";

        public decimal TotalIncome { get; set; }

        public decimal TotalExpenses { get; set; }

        public decimal Balance => TotalIncome - TotalExpenses;
    }
}