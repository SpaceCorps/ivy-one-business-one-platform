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
        var searchQuery = this.UseState("");
        var isNewTimesheetOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        var query = context.Timesheets.Include(t => t.Project).AsQueryable();
        
        if (!string.IsNullOrEmpty(searchQuery.Value))
        {
            var searchPattern = $"%{searchQuery.Value}%";
            query = query.Where(t => 
                EF.Functions.Like(t.EmployeeName, searchPattern) ||
                EF.Functions.Like(t.Description, searchPattern) ||
                EF.Functions.Like(t.Status, searchPattern));
        }
        
        var timesheets = query.OrderByDescending(t => t.Date).ToList();
        
        var listItems = timesheets.Select(ts => new ListItem(
            title: $"{ts.EmployeeName} - {ts.Project?.Name ?? "No Project"}",
            subtitle: $"{ts.Date:MMM dd, yyyy} - {ts.HoursWorked:F1}h - {ts.Description}",
            icon: Icons.Clock,
            badge: ts.Status,
            onClick: _ => blades.Push(this, new TimesheetDetailBlade(ts.Id, () => refreshToken.Refresh()), $"Timesheet {ts.Date:MMM dd}")
        ));
        
        var mainContent = BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(4)
                .Add(searchQuery.ToSearchInput().Placeholder("Search by employee, description, or status..."))
                .Add(new Button("Log Time")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => isNewTimesheetOpen.Set(true))),
            timesheets.Count == 0 
                ? Text.Block("No timesheet entries. Log your first entry!")
                : new List(listItems)
        );

        return isNewTimesheetOpen.Value ? new Sheet(
            (Event<Sheet> _) => isNewTimesheetOpen.Set(false),
            new TimesheetFormSheet(null, () => {
                isNewTimesheetOpen.Set(false);
                refreshToken.Refresh();
            }),
            title: "Log Time",
            description: "Create a new timesheet entry"
        ).Width(Size.Fraction(1/3f)) : mainContent;
    }
}

public class TimesheetDetailBlade(int timesheetId, Action? onRefresh = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var initialTimesheet = context.Timesheets.Include(t => t.Project).FirstOrDefault(t => t.Id == timesheetId);
        
        if (initialTimesheet == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Timesheet Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var timesheetData = this.UseState(initialTimesheet);
        var isEditOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        this.UseEffect(() =>
        {
            var updatedTimesheet = context.Timesheets.Include(t => t.Project).FirstOrDefault(t => t.Id == timesheetId);
            if (updatedTimesheet != null)
            {
                timesheetData.Set(updatedTimesheet);
            }
        }, [refreshToken.ToTrigger()]);
        
        var statusBadge = new Badge(timesheetData.Value.Status)
            .Variant(timesheetData.Value.Status == "Approved" ? BadgeVariant.Success :
                   timesheetData.Value.Status == "Rejected" ? BadgeVariant.Destructive :
                   timesheetData.Value.Status == "Submitted" ? BadgeVariant.Primary :
                   BadgeVariant.Secondary);

        var timesheetDetails = new
        {
            EmployeeName = timesheetData.Value.EmployeeName,
            Project = timesheetData.Value.Project?.Name ?? "No Project",
            Date = timesheetData.Value.Date.ToString("MMM dd, yyyy"),
            HoursWorked = $"{timesheetData.Value.HoursWorked:F1}h",
            Description = timesheetData.Value.Description,
            Status = statusBadge
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3($"Timesheet - {timesheetData.Value.Date:MMM dd, yyyy}"))
            .Add(timesheetDetails.ToDetails().RemoveEmpty().MultiLine(x => x.Description))
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(timesheetData.Value.Status == "Draft" ? new Button("Submit for Approval", _ => {
                    var dbTimesheet = context.Timesheets.FirstOrDefault(t => t.Id == timesheetId);
                    if (dbTimesheet != null)
                    {
                        dbTimesheet.Status = "Submitted";
                        dbTimesheet.UpdatedAt = DateTime.UtcNow;
                        context.SaveChanges();
                        client.Toast($"Timesheet submitted for approval!");
                        refreshToken.Refresh();
                        onRefresh?.Invoke();
                    }
                })
                    .Variant(ButtonVariant.Success)
                    .Icon(Icons.Send)
                    : null)
                .Add(timesheetData.Value.Status == "Submitted" ? new Button("Approve", _ => {
                    var dbTimesheet = context.Timesheets.FirstOrDefault(t => t.Id == timesheetId);
                    if (dbTimesheet != null)
                    {
                        dbTimesheet.Status = "Approved";
                        dbTimesheet.UpdatedAt = DateTime.UtcNow;
                        context.SaveChanges();
                        client.Toast($"Timesheet approved!");
                        refreshToken.Refresh();
                        onRefresh?.Invoke();
                    }
                })
                    .Variant(ButtonVariant.Success)
                    .Icon(Icons.Check)
                    : null)
                .Add(timesheetData.Value.Status == "Submitted" ? new Button("Reject", _ => {
                    var dbTimesheet = context.Timesheets.FirstOrDefault(t => t.Id == timesheetId);
                    if (dbTimesheet != null)
                    {
                        dbTimesheet.Status = "Rejected";
                        dbTimesheet.UpdatedAt = DateTime.UtcNow;
                        context.SaveChanges();
                        client.Toast($"Timesheet rejected!");
                        refreshToken.Refresh();
                        onRefresh?.Invoke();
                    }
                })
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.X)
                    : null)
                .Add(new Button("Edit Timesheet")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .HandleClick(_ => isEditOpen.Set(true)))
                .Add(new Button("Delete Timesheet")
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.Trash)
                    .HandleClick(_ => {
                        try
                        {
                            var timesheetToDelete = context.Timesheets.FirstOrDefault(t => t.Id == timesheetId);
                            if (timesheetToDelete != null)
                            {
                                context.Timesheets.Remove(timesheetToDelete);
                                context.SaveChanges();
                                client.Toast($"Timesheet deleted successfully!");
                                refreshToken.Refresh();
                                onRefresh?.Invoke();
                                blades.Pop();
                            }
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error deleting timesheet: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)))
            .Add(isEditOpen.Value ? new Sheet(
                (Event<Sheet> _) => isEditOpen.Set(false),
                new TimesheetFormSheet(timesheetId, () => {
                    isEditOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Edit Timesheet",
                description: $"Edit timesheet entry"
            ).Width(Size.Fraction(1/3f)) : null);
    }
}

public class TimesheetFormSheet(int? timesheetId = null, Action? onClose = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = timesheetId.HasValue;
        var existingTimesheet = isEdit ? context.Timesheets.FirstOrDefault(t => t.Id == timesheetId!.Value) : null;
        
        var projects = context.Projects.Where(p => p.Status != "Completed").OrderBy(p => p.Name).ToList();
        
        var timesheetForm = this.UseState(existingTimesheet ?? new Timesheet
        {
            ProjectId = projects.FirstOrDefault()?.Id ?? 0,
            EmployeeName = "",
            Date = DateTime.UtcNow,
            HoursWorked = 0.0m,
            Description = "",
            Status = "Draft"
        });
        
        var statusOptions = new[] { "Draft", "Submitted", "Approved", "Rejected" };
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(timesheetForm.Value.EmployeeName))
                            {
                                client.Toast("Employee Name is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(timesheetForm.Value.Description))
                            {
                                client.Toast("Description is required", "Validation Error");
                                return;
                            }
                            
                            if (timesheetForm.Value.HoursWorked < 0.1m || timesheetForm.Value.HoursWorked > 24)
                            {
                                client.Toast("Hours worked must be between 0.1 and 24", "Validation Error");
                                return;
                            }
                            
                            if (timesheetForm.Value.ProjectId == 0)
                            {
                                client.Toast("Project is required", "Validation Error");
                                return;
                            }
                            
                            if (isEdit && existingTimesheet != null)
                            {
                                existingTimesheet.ProjectId = timesheetForm.Value.ProjectId;
                                existingTimesheet.EmployeeName = timesheetForm.Value.EmployeeName;
                                existingTimesheet.Date = timesheetForm.Value.Date;
                                existingTimesheet.HoursWorked = timesheetForm.Value.HoursWorked;
                                existingTimesheet.Description = timesheetForm.Value.Description;
                                existingTimesheet.Status = timesheetForm.Value.Status;
                                existingTimesheet.UpdatedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                var newTimesheet = new Timesheet
                                {
                                    ProjectId = timesheetForm.Value.ProjectId,
                                    EmployeeName = timesheetForm.Value.EmployeeName,
                                    Date = timesheetForm.Value.Date,
                                    HoursWorked = timesheetForm.Value.HoursWorked,
                                    Description = timesheetForm.Value.Description,
                                    Status = timesheetForm.Value.Status,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                context.Timesheets.Add(newTimesheet);
                            }
                            
                            context.SaveChanges();
                            client.Toast(isEdit ? "Timesheet updated successfully!" : "Timesheet created successfully!");
                            onClose?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)
                    .HandleClick(_ => onClose?.Invoke())),
            
            Layout.Vertical().Gap(4)
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Timesheet Information"))
                        .Add(Text.Small("Employee Name"))
                        .Add(new TextInput(timesheetForm.Value.EmployeeName, e => {
                            var updated = timesheetForm.Value;
                            updated.EmployeeName = e.Value;
                            timesheetForm.Set(updated);
                        }).Placeholder("John Doe"))
                        .Add(Text.Small("Project"))
                        .Add(new SelectInput<int>(timesheetForm.Value.ProjectId, e => {
                            var updated = timesheetForm.Value;
                            updated.ProjectId = e.Value;
                            timesheetForm.Set(updated);
                        }, projects.Select(p => (p.Id, p.Name)).ToOptions()))
                        .Add(Text.Small("Date"))
                        .Add(new DateTimeInput<DateTime>(timesheetForm.Value.Date, e => {
                            var updated = timesheetForm.Value;
                            updated.Date = e.Value;
                            timesheetForm.Set(updated);
                        }))
                        .Add(Text.Small("Hours Worked"))
                        .Add(new NumberInput<decimal>(timesheetForm.Value.HoursWorked, v => {
                            var updated = timesheetForm.Value;
                            updated.HoursWorked = v;
                            timesheetForm.Set(updated);
                        }).Placeholder("8.0"))
                        .Add(Text.Small("Description"))
                        .Add(new TextInput(timesheetForm.Value.Description, e => {
                            var updated = timesheetForm.Value;
                            updated.Description = e.Value;
                            timesheetForm.Set(updated);
                        }).Placeholder("Work description...").Variant(TextInputs.Textarea))
                        .Add(Text.Small("Status"))
                        .Add(new SelectInput<string>(timesheetForm.Value.Status, e => {
                            var updated = timesheetForm.Value;
                            updated.Status = e.Value;
                            timesheetForm.Set(updated);
                        }, statusOptions.ToOptions()))
                ).Title("Timesheet Details"))
        );
    }
}
