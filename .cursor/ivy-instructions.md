# Ivy Framework Development Instructions

## Overview

Ivy is a C# web framework for building internal tools and back-office applications. It unifies front-end and back-end into a single C# codebase with server-side state management and WebSocket communication. The frontend uses a pre-built React rendering engine.

**Framework Repository:** <https://github.com/Ivy-Interactive/Ivy-Framework>  
**Core Logic:** Located in `Ivy/` folder  
**Documentation:** Located in `Ivy.Docs.Shared/` folder

## Core Principles

### 1. React-like Component Model

- **Views** are like React Components
- **Build()** method is like React's `render()`
- **UseState()** is like React's `useState()`
- State is managed **on the server**, not the client
- Updates are sent to the frontend via **WebSocket**

### 2. Security-First Architecture

- State maintained on server prevents secret leakage
- Authentication integrations are enterprise-grade
- Never expose sensitive data to client side
- Use proper authentication providers (Supabase, Auth0, Microsoft Entra, etc.)

### 3. No HTML/CSS/JavaScript Required

- Pure C# for everything (unless adding custom widgets)
- Declarative UI using Ivy widgets
- Use pipe operator `|` for composing layouts

---

## Project Structure

```
YourProject/
├── Apps/              # Application views (marked with [App] attribute)
├── Connections/       # Database connections and data providers
├── Assets/            # Embedded resources
├── Program.cs         # Server configuration and startup
└── GlobalUsings.cs    # Global using directives
```

---

## Core Patterns

### 1. Creating Apps

Apps are the main entry points for your application. They must:

- Inherit from `ViewBase`
- Be marked with `[App]` attribute
- Override the `Build()` method
- Return UI widgets/layouts

```csharp
namespace YourProject.Apps;

[App(icon: Icons.Dashboard, title: "My App")]
public class MyApp : ViewBase
{
    public override object? Build()
    {
        // Return your UI here
        return Layout.Center()
               | new Text.H1("Hello Ivy!");
    }
}
```

**App Attribute Properties:**

- `icon` - Icon from Icons enum
- `title` - Display name in navigation
- `description` (optional) - App description
- `category` (optional) - Grouping category

### 2. State Management

Use `UseState<T>()` for reactive state. State is stored on the server.

```csharp
public override object? Build()
{
    var nameState = this.UseState<string>();
    var countState = this.UseState<int>();
    
    // Access with .Value
    var currentName = nameState.Value;
    
    // Bind to inputs
    return Layout.Vertical()
           | nameState.ToInput(placeholder: "Enter name")
           | new Text.Block($"Hello {currentName}!");
}
```

**State Lifecycle:**

- State persists during the session
- Updates trigger re-render
- State is scoped to the current view instance

### 3. Layout Composition

Use the **pipe operator** (`|`) to compose layouts. Layouts contain widgets.

```csharp
// Vertical stacking
return Layout.Vertical().Gap(4).Padding(2)
       | new Text.H1("Title")
       | new Text.Block("Description")
       | new Button("Click Me");

// Horizontal layout
return Layout.Horizontal().Gap(2)
       | new Button("Left")
       | new Button("Right");

// Centered layout
return Layout.Center()
       | new Card(/* content */);

// Grid layout
return Layout.Grid()
       | new GridItem(/* content */)
       | new GridItem(/* content */);
```

**Layout Methods:**

- `.Gap(n)` - Spacing between items
- `.Padding(n)` - Internal padding
- `.Margin(n)` - External margin
- `.Align(...)` - Alignment
- `.Justify(...)` - Justification

### 4. Common Widgets

#### Text Widgets

```csharp
Text.H1("Large Heading")
Text.H2("Medium Heading")
Text.H3("Small Heading")
Text.Block("Paragraph text")
Text.Markdown("**Bold** and *italic*")
Text.Label("Field Label")
```

#### Input Widgets

```csharp
// State-bound inputs
nameState.ToInput(placeholder: "Enter name")
emailState.ToInput(placeholder: "Email", type: InputType.Email)
passwordState.ToInput(type: InputType.Password)

// Checkbox
var agreedState = this.UseState<bool>();
agreedState.ToCheckbox(label: "I agree")

// Select/Dropdown
var selectedState = this.UseState<string>();
selectedState.ToSelect(options: new[] { "Option 1", "Option 2" })
```

#### Container Widgets

```csharp
new Card(/* content */)
    .Width(Size.Units(120))
    .Padding(4);

new Panel(/* content */)
    .Title("Panel Title");

new Modal(/* content */)
    .Title("Modal Title")
    .Width(Size.Percent(80));
```

#### Action Widgets

```csharp
new Button("Click Me")
    .OnClick(() => {
        // Handle click
        countState.Value++;
    })
    .Primary();

new Link("Go to", url: "https://example.com");

new Icon(Icons.Check).Size(24);
```

#### Data Display

```csharp
new Table<MyModel>(dataList)
    .Column("Name", x => x.Name)
    .Column("Email", x => x.Email)
    .Sortable()
    .Filterable();

new DataGrid<MyModel>(dataList);
```

#### Visual Elements

```csharp
new Separator()  // Horizontal line

new Spacer()     // Empty space

new Avatar(imageUrl)

new Badge("New").Color(BadgeColor.Success)

new Loader()     // Loading spinner

new Progress(value: 75, max: 100)
```

### 5. Sizing

Use `Size` utilities for responsive sizing:

```csharp
.Width(Size.Units(120))           // Fixed width
.Width(Size.Percent(50))          // Percentage
.Width(Size.Units(120).Max(500))  // Min/Max constraints
.Height(Size.Auto())              // Auto height
```

### 6. Event Handling

```csharp
new Button("Save")
    .OnClick(() => {
        // Handle button click
        SaveData();
    });

new Input()
    .OnChange(value => {
        // Handle input change
        ProcessValue(value);
    });
```

### 7. Conditional Rendering

Use C# conditionals and null-coalescing:

```csharp
public override object? Build()
{
    var showDetails = this.UseState<bool>();
    
    return Layout.Vertical()
           | new Text.H1("Title")
           | (showDetails.Value 
               ? new Card(new Text.Block("Details shown"))
               : null);
}
```

### 8. Lists and Iteration

```csharp
var items = new[] { "Item 1", "Item 2", "Item 3" };

return Layout.Vertical()
       | items.Select(item => new Text.Block(item));
```

---

## Server Configuration

### Program.cs Setup

```csharp
using YourProject.Apps;

// Set culture
CultureInfo.DefaultThreadCurrentCulture = 
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

var server = new Server();

#if DEBUG
server.UseHotReload();  // Enable hot reload in development
#endif

// Register apps and connections
server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();

// Configure Chrome (navigation UI)
var chromeSettings = new ChromeSettings()
    .DefaultApp<HelloApp>()
    .UseTabs(preventDuplicates: true);

server.UseChrome(chromeSettings);

await server.RunAsync();
```

---

## Global Usings

Always include these namespaces in `GlobalUsings.cs`:

```csharp
global using Ivy;
global using Ivy.Apps;
global using Ivy.Auth;
global using Ivy.Chrome;
global using Ivy.Client;
global using Ivy.Core;
global using Ivy.Core.Hooks;
global using Ivy.Helpers;
global using Ivy.Hooks;
global using Ivy.Shared;
global using Ivy.Views;
global using Ivy.Views.Alerts;
global using Ivy.Views.Blades;
global using Ivy.Views.Builders;
global using Ivy.Views.Charts;
global using Ivy.Views.Dashboards;
global using Ivy.Views.Forms;
global using Ivy.Views.Tables;
global using Ivy.Widgets.Inputs;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using System.Collections.Immutable;
global using System.ComponentModel.DataAnnotations;
global using System.Globalization;
global using System.Reactive.Linq;
```

---

## Project File Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <NoWarn>CS8618;CS8603;CS8602;CS8604;CS9113</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <EmbeddedResource Include="Assets/**/*" />
  </ItemGroup>
  
  <ItemGroup>
    <PackageReference Include="Ivy" Version="1.*" />
  </ItemGroup>
  
  <ItemGroup>
    <Folder Include="Apps" />
    <Folder Include="Connections" />
  </ItemGroup>
</Project>
```

---

## Best Practices

### 1. Component Organization

- One App per file
- Group related Apps in folders
- Use descriptive names
- Add meaningful icons and titles

### 2. State Management

- Keep state minimal
- Use appropriate types
- Avoid state duplication
- Clear state when no longer needed

### 3. Layout Design

- Use semantic layouts (Vertical, Horizontal, Grid)
- Apply consistent spacing with `.Gap()` and `.Padding()`
- Use `Card` and `Panel` for grouping
- Center important content with `Layout.Center()`

### 4. User Experience

- Provide loading states
- Show error messages clearly
- Use appropriate input types
- Add placeholder text
- Validate input before submission

### 5. Performance

- Avoid expensive operations in `Build()`
- Use hooks for side effects
- Minimize state updates
- Lazy load data when possible

### 6. Code Style

- Use fluent API chains
- Leverage pipe operator for readability
- Keep `Build()` methods focused
- Extract complex logic to methods

---

## Common Patterns

### Form Pattern

```csharp
[App(icon: Icons.Form, title: "User Form")]
public class UserFormApp : ViewBase
{
    public override object? Build()
    {
        var nameState = this.UseState<string>();
        var emailState = this.UseState<string>();
        var submittedState = this.UseState<bool>();
        
        return Layout.Center()
               | new Card(
                   Layout.Vertical().Gap(4).Padding(4)
                   | Text.H2("User Registration")
                   | Text.Label("Name")
                   | nameState.ToInput(placeholder: "Enter your name")
                   | Text.Label("Email")
                   | emailState.ToInput(placeholder: "Enter your email", type: InputType.Email)
                   | new Button("Submit")
                       .OnClick(() => {
                           // Validation
                           if (string.IsNullOrEmpty(nameState.Value) || 
                               string.IsNullOrEmpty(emailState.Value))
                           {
                               // Show error
                               return;
                           }
                           
                           // Submit logic
                           SaveUser(nameState.Value, emailState.Value);
                           submittedState.Value = true;
                       })
                       .Primary()
                   | (submittedState.Value 
                       ? Text.Block("✓ Submitted successfully!")
                       : null)
                 )
                 .Width(Size.Units(120).Max(500));
    }
    
    private void SaveUser(string name, string email)
    {
        // Save to database
    }
}
```

### Dashboard Pattern

```csharp
[App(icon: Icons.Dashboard, title: "Dashboard")]
public class DashboardApp : ViewBase
{
    public override object? Build()
    {
        return Layout.Vertical().Gap(4).Padding(4)
               | Text.H1("Dashboard")
               | Layout.Grid().Columns(3).Gap(4)
               | new Card(
                   Layout.Vertical().Padding(3)
                   | Text.H3("Total Users")
                   | Text.Block("1,234").Size(TextSize.Large)
                 )
               | new Card(
                   Layout.Vertical().Padding(3)
                   | Text.H3("Revenue")
                   | Text.Block("$56,789").Size(TextSize.Large)
                 )
               | new Card(
                   Layout.Vertical().Padding(3)
                   | Text.H3("Active Sessions")
                   | Text.Block("42").Size(TextSize.Large)
                 );
    }
}
```

### List/Detail Pattern

```csharp
[App(icon: Icons.List, title: "Items")]
public class ItemsApp : ViewBase
{
    public override object? Build()
    {
        var selectedIdState = this.UseState<int?>();
        var items = GetItems();
        
        return Layout.Horizontal().Gap(4)
               | Layout.Vertical().Width(Size.Percent(40))
               | Text.H2("Items")
               | items.Select(item => 
                   new Button(item.Name)
                       .OnClick(() => selectedIdState.Value = item.Id)
                 )
               | Layout.Vertical().Width(Size.Percent(60))
               | (selectedIdState.Value.HasValue
                   ? ShowItemDetail(selectedIdState.Value.Value)
                   : Text.Block("Select an item"));
    }
    
    private object ShowItemDetail(int id)
    {
        var item = GetItemById(id);
        return new Card(
            Layout.Vertical().Padding(4)
            | Text.H2(item.Name)
            | Text.Block(item.Description)
        );
    }
}
```

---

## Database Integration

### Creating a Connection

```csharp
namespace YourProject.Connections;

[Connection]
public class DatabaseConnection : PostgresConnection
{
    public override string ConnectionString => 
        Configuration["Database:ConnectionString"];
}
```

### Using Connections in Apps

```csharp
public class DataApp : ViewBase
{
    private readonly DatabaseConnection _db;
    
    public DataApp(DatabaseConnection db)
    {
        _db = db;
    }
    
    public override object? Build()
    {
        var data = _db.Query<MyModel>("SELECT * FROM my_table");
        
        return new Table<MyModel>(data)
            .Column("Name", x => x.Name)
            .Column("Email", x => x.Email);
    }
}
```

---

## Hooks and Lifecycle

### UseState

Manages reactive state on the server.

```csharp
var state = this.UseState<string>();
state.Value = "new value";  // Triggers re-render
```

### UseEffect

Runs side effects when dependencies change.

```csharp
this.UseEffect(() => {
    // Side effect code
    LoadData();
}, dependencies: new[] { someState.Value });
```

### UseMemo

Memoizes expensive computations.

```csharp
var expensiveResult = this.UseMemo(() => {
    return ExpensiveOperation();
}, dependencies: new[] { inputState.Value });
```

---

## Authentication

### Configuring Auth (Program.cs)

```csharp
// Supabase
server.UseSupabaseAuth(options => {
    options.Url = Configuration["Supabase:Url"];
    options.Key = Configuration["Supabase:Key"];
});

// Microsoft Entra
server.UseMicrosoftEntraAuth(options => {
    options.TenantId = Configuration["Entra:TenantId"];
    options.ClientId = Configuration["Entra:ClientId"];
});

// Auth0
server.UseAuth0(options => {
    options.Domain = Configuration["Auth0:Domain"];
    options.ClientId = Configuration["Auth0:ClientId"];
});
```

### Protected Apps

```csharp
[App(icon: Icons.Lock, title: "Admin Panel")]
[Authorize(Roles = "Admin")]
public class AdminApp : ViewBase
{
    public override object? Build()
    {
        var user = this.User;  // Access current user
        
        return Layout.Center()
               | Text.H1($"Welcome {user.Name}");
    }
}
```

---

## CLI Commands

```bash
# Initialize new project
ivy init --hello

# Run development server
dotnet watch

# View samples
ivy samples

# View documentation
ivy docs

# Add data provider
ivy add database --postgres

# Generate app from database
ivy generate app --from-database

# Deploy
ivy deploy
```

---

## Debugging and Hot Reload

### Enable Hot Reload

```csharp
#if DEBUG
server.UseHotReload();
#endif
```

### Logging

```csharp
public class MyApp : ViewBase
{
    private readonly ILogger<MyApp> _logger;
    
    public MyApp(ILogger<MyApp> logger)
    {
        _logger = logger;
    }
    
    public override object? Build()
    {
        _logger.LogInformation("Building MyApp");
        return new Text.H1("Hello");
    }
}
```

---

## Anti-Patterns to Avoid

❌ **Don't create client-side state**

```csharp
// Wrong - trying to use JavaScript
var state = "useState()"; // This is a string, not React!
```

✅ **Use server-side UseState**

```csharp
var state = this.UseState<string>();
```

❌ **Don't return null from Build()**

```csharp
public override object? Build()
{
    return null; // Nothing renders
}
```

✅ **Return a valid widget**

```csharp
public override object? Build()
{
    return new Text.Block("Content");
}
```

❌ **Don't mutate state directly**

```csharp
nameState.Value += "text";  // May not trigger re-render reliably
```

✅ **Assign new values**

```csharp
nameState.Value = nameState.Value + "text";
```

❌ **Don't nest too deeply**

```csharp
Layout.Vertical() | Layout.Vertical() | Layout.Vertical() | Layout.Vertical()
```

✅ **Keep layouts flat and semantic**

```csharp
Layout.Vertical().Gap(4)
```

---

## Resources

- **Framework:** <https://github.com/Ivy-Interactive/Ivy-Framework>
- **Documentation:** Run `ivy docs` or check `Ivy.Docs.Shared/` folder
- **Samples:** Run `ivy samples` or check `Ivy.Samples/` project
- **Core Logic:** See `Ivy/` folder in framework repository

---

## When to Use Ivy

✅ **Perfect for:**

- Internal tools and dashboards
- Back-office applications
- Admin panels
- CRUD applications
- Database-driven apps
- Enterprise internal systems

❌ **Not ideal for:**

- Public-facing websites
- Complex SPA with heavy client-side logic
- Applications requiring offline-first capabilities
- Real-time collaborative editing (unless using proper patterns)

---

## Summary Checklist

When creating an Ivy app:

- [ ] Inherit from `ViewBase`
- [ ] Add `[App]` attribute with icon and title
- [ ] Override `Build()` method
- [ ] Use `this.UseState<T>()` for state
- [ ] Compose UI with pipe operator `|`
- [ ] Use Layout containers (Vertical, Horizontal, Center, Grid)
- [ ] Apply styling with fluent methods (.Gap(), .Padding(), .Width())
- [ ] Handle events with `.OnClick()`, `.OnChange()`, etc.
- [ ] Return valid widgets from Build()
- [ ] Test with hot reload enabled

Happy Ivy coding! 🌿
