using System.Diagnostics;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ExpenseTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return View(new DashboardViewModel());
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var totalIncome = await _context.Transactions
                .Where(t => t.UserId == userId && t.Type == TransactionType.Income)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var totalExpenses = await _context.Transactions
                .Where(t => t.UserId == userId && t.Type == TransactionType.Expense)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var latestTransactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .Include(t => t.Category)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .Take(5)
                .ToListAsync();

            var expensesByCategory = await _context.Transactions
                .Where(t => t.UserId == userId && t.Type == TransactionType.Expense)
                .Include(t => t.Category)
                .GroupBy(t => t.Category!.Name)
                .Select(g => new ExpenseByCategoryViewModel
                {
                    CategoryName = g.Key,
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                Balance = totalIncome - totalExpenses,
                LatestTransactions = latestTransactions,
                ExpensesByCategory = expensesByCategory
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
