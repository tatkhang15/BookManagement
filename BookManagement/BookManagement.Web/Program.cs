using BookManagement.Infrastructure.Data;
using BookManagement.Core.Entities;
using Microsoft.AspNetCore.Identity;
using BookManagement.Web.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Enable legacy timestamp behavior for Npgsql to fix DateTime UTC issues
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Database — needed for Identity (auth handled directly by Web project)
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=bookmanagement;Username=postgres;Password=tatkhang15";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(defaultConnection));

// Identity — Web handles auth directly
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
var webJwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing in Web configuration.");
if (!builder.Environment.IsDevelopment() &&
    string.Equals(webJwtKey, "CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS", StringComparison.Ordinal))
{
    throw new InvalidOperationException("Configure a non-placeholder Jwt:Key before running outside Development.");
}

if (!string.IsNullOrWhiteSpace(googleClientId) &&
    !string.IsNullOrWhiteSpace(googleClientSecret) &&
    googleClientId != "YOUR_GOOGLE_CLIENT_ID" &&
    googleClientSecret != "YOUR_GOOGLE_CLIENT_SECRET")
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
        });
}
else
{
    builder.Services.AddAuthentication();
}

builder.Services.AddAuthorization();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login.html";
    options.AccessDeniedPath = "/login.html";
});

// HTTP client for API calls (proxied through Web for books/transactions)
builder.Services.AddHttpClient("BookManagementApi", client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5104";
    client.BaseAddress = new Uri(baseUrl);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();

    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }

    return handler;
});

var app = builder.Build();

// Keep schema aligned with EF migrations across Api and Web.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        await DatabaseSeeder.SeedRolesAndAdminAsync(scope.ServiceProvider, builder.Configuration);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to initialize database on startup.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serve static files (HTML, CSS, JS from wwwroot)
app.UseDefaultFiles(); // Serves index.html by default for "/"
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Map account endpoints (login, register, forgot/reset password — returns JSON)
app.MapAccountEndpoints();

// Proxy endpoints: forward API calls from frontend to the API project
app.MapApiProxy();

app.Run();
