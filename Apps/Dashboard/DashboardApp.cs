using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Dashboard;

[App(icon: Icons.Grid3x3, title: "Dashboard", path: new[] { "Dashboard" })]
public class DashboardApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var invoices = context.Invoices.ToList();
        var contacts = context.Contacts.ToList();
        var projects = context.Projects.ToList();
        var employees = context.Employees.ToList();
        
        var revenue = invoices.Where(i => i.Status == "Paid").Sum(i => i.TotalAmount);
        var activeProjects = projects.Count(p => p.Status == "In Progress");
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Business Analytics Dashboard")
                .Add("Monitor key metrics across all business operations")
                .Add(Layout.Grid()
                    .Columns(4)
                    .Gap(12)
                    .Add(new Card(
                        Layout.Vertical()
                            .Gap(8)
                            .Padding(16)
                            .Add(new Icon(Icons.DollarSign))
                            .Add("Revenue")
                            .Add($"${revenue:N2}")
                            .Add(new Badge("From invoices").Variant(BadgeVariant.Success))))
                    .Add(new Card(
                        Layout.Vertical()
                            .Gap(8)
                            .Padding(16)
                            .Add(new Icon(Icons.Users))
                            .Add("Contacts")
                            .Add(contacts.Count.ToString())
                            .Add(new Badge("In CRM").Variant(BadgeVariant.Info))))
                    .Add(new Card(
                        Layout.Vertical()
                            .Gap(8)
                            .Padding(16)
                            .Add(new Icon(Icons.Check))
                            .Add("Active Projects")
                            .Add(activeProjects.ToString())
                            .Add(new Badge("In progress").Variant(BadgeVariant.Primary))))
                    .Add(new Card(
                        Layout.Vertical()
                            .Gap(8)
                            .Padding(16)
                            .Add(new Icon(Icons.User))
                            .Add("Employees")
                            .Add(employees.Count.ToString())
                            .Add(new Badge("Total staff").Variant(BadgeVariant.Secondary)))))
                .Add(Layout.Grid()
                    .Columns(2)
                    .Gap(16)
                    .Add(new Card(
                        Layout.Vertical()
                            .Gap(12)
                            .Padding(16)
                            .Add("Recent Activity")
                            .Add(Layout.Vertical()
                                .Gap(8)
                                .Add(new Card(Layout.Horizontal().Gap(8).Padding(8)
                                    .Add(new Badge("Invoice").Variant(BadgeVariant.Success))
                                    .Add("New invoice created")))
                                .Add(new Card(Layout.Horizontal().Gap(8).Padding(8)
                                    .Add(new Badge("Contact").Variant(BadgeVariant.Info))
                                    .Add("Contact added to CRM")))
                                .Add(new Card(Layout.Horizontal().Gap(8).Padding(8)
                                    .Add(new Badge("Project").Variant(BadgeVariant.Primary))
                                    .Add("Project milestone completed"))))))
                    .Add(new Card(
                        Layout.Vertical()
                            .Gap(12)
                            .Padding(16)
                            .Add("Quick Actions")
                            .Add(Layout.Vertical()
                                .Gap(8)
                                .Add(new Button("Create Invoice", _ => client.Toast("Opening Accounting"))
                                    .Variant(ButtonVariant.Primary)
                                    .Icon(Icons.Plus))
                                .Add(new Button("Add Contact", _ => client.Toast("Opening CRM"))
                                    .Variant(ButtonVariant.Secondary)
                                    .Icon(Icons.Users))
                                .Add(new Button("New Project", _ => client.Toast("Opening Projects"))
                                    .Variant(ButtonVariant.Outline)
                                    .Icon(Icons.Check))))))
        );
    }
}
