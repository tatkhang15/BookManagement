using ApiAuthRequest = BookManagement.Api.Models.RegisterRequest;
using ApiBookRequest = BookManagement.Api.Models.BookRequest;
using WebLoginRequest = BookManagement.Web.Endpoints.LoginRequest;
namespace BookManagement.Tests;

public sealed class ValidationTests
{
    [Fact]
    public void ApiRegisterRequest_RejectsWeakPassword()
    {
        var request = new ApiAuthRequest
        {
            FullName = "Test User",
            UserName = "tester",
            Email = "test@example.com",
            Password = "123"
        };

        var errors = BookManagement.Api.Validation.EndpointValidationExtensions.Validate(request);

        Assert.NotNull(errors);
        Assert.Contains("Password", errors!.Keys);
    }

    [Fact]
    public void ApiBookRequest_RejectsNonPositivePrice()
    {
        var request = new ApiBookRequest
        {
            Title = "Clean Architecture",
            Author = "Robert C. Martin",
            Price = 0
        };

        var errors = BookManagement.Api.Validation.EndpointValidationExtensions.Validate(request);

        Assert.NotNull(errors);
        Assert.Contains("Price", errors!.Keys);
    }

    [Fact]
    public void WebLoginRequest_AllowsLegacyShortPasswords()
    {
        var request = new WebLoginRequest
        {
            Identifier = "legacy-user",
            Password = "123"
        };

        var errors = BookManagement.Web.Validation.EndpointValidationExtensions.Validate(request);

        Assert.Null(errors);
    }
}
