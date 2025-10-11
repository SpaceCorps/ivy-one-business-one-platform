using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Timesheets;

[App(icon: Icons.Clock, title: "Timesheets", path: new[] { "Project & Time Management" })]
public class TimesheetsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new TimesheetsRootBlade(), "Timesheets", Size.Units(100));
    }
}

public class TimesheetsRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var timesheets = context.Timesheets.Include(t => t.Project).OrderByDescending(t => t.Date).ToList();
        var thisWeek = timesheets.Where(t => t.Date >= DateTime.Now.AddDays(-7)).Sum(t => t.HoursWorked);
        
        var listItems = timesheets.Select(ts => new ListItem(
            title: $"{ts.EmployeeName} - {ts.Project?.Name ?? "No Project"}",
            subtitle: $"{ts.Date:MMM dd, yyyy} - {ts.HoursWorked:F1}h - {ts.Description}",
            icon: Icons.Clock,
            badge: ts.Status,
            onClick: _ => { blades.Push(this, new TimesheetDetailBlade(ts.Id), $"Timesheet {ts.Date:MMM dd}"); }
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Text.H3("Timesheets"))
            .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("This Week").Add($"{thisWeek:F1}h")))
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(new Button("Log Time", _ => client.Toast("Log time"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(timesheets.Count == 0 
                ? Text.Block("No timesheet entries.")
                : new List(listItems));
    }
}

public class TimesheetDetailBlade(int timesheetId) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var timesheet = context.Timesheets.Include(t => t.Project).FirstOrDefault(t => t.Id == timesheetId);
        
        if (timesheet == null)
            return "Timesheet not found";
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add($"Timesheet - {timesheet.Date:MMM dd, yyyy}")
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add($"Employee: {timesheet.EmployeeName}")
                    .Add($"Project: {timesheet.Project?.Name ?? "No Project"}")
                    .Add($"Hours: {timesheet.HoursWorked:F1}h")
                    .Add($"Description: {timesheet.Description}")
                    .Add(new Badge(timesheet.Status)
                        .Variant(timesheet.Status == "Approved" ? BadgeVariant.Success :
                               timesheet.Status == "Rejected" ? BadgeVariant.Destructive :
                               BadgeVariant.Secondary)))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Edit", _ => client.Toast("Edit timesheet"))
                        .Variant(ButtonVariant.Primary))
                    .Add(timesheet.Status == "Draft" 
                        ? new Button("Submit", _ => client.Toast("Submit for approval"))
                            .Variant(ButtonVariant.Success)
                        : null))
        );
    }
}
