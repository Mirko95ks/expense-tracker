using ExpenseTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);

            builder.Entity<Category>().HasData(

    new Category { Id = 1, Name = "Plata", Type = CategoryType.Income },
    new Category { Id = 2, Name = "Ostalo", Type = CategoryType.Income },

    new Category { Id = 3, Name = "Kirija", Type = CategoryType.Expense },
    new Category { Id = 4, Name = "Racuni", Type = CategoryType.Expense },
    new Category { Id = 5, Name = "Teretana", Type = CategoryType.Expense },
    new Category { Id = 6, Name = "Kredit i odrzavanje racuna", Type = CategoryType.Expense },
    new Category { Id = 7, Name = "Telefon", Type = CategoryType.Expense },
    new Category { Id = 8, Name = "ETF", Type = CategoryType.Expense },
    new Category { Id = 9, Name = "Lekar/Veterinar", Type = CategoryType.Expense },
    new Category { Id = 10, Name = "Hrana", Type = CategoryType.Expense },
    new Category { Id = 11, Name = "Grickalice", Type = CategoryType.Expense },
    new Category { Id = 12, Name = "Kucna higijena", Type = CategoryType.Expense },
    new Category { Id = 13, Name = "Kozmetika", Type = CategoryType.Expense },
    new Category { Id = 14, Name = "Cigare", Type = CategoryType.Expense },
    new Category { Id = 15, Name = "Soping", Type = CategoryType.Expense },
    new Category { Id = 16, Name = "Izlasci", Type = CategoryType.Expense },
    new Category { Id = 17, Name = "Dostava hrana", Type = CategoryType.Expense },
    new Category { Id = 18, Name = "Restorani", Type = CategoryType.Expense },
    new Category { Id = 19, Name = "Aksesoari", Type = CategoryType.Expense },
    new Category { Id = 20, Name = "Pokloni", Type = CategoryType.Expense },
    new Category { Id = 21, Name = "Putovanja", Type = CategoryType.Expense },
    new Category { Id = 22, Name = "Online porudzbine", Type = CategoryType.Expense },
    new Category { Id = 23, Name = "Gorivo", Type = CategoryType.Expense }
);
        }
    }
}