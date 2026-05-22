using BlazorApp4.Components;
using BlazorApp4.Data;
using BlazorApp4.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

static string Env(string key, string fallback) =>
    Environment.GetEnvironmentVariable(key) ?? fallback;

var mysqlUrl = Env("MYSQL_URL", "");
var connectionString = !string.IsNullOrEmpty(mysqlUrl)
    ? $"server={ParseUrl(mysqlUrl, "host")};port={ParseUrl(mysqlUrl, "port")};database={ParseUrl(mysqlUrl, "path").TrimStart('/')};user={ParseUrl(mysqlUrl, "user")};password={ParseUrl(mysqlUrl, "password")};"
    : builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "server=localhost;port=3306;database=store_manager_db;user=root;password=;";

static string ParseUrl(string url, string part)
{
    try
    {
        var u = new Uri(url);
        return part switch
        {
            "host" => u.Host,
            "port" => u.Port.ToString(),
            "path" => u.AbsolutePath,
            "user" => u.UserInfo.Split(':')[0],
            "password" => u.UserInfo.Split(':')[1],
            _ => ""
        };
    }
    catch { return ""; }
}

builder.Services.AddLogging(x => x.AddConsole());

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));
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
        dbContext.Database.ExecuteSqlRaw("ALTER TABLE inventory_items ADD COLUMN MinLevel INT NOT NULL DEFAULT 5");
        Console.WriteLine("MinLevel column added.");
    }
    catch { }

    try
    {
        dbContext.Database.ExecuteSqlRaw("ALTER TABLE inventory_items ADD COLUMN ImageUrl VARCHAR(500) NOT NULL DEFAULT ''");
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
                Id INT AUTO_INCREMENT PRIMARY KEY,
                SaleDate DATETIME NOT NULL,
                TotalPrice DECIMAL(18,2) NOT NULL
            )");
        Console.WriteLine("Sales table created.");
    }
    catch { }

    try
    {
        dbContext.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS sale_items (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                SaleId INT NOT NULL,
                ProductId INT NOT NULL,
                ProductName VARCHAR(150),
                Quantity INT NOT NULL,
                UnitPrice DECIMAL(18,2) NOT NULL
            )");
        Console.WriteLine("SaleItems table created.");
    }
    catch { }

    try
    {
        dbContext.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS product_logs (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                ProductName VARCHAR(150) NOT NULL,
                ProductId INT NOT NULL,
                Action VARCHAR(20) NOT NULL,
                PreviousQty INT NOT NULL,
                ChangeQty INT NOT NULL,
                NewQty INT NOT NULL,
                Date DATETIME NOT NULL
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

