using System.Security.Claims;
using BookManagement.Api.Helpers;
using BookManagement.Api.Models;
using BookManagement.Api.Validation;
using BookManagement.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BookManagement.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/api/auth");

        auth.MapPost("/register", async (
            RegisterRequest request,
            UserManager<ApplicationUser> userManager,
            IMemoryCache cache,
            IEmailSender emailSender) =>
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
                    message = "Email da duoc su dung.",
                    errors = new[] { "Email da duoc su dung." }
                });
            }

            var existingUserByName = await userManager.FindByNameAsync(request.UserName);
            if (existingUserByName != null)
            {
                return Results.BadRequest(new
                {
                    message = "Ten dang nhap da duoc su dung.",
                    errors = new[] { "Ten dang nhap da duoc su dung." }
                });
            }

            // Generate 4-digit OTP
            var otp = Random.Shared.Next(1000, 10000).ToString("D4");

            // Store registration data in cache for 5 minutes
            var cacheKey = $"OTP_Register_{request.Email}";
            var cacheData = new RegisterDataCache
            {
                FullName = request.FullName,
                UserName = request.UserName,
                Email = request.Email,
                Password = request.Password,
                Otp = otp
            };
            cache.Set(cacheKey, cacheData, TimeSpan.FromMinutes(5));

            await emailSender.SendEmailAsync(request.Email, "Mã xác thực đăng ký tài khoản", $"Chào {request.FullName},<br><br>Mã xác thực OTP của bạn là: <strong>{otp}</strong><br>Mã này có hiệu lực trong 5 phút. Vui lòng không chia sẻ cho bất kỳ ai.");

            return Results.Ok(new { message = "Da gui ma OTP toi email cua ban.", requiresOtp = true });
        });

        auth.MapPost("/verify-register", async (
            VerifyRegisterRequest request,
            UserManager<ApplicationUser> userManager,
            IMemoryCache cache) =>
        {
            var validationErrors = request.Validate();
            if (validationErrors is not null) return Results.ValidationProblem(validationErrors);

            request.Email = request.Email.Trim().ToLowerInvariant();
            var cacheKey = $"OTP_Register_{request.Email}";

            if (!cache.TryGetValue(cacheKey, out RegisterDataCache? cachedData) || cachedData is null)
            {
                return Results.BadRequest(new { message = "Ma OTP khong ton tai hoac da het han." });
            }

            if (cachedData.Otp != request.Otp)
            {
                return Results.BadRequest(new { message = "Ma OTP khong chinh xac." });
            }

            // Create user
            var user = new ApplicationUser
            {
                UserName = cachedData.UserName,
                Email = cachedData.Email,
                FullName = cachedData.FullName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, cachedData.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return Results.BadRequest(new
                {
                    message = errors.FirstOrDefault() ?? "Dang ky that bai.",
                    errors
                });
            }

            await userManager.AddToRoleAsync(user, "User");
            cache.Remove(cacheKey);

            return Results.Ok(new { message = "Dang ky va xac thuc thanh cong." });
        });

        auth.MapPost("/login", async (
            LoginRequest request,
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
            if (user is null || string.Equals(user.Status, "Deleted", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Unauthorized();
            }

            var isValidPassword = await userManager.CheckPasswordAsync(user, request.Password);
            if (!isValidPassword)
            {
                return Results.Unauthorized();
            }

            var roles = await userManager.GetRolesAsync(user);
            var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
            var jwtIssuer = configuration["Jwt:Issuer"] ?? "BookManagement.Api";
            var jwtAudience = configuration["Jwt:Audience"] ?? "BookManagement.Clients";

            var tokenString = JwtTokenHelper.GenerateToken(user, roles, jwtKey, jwtIssuer, jwtAudience);
            var expires = DateTime.UtcNow.AddHours(8);

            return Results.Ok(new LoginResponse(tokenString, expires, roles.ToList()));
        });

        auth.MapPost("/forgot-password", async (
            ForgotPasswordRequest request,
            UserManager<ApplicationUser> userManager,
            IMemoryCache cache,
            IEmailSender emailSender) =>
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
                // To prevent email enumeration, we still pretend to send OTP
                var dummyOtp = Random.Shared.Next(1000, 10000).ToString("D4");
                await emailSender.SendEmailAsync(request.Email, "Mã xác thực khôi phục mật khẩu", $"Mã xác thực của bạn là: <strong>{dummyOtp}</strong><br>Mã này có hiệu lực trong 5 phút.");
                return Results.Ok(new { message = "Da gui ma OTP khoi phuc." });
            }

            var otp = Random.Shared.Next(1000, 10000).ToString("D4");
            var cacheKey = $"OTP_Forgot_{request.Email}";
            cache.Set(cacheKey, otp, TimeSpan.FromMinutes(5));

            await emailSender.SendEmailAsync(request.Email, "Mã xác thực khôi phục mật khẩu", $"Mã xác thực của bạn là: <strong>{otp}</strong><br>Mã này có hiệu lực trong 5 phút.");

            return Results.Ok(new { message = "Da gui ma OTP khoi phuc." });
        });

        auth.MapPost("/verify-forgot-otp", async (
            VerifyForgotOtpRequest request,
            UserManager<ApplicationUser> userManager,
            IMemoryCache cache) =>
        {
            var validationErrors = request.Validate();
            if (validationErrors is not null) return Results.ValidationProblem(validationErrors);

            request.Email = request.Email.Trim().ToLowerInvariant();
            var cacheKey = $"OTP_Forgot_{request.Email}";

            // If user doesn't exist, we just fail it silently or say invalid OTP
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return Results.BadRequest(new { message = "Ma OTP khong chinh xac hoac da het han." });
            }

            if (!cache.TryGetValue(cacheKey, out string? cachedOtp) || cachedOtp != request.Otp)
            {
                return Results.BadRequest(new { message = "Ma OTP khong chinh xac hoac da het han." });
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            cache.Remove(cacheKey);

            return Results.Ok(new
            {
                message = "Xac thuc OTP thanh cong.",
                email = request.Email,
                token
            });
        });

        auth.MapPost("/reset-password", async (
            ResetPasswordRequest request,
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
                return Results.BadRequest(new { message = "Email khong ton tai." });
            }

            var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return Results.BadRequest(new
                {
                    message = errors.FirstOrDefault() ?? "Dat lai mat khau that bai.",
                    errors
                });
            }

            return Results.Ok(new { message = "Dat lai mat khau thanh cong." });
        });

        auth.MapPost("/logout", () =>
        {
            return Results.Ok(new { message = "Logout thanh cong o client. Hay xoa JWT token phia ung dung." });
        }).RequireAuthorization();

        auth.MapPost("/change-password", async (
            ChangePasswordRequest request,
            ClaimsPrincipal principal,
            UserManager<ApplicationUser> userManager) =>
        {
            var validationErrors = request.Validate();
            if (validationErrors is not null)
            {
                return Results.ValidationProblem(validationErrors);
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            var result = await userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                return Results.BadRequest(new
                {
                    message = "Doi mat khau that bai.",
                    errors = result.Errors.Select(e => e.Description).ToArray()
                });
            }

            return Results.Ok(new { message = "Doi mat khau thanh cong." });
        }).RequireAuthorization("UserOrAdmin");

        return endpoints;
    }
}
