using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BookManagement.Api.Validation;
using BookManagement.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookManagement.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var users = endpoints.MapGroup("/api/users");

        users.MapGet("/", async (UserManager<ApplicationUser> userManager) =>
        {
            var data = await userManager.Users
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var response = new List<object>(data.Count);
            foreach (var u in data)
            {
                var roles = await userManager.GetRolesAsync(u);
                response.Add(new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.PhoneNumber,
                    u.FullName,
                    u.Balance,
                    u.Status,
                    Roles = roles
                });
            }

            return Results.Ok(response);
        }).RequireAuthorization("AdminOnly");

        users.MapGet("/me", async (ClaimsPrincipal principal, UserManager<ApplicationUser> userManager) =>
        {
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

            var roles = await userManager.GetRolesAsync(user);

            return Results.Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.FullName,
                user.PhoneNumber,
                user.Balance,
                user.CreatedAt,
                user.Status,
                Roles = roles
            });
        }).RequireAuthorization("UserOrAdmin");

        users.MapDelete("/me", async (ClaimsPrincipal principal, UserManager<ApplicationUser> userManager) =>
        {
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

            user.Status = "Deleted";
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return Results.BadRequest(new { message = "Khong the vo hieu hoa tai khoan." });
            }

            return Results.Ok(new { message = "Tai khoan da duoc vo hieu hoa thanh cong." });
        }).RequireAuthorization("UserOrAdmin");

        users.MapPut("/{id}", async (
            string id,
            UpdateUserProfileRequest request,
            UserManager<ApplicationUser> userManager) =>
        {
            var validationErrors = request.Validate();
            if (validationErrors is not null)
            {
                return Results.ValidationProblem(validationErrors);
            }

            var user = await userManager.FindByIdAsync(id);
            if (user is null)
            {
                return Results.NotFound(new { message = "Khong tim thay user." });
            }

            request.Email = request.Email.Trim();
            request.FullName = request.FullName.Trim();
            request.PhoneNumber = request.PhoneNumber?.Trim();

            var duplicatedEmail = await userManager.Users.AnyAsync(x => x.Id != id && x.Email == request.Email);
            if (duplicatedEmail)
            {
                return Results.Conflict(new { message = "Email da duoc su dung." });
            }

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.NormalizedEmail = userManager.NormalizeEmail(request.Email);

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Results.BadRequest(new
                {
                    message = "Cap nhat user that bai.",
                    errors = result.Errors.Select(x => x.Description)
                });
            }

            return Results.Ok(new { message = "Cap nhat user thanh cong." });
        }).RequireAuthorization("AdminOnly");

        users.MapPut("/{id}/role", async (
            string id,
            UpdateUserRoleRequest request,
            UserManager<ApplicationUser> userManager,
            ClaimsPrincipal principal) =>
        {
            var validationErrors = request.Validate();
            if (validationErrors is not null)
            {
                return Results.ValidationProblem(validationErrors);
            }

            var user = await userManager.FindByIdAsync(id);
            if (user is null)
            {
                return Results.NotFound(new { message = "Khong tim thay user." });
            }

            var normalizedRole = request.Role.Trim();
            if (!string.Equals(normalizedRole, "SubAdmin", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(normalizedRole, "User", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest(new { message = "Role khong hop le." });
            }

            var actorId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.Equals(actorId, user.Id, StringComparison.Ordinal))
            {
                return Results.BadRequest(new { message = "Khong the tu cap nhat quyen cua chinh minh." });
            }

            var currentRoles = await userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0)
            {
                var removeRes = await userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeRes.Succeeded)
                {
                    return Results.BadRequest(new { message = "Khong the cap nhat quyen." });
                }
            }

            var addRes = await userManager.AddToRoleAsync(user, normalizedRole);
            if (!addRes.Succeeded)
            {
                return Results.BadRequest(new { message = "Khong the them quyen moi." });
            }

            return Results.Ok(new { message = "Cap nhat quyen thanh cong." });
        }).RequireAuthorization("AdminOnly");

        return endpoints;
    }
}

public sealed class UpdateUserProfileRequest
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone, StringLength(30)]
    public string? PhoneNumber { get; set; }
}

public sealed class UpdateUserRoleRequest
{
    [Required, StringLength(30, MinimumLength = 4)]
    public string Role { get; set; } = "User";
}
