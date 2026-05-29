using BlazorApp4.Components;
using BlazorApp4.Data;
using BlazorApp4.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

static string Env(string key, string fallback) =>
    Environment.GetEnvironmentVariable(key) ?? fallback;

string BuildCs()
{
    var url = Env("MYSQL_URL", Env("MYSQLDATABASE_URL", Env("DATABASE_URL", Env("MYSQL_PRIVATE_URL", ""))));
    if (!string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.Absolute, out var u))
        return $"server={u.Host};port={u.Port};database={u.AbsolutePath.TrimStart('/')};user={u.UserInfo.Split(':')[0]};password={u.UserInfo.Split(':')[1]};SslMode=Required;AllowPublicKeyRetrieval=true;CharSet=utf8mb4;";

    var host = Env("MYSQLHOST", Env("MYSQL_HOST", ""));
    var port = Env("MYSQLPORT", Env("MYSQL_PORT", "3306"));
    var db = Env("MYSQLDATABASE", Env("MYSQL_DATABASE", ""));
    var user = Env("MYSQLUSER", Env("MYSQL_USER", ""));
    var pass = Env("MYSQLPASSWORD", Env("MYSQL_PASSWORD", ""));
    if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(db))
        return $"server={host};port={port};database={db};user={user};password={pass};SslMode=Required;AllowPublicKeyRetrieval=true;CharSet=utf8mb4;";

    return builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "server=localhost;port=3306;database=store_manager_db;user=root;password=;";
}

var connectionString = BuildCs();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure(maxRetryCount: 5)));
builder.Services.AddScoped<IInventoryRepository, MySqlInventoryRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<CustomAuthProvider>();
builder.Services.AddSingleton<SearchState>();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var dbContext = await factory.CreateDbContextAsync();
    await dbContext.Database.EnsureCreatedAsync();
    Console.WriteLine("Database ready.");
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
