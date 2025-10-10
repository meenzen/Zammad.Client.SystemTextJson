using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Zammad.Client;

#nullable enable

public sealed class ZammadOptions : IValidatableObject
{
    /// <summary>
    /// The base URL of the Zammad instance.
    /// </summary>
    /// <example>https://zammad.example.com/</example>
    public Uri? BaseUrl { get; set; }

    /// <summary>
    /// An authentication token.
    /// </summary>
    /// <remarks>
    /// This option is mutually exclusive with the Username and Password options.
    /// </remarks>
    public string? Token { get; set; }

    /// <summary>
    /// The username for basic authentication.
    /// </summary>
    /// <remarks>
    /// This option is mutually exclusive with the Token option.
    /// </remarks>
    public string? Username { get; set; }

    /// <summary>
    /// The password for basic authentication.
    /// </summary>
    /// <remarks>
    /// This option is mutually exclusive with the Token option.
    /// </remarks>
    public string? Password { get; set; }

    /// <summary>
    /// This will be used as the X-On-Behalf-Of header on requests.
    /// </summary>
    /// <remarks>User id, user login or user email</remarks>
    public string? OnBehalfOf { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (BaseUrl is null || string.IsNullOrWhiteSpace(BaseUrl.ToString()))
        {
            yield return new ValidationResult("The BaseUrl property must be set.", [nameof(BaseUrl)]);
        }

        var hasToken = !string.IsNullOrWhiteSpace(Token);
        var hasUsername = !string.IsNullOrWhiteSpace(Username);
        var hasPassword = !string.IsNullOrWhiteSpace(Password);

        if (hasToken && (hasUsername || hasPassword))
        {
            yield return new ValidationResult(
                "The Token property cannot be used together with the Username or Password properties.",
                [nameof(Token), nameof(Username), nameof(Password)]
            );
        }

        if (!hasToken && (hasUsername != hasPassword))
        {
            yield return new ValidationResult(
                "Both the Username and Password properties must be set when not using the Token property.",
                [nameof(Username), nameof(Password)]
            );
        }
    }

    public void ThrowIfInvalid()
    {
        var results = Validate(new ValidationContext(this)).ToList();
        if (!results.Any())
        {
            return;
        }

        var builder = new StringBuilder();
        foreach (var result in results)
        {
            builder.AppendLine(result.ErrorMessage);
        }

        throw new ValidationException(builder.ToString());
    }
}
