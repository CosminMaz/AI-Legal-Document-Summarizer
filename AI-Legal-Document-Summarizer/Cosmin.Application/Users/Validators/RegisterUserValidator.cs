using Cosmin.Application.Users.Commands;
using FluentValidation;

namespace Cosmin.Application.Users.Validators;

public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
      public RegisterUserValidator()
      {
            RuleFor(x => x.Username).MinimumLength(3).MaximumLength(100);
            RuleFor(x => x.Email).EmailAddress();
            RuleFor(x => x.Password).MinimumLength(8);
      }     
}
