using backend.Domain.User.Dto;
using FluentValidation;

namespace backend.Domain.User.Validators;

public class RegisterUserValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Username).NotEmpty()
            .NotNull()
            .MinimumLength(3)
            .WithMessage("Name must be at least 3 characters long.");
        
        RuleFor(x => x.Email).NotEmpty()
            .NotNull()
            .EmailAddress()
            .WithMessage("Email must be a valid email address.");
        
        RuleFor(x => x.Password).NotEmpty()
            .NotNull()
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"\d").WithMessage("Password must contain at least one digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}
