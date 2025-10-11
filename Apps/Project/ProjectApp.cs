using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Project;

[App(icon: Icons.Check, title: "Project", path: new[] { "Project & Time Management" })]
public class ProjectApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var projects = context.Projects.Include(p => p.Tasks).ToList();
        var activeProjects = projects.Count(p => p.Status == "In Progress");
        var totalTasks = projects.SelectMany(p => p.Tasks).Count();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Project Management")
                .Add("Manage projects, tasks, and deliverables")
                .Add(Layout.Grid()
                    .Columns(3)
                    .Gap(12)
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Total Projects").Add(projects.Count.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Active").Add(activeProjects.ToString())))
                    .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Total Tasks").Add(totalTasks.ToString()))))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("New Project", _ => client.Toast("Create new project"))
                        .Icon(Icons.Plus)
                        .Variant(ButtonVariant.Primary)))
                .Add(BuildProjectsList(projects, client))
        );
    }

    private object BuildProjectsList(List<Data.Project> projects, IClientProvider client)
    {
        if (projects.Count == 0)
            return new Card(Layout.Vertical().Padding(16).Add("No projects found. Create your first project!"));
        
        var projectCards = projects.Select(proj => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(16)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(proj.Name)
                    .Add(proj.Description)
                    .Add(Layout.Horizontal()
                        .Gap(8)
                        .Add(new Badge(proj.Status)
                            .Variant(proj.Status == "In Progress" ? BadgeVariant.Primary :
                                   proj.Status == "Completed" ? BadgeVariant.Success :
                                   proj.Status == "On Hold" ? BadgeVariant.Warning :
                                   BadgeVariant.Secondary))
                        .Add(new Badge($"${proj.Budget:N0}").Variant(BadgeVariant.Info))
                        .Add(new Badge($"{proj.Tasks.Count} tasks").Variant(BadgeVariant.Outline))))
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add($"Start: {proj.StartDate:MMM dd, yyyy}")
                    .Add(Layout.Horizontal()
                        .Gap(4)
                        .Add(new Button("View", _ => client.Toast($"Viewing: {proj.Name}"))
                            .Small()
                            .Variant(ButtonVariant.Primary))
                        .Add(new Button("Tasks", _ => client.Toast($"Tasks for: {proj.Name}"))
                            .Small()
                            .Variant(ButtonVariant.Outline))))
        ));
        
        return Layout.Vertical().Gap(8).Add(projectCards);
    }
}
