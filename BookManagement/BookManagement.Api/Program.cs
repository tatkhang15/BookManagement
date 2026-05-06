using System.Text;
using BookManagement.Api.Endpoints;
using BookManagement.Core.Entities;
using BookManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BookManagement.Api.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Enable legacy timestamp behavior for Npgsql to fix DateTime UTC issues
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// ── Database ──────────────────────────────────────────────────────────────
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Port=5432;Database=bookmanagement;Username=postgres;Password=tatkhang15";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(defaultConnection));

builder.Services.AddMemoryCache();
builder.Services.AddTransient<BookManagement.Api.Helpers.IEmailSender, BookManagement.Api.Helpers.SmtpEmailSender>();

// ── Identity ──────────────────────────────────────────────────────────────
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
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

// ── JWT Authentication ────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
var jwtIssuer = jwtSection["Issuer"] ?? "BookManagement.Api";
var jwtAudience = jwtSection["Audience"] ?? "BookManagement.Clients";
if (!builder.Environment.IsDevelopment() &&
    string.Equals(jwtKey, "CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS", StringComparison.Ordinal))
{
    throw new InvalidOperationException("Configure a non-placeholder Jwt:Key before running outside Development.");
}

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// ── Authorization ─────────────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin", "SubAdmin"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin", "SubAdmin"));
});

// ── Swagger ───────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new NullableDateTimeJsonConverter());
});
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BookManagement API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter token: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS ──────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWeb", policy =>
        policy
            .WithOrigins(
                builder.Configuration["Web:BaseUrl"] ?? "https://localhost:7188",
                "https://localhost:7188",
                "http://localhost:5139",
                "http://localhost:5254")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// ══════════════════════════════════════════════════════════════════════════
var app = builder.Build();

// Auto-apply migrations & seed roles/admin
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupMigration");
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to apply database migrations on startup.");
    }

    try
    {
        await DatabaseSeeder.SeedRolesAndAdminAsync(scope.ServiceProvider, builder.Configuration);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to seed roles/admin on startup.");
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Enable serving images from wwwroot
app.UseCors("AllowWeb");
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (BadHttpRequestException ex) when (ex.Message.Contains("JSON", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new
        {
            message = "Dữ liệu gửi lên không hợp lệ. publishDate phải là yyyy-MM-dd, yyyy-MM-ddTHH:mm:ss hoặc null."
        });
    }
});
app.UseAuthentication();
app.UseAuthorization();

// ── Map Endpoints ─────────────────────────────────────────────────────────
app.MapAuthEndpoints();
app.MapBookEndpoints();
app.MapTransactionEndpoints();
app.MapUploadEndpoints();
app.MapUserEndpoints();

app.MapGet("/", () => Results.Ok(new
{
    service = "BookManagement.Api",
    status = "ok"
}));

app.Run();

public partial class Program { }
