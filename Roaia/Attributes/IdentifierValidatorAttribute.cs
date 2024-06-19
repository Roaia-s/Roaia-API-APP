using System.Text.RegularExpressions;

namespace Roaia.Attributes;

public class IdentifierValidatorAttribute : ValidationAttribute
{
    // This method is used to validate the identifier and must be a valid email, phone number, or username.
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var identifier = value as string;

        if (identifier is not null)
        {
            var isEmail = new EmailAddressAttribute().IsValid(identifier);
            var isMobileNumber = Regex.IsMatch(identifier, RegexPatterns.MobileNumber);
            var isUserName = Regex.IsMatch(identifier, RegexPatterns.UserName);

            if (!(isEmail || isMobileNumber || isUserName))
            {
                return new ValidationResult(Errors.InvalidIdentifier);
            }
        }

        return ValidationResult.Success;
    }

}
