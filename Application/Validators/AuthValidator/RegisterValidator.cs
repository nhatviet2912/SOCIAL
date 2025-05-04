using Application.DTO.Request.Register;
using FluentValidation;

namespace Application.Validators.AuthValidator;


public class RegisterValidator : AbstractValidator<RegisterRequest>
{
    public RegisterValidator() 
    {
        RuleFor(x => x.UserName).NotEmpty().MinimumLength(5);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}