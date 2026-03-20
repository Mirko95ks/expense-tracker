using System.Security.Claims;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Monthly()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync();

            var items = transactions
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .Select(g => new MonthlyReportItemViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalIncome = g
                        .Where(t => t.Type == TransactionType.Income)
                        .Sum(t => t.Amount),
                    TotalExpenses = g
                        .Where(t => t.Type == TransactionType.Expense)
                        .Sum(t => t.Amount)
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ToList();

            var model = new MonthlyReportViewModel
            {
                Items = items
            };

            return View(model);
        }
    }
}