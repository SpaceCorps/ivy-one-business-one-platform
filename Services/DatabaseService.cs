using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Services;

public static class DatabaseService
{
    public static void AddDatabase(this IServiceCollection services)
    {
        var connectionString = "Data Source=business_platform.db";
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));
    }

    public static async System.Threading.Tasks.Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Seed initial data if needed
        await SeedInitialDataAsync(context);
    }

    private static async System.Threading.Tasks.Task SeedInitialDataAsync(ApplicationDbContext context)
    {
        // Check if data already exists
        if (await context.Categories.AnyAsync())
            return;

        // Seed categories
        var categories = new[]
        {
            new Category { Name = "General", Description = "General articles" },
            new Category { Name = "Technical", Description = "Technical documentation" },
            new Category { Name = "FAQ", Description = "Frequently asked questions" },
            new Category { Name = "Tutorials", Description = "Step-by-step tutorials" }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        // Seed departments
        var departments = new[]
        {
            new Department { Name = "Sales", Description = "Sales department", ManagerName = "John Smith", Budget = 100000 },
            new Department { Name = "Marketing", Description = "Marketing department", ManagerName = "Jane Doe", Budget = 75000 },
            new Department { Name = "Development", Description = "Software development", ManagerName = "Bob Johnson", Budget = 200000 },
            new Department { Name = "HR", Description = "Human resources", ManagerName = "Alice Brown", Budget = 50000 }
        };

        await context.Departments.AddRangeAsync(departments);
        await context.SaveChangesAsync();

        // Seed accounts
        var accounts = new[]
        {
            new Account { AccountNumber = "1000", AccountName = "Cash", AccountType = "Asset", Balance = 50000 },
            new Account { AccountNumber = "2000", AccountName = "Accounts Receivable", AccountType = "Asset", Balance = 25000 },
            new Account { AccountNumber = "3000", AccountName = "Inventory", AccountType = "Asset", Balance = 15000 },
            new Account { AccountNumber = "4000", AccountName = "Accounts Payable", AccountType = "Liability", Balance = 10000 },
            new Account { AccountNumber = "5000", AccountName = "Sales Revenue", AccountType = "Revenue", Balance = 0 },
            new Account { AccountNumber = "6000", AccountName = "Office Expenses", AccountType = "Expense", Balance = 0 }
        };

        await context.Accounts.AddRangeAsync(accounts);
        await context.SaveChangesAsync();
    }
}
