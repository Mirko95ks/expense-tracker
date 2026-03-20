using System.Security.Claims;
using ExpenseTracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Models;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using ClosedXML.Excel;
using System.IO;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
       DateTime? fromDate,
       DateTime? toDate,
       TransactionType? type,
       int? categoryId,
       int page = 1)
        {
            const int pageSize = 10;
            page = page < 1 ? 1 : page;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(t => t.Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var inclusiveToDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(t => t.Date <= inclusiveToDate);
            }

            if (type.HasValue)
            {
                query = query.Where(t => t.Type == type.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == categoryId.Value);
            }

            var totalCount = await query.CountAsync();

            var transactions = await query
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _context.Categories
                .Where(c => c.UserId == null || c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var model = new TransactionIndexViewModel
            {
                Filter = new TransactionFilterViewModel
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    Type = type,
                    CategoryId = categoryId,
                    TypeOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "All" },
                new SelectListItem
                {
                    Value = ((int)TransactionType.Income).ToString(),
                    Text = "Income"
                },
                new SelectListItem
                {
                    Value = ((int)TransactionType.Expense).ToString(),
                    Text = "Expense"
                }
            },
                    CategoryOptions = categories
                        .Select(c => new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = c.Name
                        })
                        .Prepend(new SelectListItem
                        {
                            Value = "",
                            Text = "All"
                        })
                        .ToList()
                },
                Transactions = transactions,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View(model);
        }

        private async Task PopulateCategoriesAsync(TransactionType? selectedType = null, int? selectedCategoryId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var categories = await _context.Categories
                .Where(c => c.UserId == null || c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var filteredCategories = selectedType.HasValue
                ? categories.Where(c => (int)c.Type == (int)selectedType.Value).ToList()
                : categories;

            ViewBag.CategoryId = new SelectList(filteredCategories, "Id", "Name", selectedCategoryId);
            ViewBag.Type = new SelectList(
                    Enum.GetValues(typeof(TransactionType))
                        .Cast<TransactionType>()
                        .Select(t => new
                        {
                            Value = ((int)t).ToString(),
                            Text = t.ToString()
                        }),
                         "Value",
                         "Text",
                         selectedType.HasValue ? ((int)selectedType.Value).ToString() : null
                                           );

            ViewBag.AllCategoriesJson = JsonSerializer.Serialize(
                categories.Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    type = (int)c.Type
                }));
        }
        public async Task<IActionResult> Create(TransactionType? type)
        {
            var selectedType = type ?? TransactionType.Expense;

            var model = new TransactionFormViewModel
            {
                Date = DateTime.Today,
                Type = selectedType
            };

            await PopulateCategoriesAsync(model.Type);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransactionFormViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.Id == model.CategoryId &&
                    (c.UserId == null || c.UserId == userId));

            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Selected category does not exist.");
            }
            else if ((int)category.Type != (int)model.Type)
            {
                ModelState.AddModelError("CategoryId", "Selected category does not match transaction type.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync(model.Type, model.CategoryId);
                return View(model);
            }

            var transaction = new Transaction
            {
                Amount = model.Amount,
                Date = model.Date,
                Description = model.Description,
                Type = model.Type,
                CategoryId = model.CategoryId,
                UserId = userId!
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (transaction == null)
            {
                return NotFound();
            }

            var model = new TransactionFormViewModel
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                Date = transaction.Date,
                Description = transaction.Description,
                Type = transaction.Type,
                CategoryId = transaction.CategoryId
            };

            await PopulateCategoriesAsync(model.Type, model.CategoryId);
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TransactionFormViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (transaction == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.Id == model.CategoryId &&
                    (c.UserId == null || c.UserId == userId));

            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Selected category does not exist.");
            }
            else if ((int)category.Type != (int)model.Type)
            {
                ModelState.AddModelError("CategoryId", "Selected category does not match transaction type.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync(model.Type, model.CategoryId);
                return View(model);
            }

            transaction.Amount = model.Amount;
            transaction.Date = model.Date;
            transaction.Description = model.Description;
            transaction.Type = model.Type;
            transaction.CategoryId = model.CategoryId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> ExportToExcel(
    DateTime? fromDate,
    DateTime? toDate,
    TransactionType? type,
    int? categoryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(t => t.Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var inclusiveToDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(t => t.Date <= inclusiveToDate);
            }

            if (type.HasValue)
            {
                query = query.Where(t => t.Type == type.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == categoryId.Value);
            }

            var transactions = await query
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Transactions");

            worksheet.Cell(1, 1).Value = "Transactions Export";
            worksheet.Range(1, 1, 1, 5).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(3, 1).Value = "Date";
            worksheet.Cell(3, 2).Value = "Type";
            worksheet.Cell(3, 3).Value = "Category";
            worksheet.Cell(3, 4).Value = "Description";
            worksheet.Cell(3, 5).Value = "Amount";

            var headerRange = worksheet.Range(3, 1, 3, 5);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 4;

            foreach (var transaction in transactions)
            {
                worksheet.Cell(row, 1).Value = transaction.Date;
                worksheet.Cell(row, 1).Style.DateFormat.Format = "dd.MM.yyyy";

                worksheet.Cell(row, 2).Value = transaction.Type.ToString();
                worksheet.Cell(row, 3).Value = transaction.Category?.Name ?? "";
                worksheet.Cell(row, 4).Value = transaction.Description ?? "";

                var signedAmount = transaction.Type == TransactionType.Income
                    ? transaction.Amount
                    : -transaction.Amount;

                worksheet.Cell(row, 5).Value = signedAmount;
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";

                if (signedAmount >= 0)
                {
                    worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.DarkGreen;
                }
                else
                {
                    worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.DarkRed;
                }

                row++;
            }

            var totalIncome = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var totalExpenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            var balance = totalIncome - totalExpenses;

            row += 1;

            worksheet.Cell(row, 4).Value = "Total Income:";
            worksheet.Cell(row, 5).Value = totalIncome;
            worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.DarkGreen;

            row++;
            worksheet.Cell(row, 4).Value = "Total Expenses:";
            worksheet.Cell(row, 5).Value = totalExpenses;
            worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 5).Style.Font.FontColor = XLColor.DarkRed;

            row++;
            worksheet.Cell(row, 4).Value = "Balance:";
            worksheet.Cell(row, 5).Value = balance;
            worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 5).Style.Font.Bold = true;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"transactions_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}