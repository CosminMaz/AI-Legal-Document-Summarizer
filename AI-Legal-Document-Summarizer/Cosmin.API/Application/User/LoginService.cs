using backend.Domain.User.Dto;
using backend.Infrastructure.Persistence;
using DevOne.Security.Cryptography.BCrypt;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.User;

public class LoginService(BackendContext context, IValidator<LoginUserRequest> validator)
{
    public async Task<IResult> Handle(LoginUserRequest request)
    {
        var validationError = await ValidateRequest(request);
        return validationError ?? Results.Ok();
    }

    private async Task<IResult?> ValidateRequest(LoginUserRequest request)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            //TODO: Logger
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var userExists = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (userExists is null || !BCryptHelper.CheckPassword(request.Password, userExists.Password))
        {
            //TODO: Logger
            return Results.Unauthorized();
        }
        return Results.Ok();
    }
}