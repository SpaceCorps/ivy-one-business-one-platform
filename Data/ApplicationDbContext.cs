using Microsoft.EntityFrameworkCore;

namespace IvyOneBusinessOnePlatform.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Accounting tables
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    // CRM tables
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Lead> Leads { get; set; }
    public DbSet<Opportunity> Opportunities { get; set; }

    // Knowledge base tables
    public DbSet<Article> Articles { get; set; }
    public DbSet<Category> Categories { get; set; }

    // Project management tables
    public DbSet<Project> Projects { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<Timesheet> Timesheets { get; set; }

    // HR tables
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }

    // Inventory tables
    public DbSet<Product> Products { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }

    // Marketing tables
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<Subscriber> Subscribers { get; set; }

    // Document management
    public DbSet<Document> Documents { get; set; }

    // Digital signatures
    public DbSet<Signature> Signatures { get; set; }

    // Subscriptions
    public DbSet<Subscription> Subscriptions { get; set; }

    // Manufacturing
    public DbSet<WorkOrder> WorkOrders { get; set; }
    
    // Purchase orders
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    
    // Helpdesk
    public DbSet<Ticket> Tickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships and constraints here
        modelBuilder.Entity<Invoice>()
            .HasMany(i => i.Payments)
            .WithOne(p => p.Invoice)
            .HasForeignKey(p => p.InvoiceId);

        modelBuilder.Entity<Project>()
            .HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId);

        modelBuilder.Entity<Contact>()
            .HasMany(c => c.Opportunities)
            .WithOne(o => o.Contact)
            .HasForeignKey(o => o.ContactId);

        modelBuilder.Entity<WorkOrder>()
            .HasOne(wo => wo.Product)
            .WithMany()
            .HasForeignKey(wo => wo.ProductId);
    }
}
