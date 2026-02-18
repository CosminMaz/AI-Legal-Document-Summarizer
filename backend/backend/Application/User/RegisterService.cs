using backend.Domain.User.Dto;
using backend.Infrastructure.Persistence;
using DevOne.Security.Cryptography.BCrypt;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.User;

public class RegisterService(BackendContext context, IValidator<RegisterUserRequest> validator)
{
    public async Task<IResult> Handle(RegisterUserRequest request)
    {
        var validationError = await ValidateRequest(request);
        if (validationError != null)
        {
            return validationError;
        }

        var user = new Domain.User.Dto.User(
            Guid.NewGuid(),
               request.Username,
               request.Email,
               BCryptHelper.HashPassword(request.Password, BCryptHelper.GenerateSalt()),
               Roles.User,
               DateTime.UtcNow,
               DateTime.UtcNow
        );
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        
        return Results.Created();
    }

    private async Task<IResult?> ValidateRequest(RegisterUserRequest request)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            //TODO: Logger
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var userExists = await context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email);
        
        return userExists ?
            //TODO: Logger
            Results.Conflict("User with the same username or email already exists.") : null;
    }
}