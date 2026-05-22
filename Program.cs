using BlazorApp4.Components;
using BlazorApp4.Data;
using BlazorApp4.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=store.db";

builder.Services.AddLogging(x => x.AddConsole());

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddScoped<IInventoryRepository, MySqlInventoryRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<CustomAuthProvider>();
builder.Services.AddSingleton<SearchState>();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    dbContext.Database.EnsureCreated();

    try
    {
        dbContext.Database.ExecuteSqlRaw("ALTER TABLE inventory_items ADD COLUMN MinLevel INTEGER NOT NULL DEFAULT 5");
        Console.WriteLine("MinLevel column added.");
    }
    catch { }

    try
    {
        dbContext.Database.ExecuteSqlRaw("ALTER TABLE inventory_items ADD COLUMN ImageUrl TEXT NOT NULL DEFAULT ''");
        Console.WriteLine("ImageUrl column added.");
    }
    catch { }

    try
    {
        dbContext.Database.ExecuteSqlRaw("UPDATE inventory_items SET MinLevel = 5 WHERE MinLevel = 0 OR MinLevel IS NULL");
    }
    catch { }

    try
    {
        dbContext.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS sales (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SaleDate TEXT NOT NULL,
                TotalPrice REAL NOT NULL
            )");
        Console.WriteLine("Sales table created.");
    }
    catch { }

    try
    {
        dbContext.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS sale_items (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SaleId INTEGER NOT NULL,
                ProductId INTEGER NOT NULL,
                ProductName TEXT,
                Quantity INTEGER NOT NULL,
                UnitPrice REAL NOT NULL
            )");
        Console.WriteLine("SaleItems table created.");
    }
    catch { }

    try
    {
        dbContext.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS product_logs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductName TEXT NOT NULL,
                ProductId INTEGER NOT NULL,
                Action TEXT NOT NULL,
                PreviousQty INTEGER NOT NULL,
                ChangeQty INTEGER NOT NULL,
                NewQty INTEGER NOT NULL,
                Date TEXT NOT NULL
            )");
        Console.WriteLine("ProductLogs table created.");
    }
    catch { }

    var userCount = dbContext.Users.Count();
    var itemCount = dbContext.InventoryItems.Count();
    Console.WriteLine($"Database ready. Users: {userCount}, Items: {itemCount}");
}
catch (Exception ex)
{
    Console.WriteLine($"Database init error: {ex.Message}");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

