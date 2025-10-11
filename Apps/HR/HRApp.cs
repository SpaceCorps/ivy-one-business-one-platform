using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.HR;

[App(icon: Icons.User, title: "HR", path: new[] { "Human Resources" })]
public class HRApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var employees = context.Employees.Include(e => e.Department).ToList();
        var departments = context.Departments.ToList();
        var activeEmployees = employees.Count(e => e.Status == "Active");
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Human Resources")
                .Add("Manage employees, departments, and HR operations")
                .Add(Layout.Grid()
                    .Columns(3)
                    .Gap(12)
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Total Employees").Add(employees.Count.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Active").Add(activeEmployees.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Departments").Add(departments.Count.ToString()))))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Add Employee", _ => client.Toast("Adding new employee"))
                        .Variant(ButtonVariant.Primary)
                        .Icon(Icons.Plus))
                    .Add(new Button("Departments", _ => client.Toast("Manage departments"))
                        .Variant(ButtonVariant.Secondary))
                    .Add(new Button("Payroll", _ => client.Toast("Process payroll"))
                        .Variant(ButtonVariant.Outline)))
                .Add(BuildEmployeesList(employees, client))
        );
    }

    private object BuildEmployeesList(List<Employee> employees, IClientProvider client)
    {
        if (employees.Count == 0)
            return new Card(Layout.Vertical().Padding(16).Add("No employees yet. Add your first employee!"));
        
        var cards = employees.Take(10).Select(emp => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(16)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add($"{emp.FirstName} {emp.LastName}")
                    .Add(emp.JobTitle)
                    .Add($"Department: {emp.Department?.Name ?? "Unassigned"}")
                    .Add(Layout.Horizontal()
                        .Gap(8)
                        .Add(new Badge(emp.Status)
                            .Variant(emp.Status == "Active" ? BadgeVariant.Success : BadgeVariant.Secondary))
                        .Add(new Badge(emp.Email).Variant(BadgeVariant.Outline))
                        .Add(new Badge($"${emp.Salary:N0}/year").Variant(BadgeVariant.Info))))
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add($"Hire: {emp.HireDate:MMM dd, yyyy}")
                    .Add(Layout.Horizontal()
                        .Gap(4)
                        .Add(new Button("View", _ => client.Toast($"Viewing: {emp.FirstName} {emp.LastName}"))
                            .Small()
                            .Variant(ButtonVariant.Primary))
                        .Add(new Button("Edit", _ => client.Toast($"Editing: {emp.FirstName} {emp.LastName}"))
                            .Small()
                            .Variant(ButtonVariant.Outline))))
        ));
        
        return Layout.Vertical().Gap(8).Add(cards);
    }
}
