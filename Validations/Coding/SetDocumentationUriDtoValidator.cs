using System;
using FluentValidation;
using Telebill.Dto.Coding;

namespace Telebill.Validations.Coding;

public class SetDocumentationUriDtoValidator : AbstractValidator<SetDocumentationUriDto>
{
    public SetDocumentationUriDtoValidator()
    {
        RuleFor(x => x.DocumentationUri)
            .MaximumLength(2000)
            .Must(BeUriOrRelativeOrEmpty)
            .WithMessage("DocumentationUri must be empty or a valid absolute http(s) URI, or a relative path.")
            .When(x => !string.IsNullOrWhiteSpace(x.DocumentationUri));
    }

    private static bool BeUriOrRelativeOrEmpty(string? uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
            return true;

        var trimmed = uri.Trim();
        if (trimmed.StartsWith('/') || trimmed.StartsWith("./"))
            return true;

        return Uri.TryCreate(trimmed, UriKind.Absolute, out var u) &&
               (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
    }
}
