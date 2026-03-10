using backend.API.User;
using backend.Domain.User.Dto;
using FluentValidation;

namespace backend.Domain.User.Validators;

public class LoginUserValidator : AbstractValidator<LoginUserRequest>
{
    public LoginUserValidator()
    {
        RuleFor(x => x.Username).NotEmpty()
            .NotNull()
            .MinimumLength(3)
            .WithMessage("Name must be at least 3 characters long");
        
        RuleFor(x => x.Password).NotEmpty()
            .NotNull()
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters long");
    }
}