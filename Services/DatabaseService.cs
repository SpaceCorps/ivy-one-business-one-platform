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

        var workOrders = new[]
        {
            new WorkOrder
            {
                WorkOrderNumber = "WO-001",
                ProductName = "Widget A",
                Quantity = 500,
                Status = WorkOrderStatus.InProduction,
                Progress = 75,
                Priority = WorkOrderPriority.High,
                PlannedStartDate = DateTime.UtcNow.AddDays(-5),
                PlannedEndDate = DateTime.UtcNow.AddDays(5),
                StartDate = DateTime.UtcNow.AddDays(-3),
                Notes = "High priority order for key customer"
            },
            new WorkOrder
            {
                WorkOrderNumber = "WO-002",
                ProductName = "Component B",
                Quantity = 1000,
                Status = WorkOrderStatus.Scheduled,
                Progress = 0,
                Priority = WorkOrderPriority.Normal,
                PlannedStartDate = DateTime.UtcNow.AddDays(2),
                PlannedEndDate = DateTime.UtcNow.AddDays(10),
                Notes = "Standard component production run"
            },
            new WorkOrder
            {
                WorkOrderNumber = "WO-003",
                ProductName = "Assembly C",
                Quantity = 250,
                Status = WorkOrderStatus.Completed,
                Progress = 100,
                Priority = WorkOrderPriority.Normal,
                PlannedStartDate = DateTime.UtcNow.AddDays(-10),
                PlannedEndDate = DateTime.UtcNow.AddDays(-2),
                StartDate = DateTime.UtcNow.AddDays(-9),
                CompletionDate = DateTime.UtcNow.AddDays(-1)
            },
            new WorkOrder
            {
                WorkOrderNumber = "WO-004",
                ProductName = "Custom Part D",
                Quantity = 100,
                Status = WorkOrderStatus.Scheduled,
                Progress = 0,
                Priority = WorkOrderPriority.Urgent,
                PlannedStartDate = DateTime.UtcNow,
                PlannedEndDate = DateTime.UtcNow.AddDays(3),
                Notes = "Urgent custom order - expedite shipping"
            }
        };
        await context.WorkOrders.AddRangeAsync(workOrders);
        // Seed purchase orders
        var purchaseOrders = new[]
        {
            new PurchaseOrder 
            { 
                OrderNumber = "PO-001", 
                Supplier = "Office Supplies Co", 
                Amount = 1250.00m, 
                Status = PurchaseOrderStatus.Pending,
                OrderDate = DateTime.UtcNow.AddDays(-7),
                ExpectedDeliveryDate = DateTime.UtcNow.AddDays(14),
                PaymentTerms = "Net 30",
                Department = "Operations",
                Notes = "Monthly office supplies order"
            },
            new PurchaseOrder 
            { 
                OrderNumber = "PO-002", 
                Supplier = "Tech Equipment Ltd", 
                Amount = 5400.00m, 
                Status = PurchaseOrderStatus.Approved,
                OrderDate = DateTime.UtcNow.AddDays(-14),
                ExpectedDeliveryDate = DateTime.UtcNow.AddDays(7),
                PaymentTerms = "Net 45",
                Department = "IT",
                Notes = "New laptops for development team"
            },
            new PurchaseOrder 
            { 
                OrderNumber = "PO-003", 
                Supplier = "Furniture World", 
                Amount = 3200.00m, 
                Status = PurchaseOrderStatus.Received,
                OrderDate = DateTime.UtcNow.AddDays(-30),
                ExpectedDeliveryDate = DateTime.UtcNow.AddDays(-10),
                PaymentTerms = "Net 30",
                Department = "Facilities",
                Notes = "Conference room furniture"
            }
        };

        await context.PurchaseOrders.AddRangeAsync(purchaseOrders);
        await context.SaveChangesAsync();
        
        // Seed tickets
        var tickets = new[]
        {
            new Ticket { TicketNumber = "HD-001", CustomerName = "John Doe", CustomerEmail = "john.doe@example.com", Subject = "Login Issue", Description = "Unable to login to the system", Status = TicketStatus.Open, Priority = TicketPriority.High, Category = "Technical", CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new Ticket { TicketNumber = "HD-002", CustomerName = "Jane Smith", CustomerEmail = "jane.smith@example.com", Subject = "Feature Request", Description = "Request for new reporting feature", Status = TicketStatus.InProgress, Priority = TicketPriority.Medium, Category = "Feature", AssignedTo = "Support Agent", CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new Ticket { TicketNumber = "HD-003", CustomerName = "Bob Johnson", CustomerEmail = "bob.johnson@example.com", Subject = "Bug Report", Description = "Found a bug in the invoice module", Status = TicketStatus.Resolved, Priority = TicketPriority.Low, Category = "Bug", AssignedTo = "Tech Support", ResolvedAt = DateTime.UtcNow.AddDays(-1), CreatedAt = DateTime.UtcNow.AddDays(-7) },
            new Ticket { TicketNumber = "HD-004", CustomerName = "Alice Brown", CustomerEmail = "alice.brown@example.com", Subject = "Password Reset", Description = "Need help resetting my password", Status = TicketStatus.Closed, Priority = TicketPriority.Low, Category = "Account", AssignedTo = "Support Agent", ResolvedAt = DateTime.UtcNow.AddDays(-2), CreatedAt = DateTime.UtcNow.AddDays(-8) }
        };
        
        await context.Tickets.AddRangeAsync(tickets);
        await context.SaveChangesAsync();
    }
}