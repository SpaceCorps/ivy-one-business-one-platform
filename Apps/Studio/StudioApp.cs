namespace IvyOneBusinessOnePlatform.Apps.Studio;

[App(icon: Icons.Wrench, title: "Studio", path: new[] { "Development & Content" })]
public class StudioApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new StudioRootBlade(), "Studio");
    }
}

public class StudioRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var blades = this.UseContext<IBladeController>();
        
        var menuItems = new[]
        {
            new ListItem("Projects",
                subtitle: "Manage development projects",
                icon: Icons.Folder,
                badge: "3",
                onClick: _ => blades.Push(this, new ProjectsBlade(), "Projects")),
            new ListItem("Code Editor",
                subtitle: "Edit and manage code files",
                icon: Icons.Code,
                onClick: _ => blades.Push(this, new EditorBlade(), "Editor")),
            new ListItem("Templates",
                subtitle: "Browse project templates",
                icon: Icons.FileText,
                badge: "12",
                onClick: _ => blades.Push(this, new TemplatesBlade(), "Templates")),
            new ListItem("Integrations",
                subtitle: "Manage external services",
                icon: Icons.Link,
                badge: "5",
                onClick: _ => blades.Push(this, new IntegrationsBlade(), "Integrations"))
        };
        
        return new List(menuItems);
    }
}

public class ProjectsBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var blades = this.UseContext<IBladeController>();
        
        var projects = new[]
        {
            new { Name = "Main Platform", Type = "Web App", Status = "Active" },
            new { Name = "Mobile App", Type = "Mobile", Status = "In Development" },
            new { Name = "API Gateway", Type = "Backend", Status = "Active" }
        };
        
        var listItems = projects.Select(proj => new ListItem(
            title: proj.Name,
            subtitle: $"{proj.Type} - {proj.Status}",
            icon: Icons.Folder,
            badge: proj.Status,
            onClick: _ => blades.Push(this, new ProjectDetailBlade(proj.Name), proj.Name)
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
            .Add(new List(listItems));
    }
}

public class ProjectDetailBlade(string projectName) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(projectName)
                .Add("Project settings and configuration")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Open", _ => client.Toast($"Opening {projectName}"))
                        .Variant(ButtonVariant.Primary))
                    .Add(new Button("Settings", _ => client.Toast("Project settings"))
                        .Variant(ButtonVariant.Secondary)))
        );
    }
}

public class EditorBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        var files = new[] { "App.cs", "Program.cs", "Startup.cs" };
        
        var listItems = files.Select(file => new ListItem(
            title: file,
            icon: Icons.FileText,
            onClick: _ => client.Toast($"Opening {file}")
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(Text.H3("Code Editor"))
                .Add(new Button("New File", _ => client.Toast("Create file"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(new List(listItems));
    }
}

public class TemplatesBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        var templates = new[]
        {
            new { Name = "Dashboard Template", Category = "UI" },
            new { Name = "CRUD Template", Category = "Backend" },
            new { Name = "Authentication", Category = "Security" }
        };
        
        var listItems = templates.Select(template => new ListItem(
            title: template.Name,
            subtitle: template.Category,
            icon: Icons.FileText,
            badge: template.Category,
            onClick: _ => client.Toast($"Using template: {template.Name}")
        ));
        
        return new List(listItems);
    }
}

public class IntegrationsBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        var integrations = new[]
        {
            new { Name = "GitHub", Status = "Connected", Icon = Icons.Github },
            new { Name = "Docker", Status = "Not Connected", Icon = Icons.Box },
            new { Name = "Azure", Status = "Connected", Icon = Icons.Cloud }
        };
        
        var listItems = integrations.Select(integration => new ListItem(
            title: integration.Name,
            subtitle: integration.Status,
            icon: integration.Icon,
            badge: integration.Status,
            onClick: _ => client.Toast($"{(integration.Status == "Connected" ? "Disconnect" : "Connect")} {integration.Name}")
        ));
        
        return new List(listItems);
    }
}
