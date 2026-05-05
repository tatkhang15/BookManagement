using System.ComponentModel.DataAnnotations;

namespace BookManagement.Web.Validation;

public static class EndpointValidationExtensions
{
    public static Dictionary<string, string[]>? Validate<T>(this T model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var validationContext = new ValidationContext(model);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
        if (isValid)
        {
            return null;
        }

        return validationResults
            .SelectMany(result =>
            {
                var members = result.MemberNames.Any() ? result.MemberNames : new[] { string.Empty };
                return members.Select(member => new
                {
                    Member = member,
                    Error = result.ErrorMessage ?? "The request is invalid."
                });
            })
            .GroupBy(x => x.Member, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Select(x => x.Error).Distinct(StringComparer.Ordinal).ToArray(),
                StringComparer.OrdinalIgnoreCase);
    }
}
