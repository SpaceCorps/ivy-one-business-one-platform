using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.HR;

[App(icon: Icons.User, title: "HR", path: new[] { "Human Resources" })]
public class HRApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new HRRootBlade(), "HR", Size.Units(80));
    }
}

public class HRRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var employees = context.Employees.Include(e => e.Department).ToList();
        
        var listItems = employees.Select(emp => new ListItem(
            title: $"{emp.FirstName} {emp.LastName}",
            subtitle: $"{emp.JobTitle} - {emp.Department?.Name ?? "Unassigned"} - ${emp.Salary:N0}/year",
            icon: Icons.User,
            badge: emp.Status,
            onClick: _ => { blades.Push(this, new EmployeeDetailBlade(emp.Id), $"{emp.FirstName} {emp.LastName}"); return default; }
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Text.H3("Human Resources"))
            .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Total Employees").Add(employees.Count.ToString())))
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(new Button("Add Employee", _ => client.Toast("Add employee"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(employees.Count == 0 
                ? Text.Block("No employees yet.")
                : new List(listItems));
    }
}

public class EmployeeDetailBlade(int employeeId) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var employee = context.Employees.Include(e => e.Department).FirstOrDefault(e => e.Id == employeeId);
        
        if (employee == null)
            return "Employee not found";
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add($"{employee.FirstName} {employee.LastName}")
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add($"Employee ID: {employee.EmployeeId}")
                    .Add($"Job Title: {employee.JobTitle}")
                    .Add($"Department: {employee.Department?.Name ?? "Unassigned"}")
                    .Add($"Email: {employee.Email}")
                    .Add($"Phone: {employee.Phone}")
                    .Add($"Salary: ${employee.Salary:N0}/year")
                    .Add($"Hire Date: {employee.HireDate:MMM dd, yyyy}")
                    .Add(new Badge(employee.Status)
                        .Variant(employee.Status == "Active" ? BadgeVariant.Success : BadgeVariant.Secondary)))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Edit", _ => client.Toast("Edit employee"))
                        .Variant(ButtonVariant.Primary))
                    .Add(new Button("Payroll", _ => client.Toast("View payroll"))
                        .Variant(ButtonVariant.Secondary)))
        );
    }
}
