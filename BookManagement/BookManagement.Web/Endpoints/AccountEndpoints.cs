using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using BookManagement.Core.Entities;
using BookManagement.Web.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BookManagement.Web.Endpoints;

public sealed class RegisterRequest
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required, MinLength(3)]
    public string Password { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public sealed class LoginRequest
{
    [Required]
    public string Identifier { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public sealed class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public sealed class ResetPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, MinLength(3)]
    public string NewPassword { get; set; } = string.Empty;
}

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var accountGroup = endpoints.MapGroup("/account");

        accountGroup.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            UserManager<ApplicationUser> userManager) =>
        {
            var validationErrors = request.Validate();
            if (validationErrors is not null)
            {
                return Results.ValidationProblem(validationErrors);
            }

            request.Email = request.Email.Trim().ToLowerInvariant();
            request.UserName = request.UserName.Trim();
            request.FullName = request.FullName.Trim();

            var existingUserByEmail = await userManager.FindByEmailAsync(request.Email);
            if (existingUserByEmail != null)
            {
                return Results.BadRequest(new
                {
                    message = "Email đã được sử dụng.",
                    errors = new[] { "Email đã được sử dụng." }
                });
            }

            var existingUserByName = await userManager.FindByNameAsync(request.UserName);
            if (existingUserByName != null)
            {
                return Results.BadRequest(new
                {
                    message = "Tên đăng nhập đã được sử dụng.",
                    errors = new[] { "Tên đăng nhập đã được sử dụng." }
                });
            }

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                FullName = request.FullName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return Results.BadRequest(new
                {
                    message = errors.FirstOrDefault() ?? "Đăng ký thất bại.",
                    errors
                });
            }

            await userManager.AddToRoleAsync(user, "User");
            return Results.Ok(new { message = "Đăng ký thành công." });
        }).DisableAntiforgery();

        accountGroup.MapPost("/login", async (
            [FromBody] LoginRequest request,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration) =>
        {
            var validationErrors = request.Validate();
            if (validationErrors is not null)
            {
                return Results.ValidationProblem(validationErrors);
            }

            request.Identifier = request.Identifier.Trim();
            var normalizedIdentifier = request.Identifier.ToUpperInvariant();

            var user = await userManager.Users
                .FirstOrDefaultAsync(x =>
                    x.NormalizedEmail == normalizedIdentifier ||
                    x.NormalizedUserName == normalizedIdentifier);
            if (user is null)
            {
                return Results.BadRequest(new { message = "Tên đăng nhập hoặc mật khẩu không đúng." });
            }

            if (string.Equals(user.Status, "Deleted", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest(new { message = "Tài khoản không còn hoạt động." });
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return Results.BadRequest(new { message = "Tên đăng nhập hoặc mật khẩu không đúng." });
            }

            var roles = await userManager.GetRolesAsync(user);
            var apiToken = GenerateApiAccessToken(user, roles, configuration);

            return Results.Ok(new
            {
                token = apiToken,
                roles,
                message = "Đăng nhập thành công"
            });
        }).DisableAntiforgery();

        accountGroup.MapPost("/forgot-password", async (
            [FromBody] ForgotPasswordRequest request,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment) =>
        {
            var validationErrors = request.Validate();
            if (validationErrors is not null)
            {
                return Results.ValidationProblem(validationErrors);
            }

            request.Email = request.Email.Trim().ToLowerInvariant();
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return Results.Ok(new { message = "Yêu cầu khôi phục mật khẩu đã được xử lý." });
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            if (!environment.IsDevelopment())
            {
                return Results.Ok(new { message = "Yêu cầu khôi phục mật khẩu đã được xử lý." });
            }

            return Results.Ok(new
            {
                message = "Tạo token reset thành công.",
                email = request.Email,
                token
            });
        }).DisableAntiforgery();

        accountGroup.MapPost("/reset-password", async (
            [FromBody] ResetPasswordRequest request,
            UserManager<ApplicationUser> userManager) =>
        {
            var validationErrors = request.Validate();
            if (validationErrors is not null)
            {
                return Results.ValidationProblem(validationErrors);
            }

            request.Email = request.Email.Trim().ToLowerInvariant();
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return Results.BadRequest(new { message = "Email không tồn tại." });
            }

            var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                return Results.BadRequest(new { message = string.Join(" ", result.Errors.Select(e => e.Description)) });
            }

            return Results.Ok(new { message = "Đặt lại mật khẩu thành công." });
        }).DisableAntiforgery();

        accountGroup.MapGet("/login-google", (SignInManager<ApplicationUser> signInManager, [FromQuery] string? returnUrl) =>
        {
            var redirectUrl = $"/account/google-response?returnUrl={returnUrl}";
            var properties = signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return Results.Challenge(properties, new[] { "Google" });
        });

        accountGroup.MapGet("/google-response", async (
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            [FromQuery] string? returnUrl) =>
        {
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return Results.Redirect("/login.html?error=Error loading external login information.");
            }

            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
            var apiToken = string.Empty;

            if (result.Succeeded)
            {
                var existingUser = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (existingUser is not null && !string.Equals(existingUser.Status, "Deleted", StringComparison.OrdinalIgnoreCase))
                {
                    var roles = await userManager.GetRolesAsync(existingUser);
                    apiToken = GenerateApiAccessToken(existingUser, roles, configuration);
                }
            }
            else if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var existingUser = await userManager.FindByEmailAsync(email);
                    if (existingUser is null)
                    {
                        existingUser = new ApplicationUser
                        {
                            UserName = email,
                            Email = email,
                            FullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                            EmailConfirmed = true
                        };

                        var createResult = await userManager.CreateAsync(existingUser);
                        if (!createResult.Succeeded)
                        {
                            return Results.Redirect("/login.html?error=Error registering external user.");
                        }

                        await userManager.AddToRoleAsync(existingUser, "User");
                    }

                    var addLoginResult = await userManager.AddLoginAsync(existingUser, info);
                    if (addLoginResult.Succeeded || addLoginResult.Errors.All(x => x.Code == "LoginAlreadyAssociated"))
                    {
                        var roles = await userManager.GetRolesAsync(existingUser);
                        apiToken = GenerateApiAccessToken(existingUser, roles, configuration);
                    }
                }
            }

            if (!string.IsNullOrEmpty(apiToken))
            {
                var dest = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;
                var separator = dest.Contains('?') ? "&" : "?";
                return Results.Redirect($"{dest}{separator}token={Uri.EscapeDataString(apiToken)}");
            }

            return Results.Redirect("/login.html?error=Error registering external user.");
        });

        return endpoints;
    }

    private static string GenerateApiAccessToken(ApplicationUser user, IEnumerable<string> roles, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing in Web configuration.");
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "BookManagement.Api";
        var jwtAudience = configuration["Jwt:Audience"] ?? "BookManagement.Clients";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FullName ?? user.UserName ?? user.Email ?? string.Empty)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}
