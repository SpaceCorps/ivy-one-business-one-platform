using IvyOneBusinessOnePlatform.Apps;
using IvyOneBusinessOnePlatform.Services;
using IvyOneBusinessOnePlatform.Data;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

var server = new Server();

// Configure database services
server.Services.AddDatabase();

// Initialize database
using (var scope = server.Services.BuildServiceProvider().CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();
    
    // Seed data
    await server.Services.BuildServiceProvider().InitializeDatabaseAsync();
}

#if DEBUG
server.UseHotReload();
#endif
server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();
var chromeSettings = new ChromeSettings().DefaultApp<MainDashboardApp>().UseTabs(preventDuplicates: true);
server.UseChrome(chromeSettings);
await server.RunAsync();
