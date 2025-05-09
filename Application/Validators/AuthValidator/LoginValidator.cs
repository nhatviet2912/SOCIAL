using FluentValidation;
using Microsoft.AspNetCore.Identity.Data;

namespace Application.Validators.AuthValidator;

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}