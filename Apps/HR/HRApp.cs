using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.HR;

[App(icon: Icons.User, title: "HR", path: new[] { "Human Resources" })]
public class HRApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new HRRootBlade(), "HR", Size.Units(110));
    }
}

public class HRRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var searchQuery = this.UseState("");
        var isNewEmployeeOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        var query = context.Employees.Include(e => e.Department).AsQueryable();
        
        if (!string.IsNullOrEmpty(searchQuery.Value))
        {
            var searchPattern = $"%{searchQuery.Value}%";
            query = query.Where(e => 
                EF.Functions.Like(e.FirstName, searchPattern) ||
                EF.Functions.Like(e.LastName, searchPattern) ||
                EF.Functions.Like(e.EmployeeId, searchPattern) ||
                EF.Functions.Like(e.JobTitle, searchPattern) ||
                EF.Functions.Like(e.Email, searchPattern));
        }
        
        var employees = query.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ToList();
        
        var listItems = employees.Select(emp => new ListItem(
            title: $"{emp.FirstName} {emp.LastName}",
            subtitle: $"{emp.JobTitle} - {emp.Department?.Name ?? "Unassigned"} - ${emp.Salary:N0}/year",
            icon: Icons.User,
            badge: emp.Status,
            onClick: _ => blades.Push(this, new EmployeeDetailBlade(emp.Id, () => refreshToken.Refresh()), $"{emp.FirstName} {emp.LastName}")
        ));
        
        var mainContent = BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(4)
                .Add(searchQuery.ToSearchInput().Placeholder("Search by name, ID, job title, or email..."))
                .Add(new Button("Add Employee")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => isNewEmployeeOpen.Set(true))),
            employees.Count == 0 
                ? Text.Block("No employees found. Try a different search or add your first employee!")
                : new List(listItems)
        );

        return isNewEmployeeOpen.Value ? new Sheet(
            (Event<Sheet> _) => isNewEmployeeOpen.Set(false),
            new EmployeeFormSheet(null, () => {
                isNewEmployeeOpen.Set(false);
                refreshToken.Refresh();
            }),
            title: "New Employee",
            description: "Add a new employee to the system"
            ).Width(Size.Fraction(1/3f)) : mainContent;
    }
}

public class EmployeeDetailBlade(int employeeId, Action? onRefresh = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var initialEmployee = context.Employees.Include(e => e.Department).FirstOrDefault(e => e.Id == employeeId);
        
        if (initialEmployee == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Employee Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var employeeData = this.UseState(initialEmployee);
        var isEditOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        // Refresh employee data when refresh token changes
        this.UseEffect(() =>
        {
            var updatedEmployee = context.Employees.Include(e => e.Department).FirstOrDefault(e => e.Id == employeeId);
            if (updatedEmployee != null)
            {
                employeeData.Set(updatedEmployee);
            }
        }, [refreshToken.ToTrigger()]);
        
        var statusBadge = new Badge(employeeData.Value.Status)
            .Variant(employeeData.Value.Status == "Active" ? BadgeVariant.Success :
                   employeeData.Value.Status == "Inactive" ? BadgeVariant.Secondary :
                   BadgeVariant.Destructive);

        var employeeDetails = new
        {
            EmployeeID = employeeData.Value.EmployeeId,
            Name = $"{employeeData.Value.FirstName} {employeeData.Value.LastName}",
            JobTitle = employeeData.Value.JobTitle,
            Department = employeeData.Value.Department?.Name ?? "Unassigned",
            Email = employeeData.Value.Email,
            Phone = employeeData.Value.Phone,
            Salary = $"${employeeData.Value.Salary:N0}/year",
            HireDate = employeeData.Value.HireDate.ToString("MMM dd, yyyy"),
            TerminationDate = employeeData.Value.TerminationDate?.ToString("MMM dd, yyyy"),
            Status = statusBadge
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3($"{employeeData.Value.FirstName} {employeeData.Value.LastName}"))
            .Add(employeeDetails.ToDetails().RemoveEmpty())
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(new Button("Edit Employee")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .HandleClick(_ => isEditOpen.Set(true)))
                .Add(new Button("Delete Employee")
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.Trash)
                    .HandleClick(_ => {
                        try
                        {
                            var employeeToDelete = context.Employees.FirstOrDefault(e => e.Id == employeeId);
                            if (employeeToDelete != null)
                            {
                                context.Employees.Remove(employeeToDelete);
                                context.SaveChanges();
                                client.Toast($"Employee {employeeData.Value.FirstName} {employeeData.Value.LastName} deleted successfully!");
                                refreshToken.Refresh();
                                onRefresh?.Invoke();
                                blades.Pop();
                            }
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error deleting employee: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)))
            .Add(isEditOpen.Value ? new Sheet(
                (Event<Sheet> _) => isEditOpen.Set(false),
                new EmployeeFormSheet(employeeId, () => {
                    isEditOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Edit Employee",
                description: $"Edit employee {employeeData.Value.FirstName} {employeeData.Value.LastName}"
            ).Width(Size.Fraction(1/3f)) : null);
    }
}

public class EmployeeFormSheet(int? employeeId = null, Action? onClose = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = employeeId.HasValue;
        var existingEmployee = isEdit ? context.Employees.FirstOrDefault(e => e.Id == employeeId!.Value) : null;
        
        var departments = context.Departments.Where(d => d.IsActive).OrderBy(d => d.Name).ToList();
        
        var employeeForm = this.UseState(existingEmployee ?? new Employee
        {
            EmployeeId = $"EMP-{DateTime.Now:yyyyMMddHHmmss}",
            FirstName = "",
            LastName = "",
            Email = "",
            Phone = "",
            JobTitle = "",
            Salary = 50000,
            Status = "Active",
            HireDate = DateTime.UtcNow,
            DepartmentId = departments.FirstOrDefault()?.Id ?? 0
        });
        
        var statusOptions = new[] { "Active", "Inactive", "Terminated" };
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => {
                        try
                        {
                            // Client-side validation
                            if (string.IsNullOrWhiteSpace(employeeForm.Value.EmployeeId))
                            {
                                client.Toast("Employee ID is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(employeeForm.Value.FirstName))
                            {
                                client.Toast("First Name is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(employeeForm.Value.LastName))
                            {
                                client.Toast("Last Name is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(employeeForm.Value.Email))
                            {
                                client.Toast("Email is required", "Validation Error");
                                return;
                            }
                            
                            if (employeeForm.Value.Salary <= 0)
                            {
                                client.Toast("Salary must be greater than zero", "Validation Error");
                                return;
                            }
                            
                            if (employeeForm.Value.FirstName.Length > 100)
                            {
                                client.Toast("First Name cannot exceed 100 characters", "Validation Error");
                                return;
                            }
                            
                            if (employeeForm.Value.LastName.Length > 100)
                            {
                                client.Toast("Last Name cannot exceed 100 characters", "Validation Error");
                                return;
                            }
                            
                            if (isEdit && existingEmployee != null)
                            {
                                // Update existing employee
                                existingEmployee.EmployeeId = employeeForm.Value.EmployeeId;
                                existingEmployee.FirstName = employeeForm.Value.FirstName;
                                existingEmployee.LastName = employeeForm.Value.LastName;
                                existingEmployee.Email = employeeForm.Value.Email;
                                existingEmployee.Phone = employeeForm.Value.Phone;
                                existingEmployee.JobTitle = employeeForm.Value.JobTitle;
                                existingEmployee.Salary = employeeForm.Value.Salary;
                                existingEmployee.Status = employeeForm.Value.Status;
                                existingEmployee.HireDate = employeeForm.Value.HireDate;
                                existingEmployee.TerminationDate = employeeForm.Value.TerminationDate;
                                existingEmployee.DepartmentId = employeeForm.Value.DepartmentId;
                                existingEmployee.UpdatedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                // Create new employee
                                var newEmployee = new Employee
                                {
                                    EmployeeId = employeeForm.Value.EmployeeId,
                                    FirstName = employeeForm.Value.FirstName,
                                    LastName = employeeForm.Value.LastName,
                                    Email = employeeForm.Value.Email,
                                    Phone = employeeForm.Value.Phone,
                                    JobTitle = employeeForm.Value.JobTitle,
                                    Salary = employeeForm.Value.Salary,
                                    Status = employeeForm.Value.Status,
                                    HireDate = employeeForm.Value.HireDate,
                                    TerminationDate = employeeForm.Value.TerminationDate,
                                    DepartmentId = employeeForm.Value.DepartmentId,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                context.Employees.Add(newEmployee);
                            }
                            
                            context.SaveChanges();
                            client.Toast(isEdit ? "Employee updated successfully!" : "Employee created successfully!");
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
                        .Add(Text.Small("Personal Information"))
                        .Add(Text.Small("Employee ID"))
                        .Add(new TextInput(employeeForm.Value.EmployeeId, e => {
                            var updated = employeeForm.Value;
                            updated.EmployeeId = e.Value;
                            employeeForm.Set(updated);
                        }).Placeholder("EMP-001").Disabled(isEdit))
                        .Add(Text.Small("First Name"))
                        .Add(new TextInput(employeeForm.Value.FirstName, e => {
                            var updated = employeeForm.Value;
                            updated.FirstName = e.Value;
                            employeeForm.Set(updated);
                        }).Placeholder("John"))
                        .Add(Text.Small("Last Name"))
                        .Add(new TextInput(employeeForm.Value.LastName, e => {
                            var updated = employeeForm.Value;
                            updated.LastName = e.Value;
                            employeeForm.Set(updated);
                        }).Placeholder("Doe"))
                        .Add(Text.Small("Email"))
                        .Add(new TextInput(employeeForm.Value.Email, e => {
                            var updated = employeeForm.Value;
                            updated.Email = e.Value;
                            employeeForm.Set(updated);
                        }).Placeholder("john.doe@company.com"))
                        .Add(Text.Small("Phone"))
                        .Add(new TextInput(employeeForm.Value.Phone, e => {
                            var updated = employeeForm.Value;
                            updated.Phone = e.Value;
                            employeeForm.Set(updated);
                        }).Placeholder("+1-555-0123"))
                ).Title("Employee Details"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Job Information"))
                        .Add(Text.Small("Job Title"))
                        .Add(new TextInput(employeeForm.Value.JobTitle, e => {
                            var updated = employeeForm.Value;
                            updated.JobTitle = e.Value;
                            employeeForm.Set(updated);
                        }).Placeholder("Software Engineer"))
                        .Add(Text.Small("Department"))
                        .Add(new SelectInput<int>(employeeForm.Value.DepartmentId, e => {
                            var updated = employeeForm.Value;
                            updated.DepartmentId = e.Value;
                            employeeForm.Set(updated);
                        }, departments.Select(d => new SelectOption { Label = d.Name, Value = d.Id }).ToArray()))
                        .Add(Text.Small("Salary ($)"))
                        .Add(new NumberInput<decimal>(employeeForm.Value.Salary, v => {
                            var updated = employeeForm.Value;
                            updated.Salary = v;
                            employeeForm.Set(updated);
                        }).Placeholder("50000"))
                        .Add(Text.Small("Status"))
                        .Add(new SelectInput<string>(employeeForm.Value.Status, e => {
                            var updated = employeeForm.Value;
                            updated.Status = e.Value;
                            employeeForm.Set(updated);
                        }, statusOptions.ToOptions()))
                ).Title("Position"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Employment Dates"))
                        .Add(Text.Small("Hire Date"))
                        .Add(new DateTimeInput<DateTime>(employeeForm.Value.HireDate, e => {
                            var updated = employeeForm.Value;
                            updated.HireDate = e.Value;
                            employeeForm.Set(updated);
                        }))
                        .Add(Text.Small("Termination Date (Optional)"))
                        .Add(new DateTimeInput<DateTime>(employeeForm.Value.TerminationDate ?? DateTime.UtcNow, e => {
                            var updated = employeeForm.Value;
                            updated.TerminationDate = e.Value;
                            employeeForm.Set(updated);
                        }))
                ).Title("Timeline"))
        );
    }
}
