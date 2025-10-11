using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Dashboard;

[App(icon: Icons.Grid3x3, title: "Dashboard", path: new[] { "Dashboard" })]
public class DashboardApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new DashboardRootBlade(), "Dashboard", Size.Units(95));
    }
}

public class DashboardRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var invoices = context.Invoices.ToList();
        var contacts = context.Contacts.ToList();
        var projects = context.Projects.ToList();
        var employees = context.Employees.ToList();
        
        var revenue = invoices.Where(i => i.Status == "Paid").Sum(i => i.TotalAmount);
        var activeProjects = projects.Count(p => p.Status == "In Progress");
        
        var metrics = new[]
        {
            new { Title = "Revenue", Value = $"${revenue:N2}", Icon = Icons.DollarSign, Badge = "Invoices" },
            new { Title = "Contacts", Value = contacts.Count.ToString(), Icon = Icons.Users, Badge = "CRM" },
            new { Title = "Projects", Value = activeProjects.ToString(), Icon = Icons.Check, Badge = "Active" },
            new { Title = "Employees", Value = employees.Count.ToString(), Icon = Icons.User, Badge = "Total" }
        };
        
        var listItems = metrics.Select(metric => new ListItem(
            title: metric.Title,
            subtitle: metric.Value,
            icon: metric.Icon,
            badge: metric.Badge,
            onClick: _ => { client.Toast($"View {metric.Title}"); return default; }
        ));
        
        var quickActions = new[]
        {
            new ListItem("Create Invoice", subtitle: "Open Accounting", icon: Icons.FileText, onClick: _ => { client.Toast("Opening Accounting"); return default; }),
            new ListItem("Add Contact", subtitle: "Open CRM", icon: Icons.UserPlus, onClick: _ => { client.Toast("Opening CRM"); return default; }),
            new ListItem("New Project", subtitle: "Open Projects", icon: Icons.FolderPlus, onClick: _ => { client.Toast("Opening Projects"); return default; })
        };
        
        return Layout.Vertical()
            .Gap(16)
            .Add(new Card(Layout.Vertical().Gap(12).Padding(16).Add("Key Metrics").Add(new List(listItems))))
            .Add(new Card(Layout.Vertical().Gap(12).Padding(16).Add("Quick Actions").Add(new List(quickActions))));
    }
}
