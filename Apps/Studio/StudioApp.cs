using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Studio;

[App(icon: Icons.Wrench, title: "Studio", path: new[] { "Development & Content" })]
public class StudioApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        var selectedView = this.UseState("projects");
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Development Studio")
                .Add("Build, customize, and manage applications")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Projects", _ => selectedView.Set("projects"))
                        .Variant(selectedView.Value == "projects" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("Code Editor", _ => selectedView.Set("editor"))
                        .Variant(selectedView.Value == "editor" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("Templates", _ => selectedView.Set("templates"))
                        .Variant(selectedView.Value == "templates" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("Integrations", _ => selectedView.Set("integrations"))
                        .Variant(selectedView.Value == "integrations" ? ButtonVariant.Primary : ButtonVariant.Secondary)))
                .Add(BuildSelectedView(selectedView.Value, client))
        );
    }

    private object BuildSelectedView(string view, IClientProvider client)
    {
        return view switch
        {
            "projects" => BuildProjectsView(client),
            "editor" => BuildEditorView(client),
            "templates" => BuildTemplatesView(client),
            "integrations" => BuildIntegrationsView(client),
            _ => "Select a view"
        };
    }

    private object BuildProjectsView(IClientProvider client)
    {
        var projects = new[]
        {
            new { Name = "Main Platform", Type = "Web App", Status = "Active", LastModified = DateTime.Now.AddHours(-2) },
            new { Name = "Mobile App", Type = "Mobile", Status = "In Development", LastModified = DateTime.Now.AddDays(-1) },
            new { Name = "API Gateway", Type = "Backend", Status = "Active", LastModified = DateTime.Now.AddHours(-5) }
        };

        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add("Development Projects")
                    .Add(new Button("New Project", _ => client.Toast("Create new project"))
                        .Icon(Icons.Plus)
                        .Variant(ButtonVariant.Primary)))
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add(projects.Select(proj => new Card(
                        Layout.Horizontal()
                            .Gap(12)
                            .Padding(16)
                            .Add(Layout.Vertical()
                                .Gap(4)
                                .Add(proj.Name)
                                .Add(proj.Type)
                                .Add(Layout.Horizontal()
                                    .Gap(8)
                                    .Add(new Badge(proj.Status)
                                        .Variant(proj.Status == "Active" ? BadgeVariant.Success : BadgeVariant.Warning))
                                    .Add(new Badge($"Modified: {proj.LastModified:MMM dd, HH:mm}")
                                        .Variant(BadgeVariant.Outline))))
                            .Add(Layout.Horizontal()
                                .Gap(4)
                                .Add(new Button("Open", _ => client.Toast($"Opening: {proj.Name}"))
                                    .Small()
                                    .Variant(ButtonVariant.Primary))
                                .Add(new Button("Settings", _ => client.Toast($"Settings: {proj.Name}"))
                                    .Small()
                                    .Variant(ButtonVariant.Outline)))
                    ))))
        );
    }

    private object BuildEditorView(IClientProvider client)
    {
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add("Code Editor")
                .Add(new Card(
                    Layout.Vertical()
                        .Gap(12)
                        .Padding(16)
                        .Add("File Explorer")
                        .Add(new Badge("src/")
                            .Icon(Icons.Folder))
                        .Add(new Badge("components/")
                            .Icon(Icons.Folder))
                        .Add(new Badge("App.cs")
                            .Icon(Icons.FileText)
                            .Variant(BadgeVariant.Outline))))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Save", _ => client.Toast("File saved!"))
                        .Icon(Icons.Save)
                        .Variant(ButtonVariant.Primary))
                    .Add(new Button("Format", _ => client.Toast("Code formatted!"))
                        .Variant(ButtonVariant.Secondary))
                    .Add(new Button("Run", _ => client.Toast("Running..."))
                        .Icon(Icons.Play)
                        .Variant(ButtonVariant.Success)))
        );
    }

    private object BuildTemplatesView(IClientProvider client)
    {
        var templates = new[]
        {
            new { Name = "Dashboard Template", Description = "Pre-built dashboard with charts", Category = "UI" },
            new { Name = "CRUD Template", Description = "Create, Read, Update, Delete operations", Category = "Backend" },
            new { Name = "Authentication", Description = "User login and registration", Category = "Security" }
        };

        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add("Project Templates")
                .Add(Layout.Grid()
                    .Columns(3)
                    .Gap(16)
                    .Add(templates.Select(template => new Card(
                        Layout.Vertical()
                            .Gap(8)
                            .Padding(16)
                            .Add(template.Name)
                            .Add(template.Description)
                            .Add(new Badge(template.Category)
                                .Variant(BadgeVariant.Secondary))
                            .Add(new Button("Use Template", _ => client.Toast($"Using: {template.Name}"))
                                .Small()
                                .Variant(ButtonVariant.Primary))
                    ))))
        );
    }

    private object BuildIntegrationsView(IClientProvider client)
    {
        var integrations = new[]
        {
            new { Name = "GitHub", Status = "Connected", Icon = Icons.Github },
            new { Name = "Docker", Status = "Not Connected", Icon = Icons.Box },
            new { Name = "Azure", Status = "Connected", Icon = Icons.Cloud }
        };

        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add("External Integrations")
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add(integrations.Select(integration => new Card(
                        Layout.Horizontal()
                            .Gap(12)
                            .Padding(16)
                            .Add(new Icon(integration.Icon))
                            .Add(Layout.Vertical()
                                .Gap(4)
                                .Add(integration.Name)
                                .Add(new Badge(integration.Status)
                                    .Variant(integration.Status == "Connected" ? BadgeVariant.Success : BadgeVariant.Secondary)))
                            .Add(new Button(integration.Status == "Connected" ? "Disconnect" : "Connect", 
                                _ => client.Toast($"{(integration.Status == "Connected" ? "Disconnecting" : "Connecting")} {integration.Name}"))
                                .Small()
                                .Variant(integration.Status == "Connected" ? ButtonVariant.Destructive : ButtonVariant.Primary))
                    ))))
        );
    }
}
