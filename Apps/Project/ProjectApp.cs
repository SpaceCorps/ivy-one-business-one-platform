using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Project;

public static class TaskHelper
{
    public static string GetPriorityLabel(int priority) => priority switch
    {
        1 => "Low",
        2 => "Medium",
        3 => "High",
        4 => "Critical",
        _ => "Unknown"
    };
}

[App(icon: Icons.Check, title: "Project", path: new[] { "Project & Time Management" })]
public class ProjectApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new ProjectRootBlade(), "Projects", Size.Units(100));
    }
}

public class ProjectRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var searchQuery = this.UseState("");
        var isNewProjectOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        var query = context.Projects.Include(p => p.Tasks).AsQueryable();
        
        if (!string.IsNullOrEmpty(searchQuery.Value))
        {
            var searchPattern = $"%{searchQuery.Value}%";
            query = query.Where(p => 
                EF.Functions.Like(p.Name, searchPattern) ||
                EF.Functions.Like(p.Description, searchPattern) ||
                EF.Functions.Like(p.ProjectManager, searchPattern) ||
                EF.Functions.Like(p.Status, searchPattern));
        }
        
        var projects = query.ToList();
        
        var listItems = projects.Select(proj => new ListItem(
            title: proj.Name,
            subtitle: $"{proj.Status} - {proj.Tasks.Count} tasks - ${proj.Budget:N0}",
            icon: Icons.Folder,
            badge: proj.Status,
            onClick: _ => blades.Push(this, new ProjectDetailBlade(proj.Id, () => refreshToken.Refresh()), proj.Name)
        ));
        
        var mainContent = BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(4)
                .Add(searchQuery.ToSearchInput().Placeholder("Search by name, description, manager, or status..."))
                .Add(new Button("New Project")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => isNewProjectOpen.Set(true))),
            projects.Count == 0 
                ? Text.Block("No projects found. Try a different search or create your first project!")
                : new List(listItems)
        );

        return isNewProjectOpen.Value ? new Sheet(
            (Event<Sheet> _) => isNewProjectOpen.Set(false),
            new ProjectFormSheet(null, () => {
                isNewProjectOpen.Set(false);
                refreshToken.Refresh();
            }),
            title: "New Project",
            description: "Create a new project"
        ).Width(Size.Fraction(1/3f)) : mainContent;
    }
}

public class ProjectDetailBlade(int projectId, Action? onRefresh = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var initialProject = context.Projects.Include(p => p.Tasks).FirstOrDefault(p => p.Id == projectId);
        
        if (initialProject == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Project Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var projectData = this.UseState(initialProject);
        var isEditOpen = this.UseState(false);
        var isNewTaskOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        this.UseEffect(() =>
        {
            var updatedProject = context.Projects.Include(p => p.Tasks).FirstOrDefault(p => p.Id == projectId);
            if (updatedProject != null)
            {
                projectData.Set(updatedProject);
            }
        }, [refreshToken.ToTrigger()]);
        
        var statusBadge = new Badge(projectData.Value.Status)
            .Variant(projectData.Value.Status == "Completed" ? BadgeVariant.Success :
                   projectData.Value.Status == "In Progress" ? BadgeVariant.Primary :
                   projectData.Value.Status == "On Hold" ? BadgeVariant.Destructive :
                   BadgeVariant.Secondary);

        var projectDetails = new
        {
            Name = projectData.Value.Name,
            Description = projectData.Value.Description,
            ProjectManager = projectData.Value.ProjectManager,
            Budget = $"${projectData.Value.Budget:N0}",
            StartDate = projectData.Value.StartDate.ToString("MMM dd, yyyy"),
            EndDate = projectData.Value.EndDate?.ToString("MMM dd, yyyy"),
            TasksCount = $"{projectData.Value.Tasks.Count} tasks",
            Status = statusBadge
        };
        
        var taskItems = projectData.Value.Tasks.Select(task => new ListItem(
            title: task.Name,
            subtitle: $"{task.Status} - Priority: {TaskHelper.GetPriorityLabel(task.Priority)} - {task.Assignee}",
            icon: Icons.Check,
            badge: task.Status,
            onClick: _ => blades.Push(this, new TaskDetailBlade(task.Id, () => refreshToken.Refresh()), task.Name)
        ));
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3(projectData.Value.Name))
            .Add(projectDetails.ToDetails().RemoveEmpty().MultiLine(x => x.Description))
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(new Button("Add Task")
                    .Variant(ButtonVariant.Primary)
                    .Icon(Icons.Plus)
                    .HandleClick(_ => isNewTaskOpen.Set(true)))
                .Add(new Button("Edit Project")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .HandleClick(_ => isEditOpen.Set(true)))
                .Add(new Button("Delete Project")
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.Trash)
                    .HandleClick(_ => {
                        try
                        {
                            var projectToDelete = context.Projects.FirstOrDefault(p => p.Id == projectId);
                            if (projectToDelete != null)
                            {
                                context.Projects.Remove(projectToDelete);
                                context.SaveChanges();
                                client.Toast($"Project {projectData.Value.Name} deleted successfully!");
                                refreshToken.Refresh();
                                onRefresh?.Invoke();
                                blades.Pop();
                            }
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error deleting project: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)))
            .Add(Text.H3("Tasks"))
            .Add(projectData.Value.Tasks.Any() 
                ? new List(taskItems)
                : Text.Block("No tasks yet. Add your first task!"))
            .Add(isEditOpen.Value ? new Sheet(
                (Event<Sheet> _) => isEditOpen.Set(false),
                new ProjectFormSheet(projectId, () => {
                    isEditOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Edit Project",
                description: $"Edit project {projectData.Value.Name}"
            ).Width(Size.Fraction(1/3f)) : null)
            .Add(isNewTaskOpen.Value ? new Sheet(
                (Event<Sheet> _) => isNewTaskOpen.Set(false),
                new TaskFormSheet(projectId, null, () => {
                    isNewTaskOpen.Set(false);
                    refreshToken.Refresh();
                }),
                title: "New Task",
                description: "Add a new task to this project"
            ).Width(Size.Fraction(1/3f)) : null);
    }
}

public class TaskDetailBlade(int taskId, Action? onRefresh = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var initialTask = context.Tasks.Include(t => t.Project).FirstOrDefault(t => t.Id == taskId);
        
        if (initialTask == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Task Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var taskData = this.UseState(initialTask);
        var isEditOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        this.UseEffect(() =>
        {
            var updatedTask = context.Tasks.Include(t => t.Project).FirstOrDefault(t => t.Id == taskId);
            if (updatedTask != null)
            {
                taskData.Set(updatedTask);
            }
        }, [refreshToken.ToTrigger()]);
        
        var statusBadge = new Badge(taskData.Value.Status)
            .Variant(taskData.Value.Status == "Done" ? BadgeVariant.Success :
                   taskData.Value.Status == "In Progress" ? BadgeVariant.Primary :
                   BadgeVariant.Secondary);

        var taskDetails = new
        {
            Name = taskData.Value.Name,
            Description = taskData.Value.Description,
            Project = taskData.Value.Project.Name,
            Assignee = taskData.Value.Assignee,
            Priority = TaskHelper.GetPriorityLabel(taskData.Value.Priority),
            DueDate = taskData.Value.DueDate?.ToString("MMM dd, yyyy"),
            Status = statusBadge
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3(taskData.Value.Name))
            .Add(taskDetails.ToDetails().RemoveEmpty().MultiLine(x => x.Description))
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(new Button("Edit Task")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .HandleClick(_ => isEditOpen.Set(true)))
                .Add(new Button("Delete Task")
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.Trash)
                    .HandleClick(_ => {
                        try
                        {
                            var taskToDelete = context.Tasks.FirstOrDefault(t => t.Id == taskId);
                            if (taskToDelete != null)
                            {
                                context.Tasks.Remove(taskToDelete);
                                context.SaveChanges();
                                client.Toast($"Task {taskData.Value.Name} deleted successfully!");
                                refreshToken.Refresh();
                                onRefresh?.Invoke();
                                blades.Pop();
                            }
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error deleting task: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)))
            .Add(isEditOpen.Value ? new Sheet(
                (Event<Sheet> _) => isEditOpen.Set(false),
                new TaskFormSheet(taskData.Value.ProjectId, taskId, () => {
                    isEditOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Edit Task",
                description: $"Edit task {taskData.Value.Name}"
            ).Width(Size.Fraction(1/3f)) : null);
    }
}

public class ProjectFormSheet(int? projectId = null, Action? onClose = null) : ViewBase
{
    private Data.Project CloneProject(Data.Project source) => new Data.Project
    {
        Id = source.Id,
        Name = source.Name,
        Description = source.Description,
        Status = source.Status,
        StartDate = source.StartDate,
        EndDate = source.EndDate,
        Budget = source.Budget,
        ProjectManager = source.ProjectManager,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt
    };
    
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = projectId.HasValue;
        var existingProject = isEdit ? context.Projects.FirstOrDefault(p => p.Id == projectId!.Value) : null;
        
        var projectForm = this.UseState(existingProject ?? new Data.Project
        {
            Name = "",
            Description = "",
            Status = "Planning",
            StartDate = DateTime.UtcNow,
            EndDate = null,
            Budget = 0.00m,
            ProjectManager = ""
        });
        
        var statusOptions = new[] { "Planning", "In Progress", "On Hold", "Completed", "Cancelled" };
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(projectForm.Value.Name))
                            {
                                client.Toast("Project Name is required", "Validation Error");
                                return;
                            }
                            
                            if (projectForm.Value.Budget < 0)
                            {
                                client.Toast("Budget cannot be negative", "Validation Error");
                                return;
                            }
                            
                            if (isEdit && existingProject != null)
                            {
                                existingProject.Name = projectForm.Value.Name;
                                existingProject.Description = projectForm.Value.Description;
                                existingProject.Status = projectForm.Value.Status;
                                existingProject.StartDate = projectForm.Value.StartDate;
                                existingProject.EndDate = projectForm.Value.EndDate;
                                existingProject.Budget = projectForm.Value.Budget;
                                existingProject.ProjectManager = projectForm.Value.ProjectManager;
                                existingProject.UpdatedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                var newProject = new Data.Project
                                {
                                    Name = projectForm.Value.Name,
                                    Description = projectForm.Value.Description,
                                    Status = projectForm.Value.Status,
                                    StartDate = projectForm.Value.StartDate,
                                    EndDate = projectForm.Value.EndDate,
                                    Budget = projectForm.Value.Budget,
                                    ProjectManager = projectForm.Value.ProjectManager,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                context.Projects.Add(newProject);
                            }
                            
                            context.SaveChanges();
                            client.Toast(isEdit ? "Project updated successfully!" : "Project created successfully!");
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
                        .Add(Text.Small("Project Information"))
                        .Add(Text.Small("Project Name"))
                        .Add(new TextInput(projectForm.Value.Name, e => {
                            var cloned = CloneProject(projectForm.Value);
                            cloned.Name = e.Value;
                            projectForm.Set(cloned);
                        }).Placeholder("Project Name"))
                        .Add(Text.Small("Description"))
                        .Add(new TextInput(projectForm.Value.Description, e => {
                            var cloned = CloneProject(projectForm.Value);
                            cloned.Description = e.Value;
                            projectForm.Set(cloned);
                        }).Placeholder("Project description...").Variant(TextInputs.Textarea))
                        .Add(Text.Small("Project Manager"))
                        .Add(new TextInput(projectForm.Value.ProjectManager, e => {
                            var cloned = CloneProject(projectForm.Value);
                            cloned.ProjectManager = e.Value;
                            projectForm.Set(cloned);
                        }).Placeholder("John Doe"))
                        .Add(Text.Small("Status"))
                        .Add(new SelectInput<string>(projectForm.Value.Status, e => {
                            var cloned = CloneProject(projectForm.Value);
                            cloned.Status = e.Value;
                            projectForm.Set(cloned);
                        }, statusOptions.ToOptions()))
                ).Title("Project Details"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Timeline & Budget"))
                        .Add(Text.Small("Start Date"))
                        .Add(new DateTimeInput<DateTime>(projectForm.Value.StartDate, e => {
                            var cloned = CloneProject(projectForm.Value);
                            cloned.StartDate = e.Value;
                            projectForm.Set(cloned);
                        }))
                        .Add(Text.Small("End Date (Optional)"))
                        .Add(new DateTimeInput<DateTime?>(projectForm.Value.EndDate, e => {
                            var cloned = CloneProject(projectForm.Value);
                            cloned.EndDate = e.Value;
                            projectForm.Set(cloned);
                        }))
                        .Add(Text.Small("Budget ($)"))
                        .Add(new NumberInput<decimal>(projectForm.Value.Budget, v => {
                            var cloned = CloneProject(projectForm.Value);
                            cloned.Budget = v;
                            projectForm.Set(cloned);
                        }).Placeholder("0.00"))
                ).Title("Schedule"))
        );
    }
}

public class TaskFormSheet(int projectId, int? taskId = null, Action? onClose = null) : ViewBase
{
    private Data.Task CloneTask(Data.Task source) => new Data.Task
    {
        Id = source.Id,
        ProjectId = source.ProjectId,
        Name = source.Name,
        Description = source.Description,
        Status = source.Status,
        Priority = source.Priority,
        DueDate = source.DueDate,
        Assignee = source.Assignee,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt
    };
    
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = taskId.HasValue;
        var existingTask = isEdit ? context.Tasks.FirstOrDefault(t => t.Id == taskId!.Value) : null;
        
        var taskForm = this.UseState(existingTask ?? new Data.Task
        {
            ProjectId = projectId,
            Name = "",
            Description = "",
            Status = "To Do",
            Priority = 2,
            DueDate = null,
            Assignee = ""
        });
        
        var statusOptions = new[] { "To Do", "In Progress", "Review", "Done" };
        var priorityOptions = new[] { (1, "Low"), (2, "Medium"), (3, "High"), (4, "Critical") };
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(taskForm.Value.Name))
                            {
                                client.Toast("Task Name is required", "Validation Error");
                                return;
                            }
                            
                            if (taskForm.Value.Priority < 1 || taskForm.Value.Priority > 4)
                            {
                                client.Toast("Priority must be between 1 and 4", "Validation Error");
                                return;
                            }
                            
                            if (isEdit && existingTask != null)
                            {
                                existingTask.Name = taskForm.Value.Name;
                                existingTask.Description = taskForm.Value.Description;
                                existingTask.Status = taskForm.Value.Status;
                                existingTask.Priority = taskForm.Value.Priority;
                                existingTask.DueDate = taskForm.Value.DueDate;
                                existingTask.Assignee = taskForm.Value.Assignee;
                                existingTask.UpdatedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                var newTask = new Data.Task
                                {
                                    ProjectId = projectId,
                                    Name = taskForm.Value.Name,
                                    Description = taskForm.Value.Description,
                                    Status = taskForm.Value.Status,
                                    Priority = taskForm.Value.Priority,
                                    DueDate = taskForm.Value.DueDate,
                                    Assignee = taskForm.Value.Assignee,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                context.Tasks.Add(newTask);
                            }
                            
                            context.SaveChanges();
                            client.Toast(isEdit ? "Task updated successfully!" : "Task created successfully!");
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
                        .Add(Text.Small("Task Information"))
                        .Add(Text.Small("Task Name"))
                        .Add(new TextInput(taskForm.Value.Name, e => {
                            var cloned = CloneTask(taskForm.Value);
                            cloned.Name = e.Value;
                            taskForm.Set(cloned);
                        }).Placeholder("Task Name"))
                        .Add(Text.Small("Description"))
                        .Add(new TextInput(taskForm.Value.Description, e => {
                            var cloned = CloneTask(taskForm.Value);
                            cloned.Description = e.Value;
                            taskForm.Set(cloned);
                        }).Placeholder("Task description...").Variant(TextInputs.Textarea))
                        .Add(Text.Small("Assignee"))
                        .Add(new TextInput(taskForm.Value.Assignee, e => {
                            var cloned = CloneTask(taskForm.Value);
                            cloned.Assignee = e.Value;
                            taskForm.Set(cloned);
                        }).Placeholder("John Doe"))
                        .Add(Text.Small("Status"))
                        .Add(new SelectInput<string>(taskForm.Value.Status, e => {
                            var cloned = CloneTask(taskForm.Value);
                            cloned.Status = e.Value;
                            taskForm.Set(cloned);
                        }, statusOptions.ToOptions()))
                        .Add(Text.Small("Priority"))
                        .Add(new SelectInput<int>(taskForm.Value.Priority, e => {
                            var cloned = CloneTask(taskForm.Value);
                            cloned.Priority = e.Value;
                            taskForm.Set(cloned);
                        }, priorityOptions.ToOptions()))
                        .Add(Text.Small("Due Date (Optional)"))
                        .Add(new DateTimeInput<DateTime?>(taskForm.Value.DueDate, e => {
                            var cloned = CloneTask(taskForm.Value);
                            cloned.DueDate = e.Value;
                            taskForm.Set(cloned);
                        }))
                ).Title("Task Details"))
        );
    }
}
