using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Project;

[App(icon: Icons.Check, title: "Project", path: new[] { "Project & Time Management" })]
public class ProjectApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new ProjectRootBlade(), "Projects");
    }
}

public class ProjectRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var projects = context.Projects.Include(p => p.Tasks).ToList();
        
        var listItems = projects.Select(proj => new ListItem(
            title: proj.Name,
            subtitle: $"{proj.Status} - {proj.Tasks.Count} tasks - ${proj.Budget:N0}",
            icon: Icons.Folder,
            badge: proj.Status,
            onClick: _ => { blades.Push(this, new ProjectDetailBlade(proj.Id), proj.Name); }
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(Text.H3("Projects"))
                .Add(new Button("New Project", _ => client.Toast("Create project"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(projects.Count == 0 
                ? Text.Block("No projects found.")
                : new List(listItems));
    }
}

public class ProjectDetailBlade(int projectId) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var project = context.Projects.Include(p => p.Tasks).FirstOrDefault(p => p.Id == projectId);
        
        if (project == null)
            return "Project not found";
        
        var taskItems = project.Tasks.Select(task => new ListItem(
            title: task.Name,
            subtitle: $"{task.Status} - Priority: {task.Priority}",
            icon: Icons.Check,
            badge: task.Status
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Add(new Card(
                Layout.Vertical()
                    .Gap(12)
                    .Padding(16)
                    .Add(project.Name)
                    .Add(project.Description)
                    .Add($"Manager: {project.ProjectManager}")
                    .Add($"Budget: ${project.Budget:N0}")
                    .Add($"Start: {project.StartDate:MMM dd, yyyy}")
                    .Add(new Badge(project.Status)
                        .Variant(project.Status == "Completed" ? BadgeVariant.Success :
                               project.Status == "In Progress" ? BadgeVariant.Primary :
                               BadgeVariant.Secondary))))
            .Add(project.Tasks.Any() 
                ? new List(taskItems)
                : "No tasks yet.");
    }
}
