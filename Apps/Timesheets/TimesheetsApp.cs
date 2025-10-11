using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Timesheets;

[App(icon: Icons.Clock, title: "Timesheets", path: new[] { "Project & Time Management" })]
public class TimesheetsApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var timesheets = context.Timesheets.Include(t => t.Project).OrderByDescending(t => t.Date).ToList();
        var thisWeek = timesheets.Where(t => t.Date >= DateTime.Now.AddDays(-7)).Sum(t => t.HoursWorked);
        var pending = timesheets.Count(t => t.Status == "Draft" || t.Status == "Submitted");
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Timesheet Management")
                .Add("Track work hours and submit timesheets")
                .Add(Layout.Grid()
                    .Columns(3)
                    .Gap(12)
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("This Week").Add($"{thisWeek:F1}h")))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Pending").Add(pending.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Total Entries").Add(timesheets.Count.ToString()))))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Log Time", _ => client.Toast("Log time entry"))
                        .Icon(Icons.Plus)
                        .Variant(ButtonVariant.Primary))
                    .Add(new Button("Submit Week", _ => client.Toast("Submit timesheet for approval"))
                        .Variant(ButtonVariant.Success)))
                .Add(BuildTimesheetsList(timesheets, client))
        );
    }

    private object BuildTimesheetsList(List<Timesheet> timesheets, IClientProvider client)
    {
        if (timesheets.Count == 0)
            return new Card(Layout.Vertical().Padding(16).Add("No time entries found. Log your first entry!"));
        
        var cards = timesheets.Take(10).Select(ts => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(16)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(ts.EmployeeName)
                    .Add(ts.Project?.Name ?? "No Project")
                    .Add(ts.Description)
                    .Add(Layout.Horizontal()
                        .Gap(8)
                        .Add(new Badge(ts.Status)
                            .Variant(ts.Status == "Approved" ? BadgeVariant.Success :
                                   ts.Status == "Rejected" ? BadgeVariant.Destructive :
                                   BadgeVariant.Secondary))
                        .Add(new Badge($"{ts.HoursWorked:F1}h").Variant(BadgeVariant.Info))))
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(ts.Date.ToString("MMM dd, yyyy"))
                    .Add(new Button("Edit", _ => client.Toast($"Editing timesheet"))
                        .Small()
                        .Variant(ButtonVariant.Outline)))
        ));
        
        return Layout.Vertical().Gap(8).Add(cards);
    }
}
