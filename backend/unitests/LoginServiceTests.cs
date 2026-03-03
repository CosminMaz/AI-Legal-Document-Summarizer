using Microsoft.AspNetCore.Http;

namespace unitests;

using backend.Application.User;
using backend.Domain.User.Dto;
using backend.Domain.User.Validators;
using backend.Infrastructure.Persistence;
using DevOne.Security.Cryptography.BCrypt;
using Microsoft.EntityFrameworkCore;

public class LoginServiceTests
{
    [Test]
    public async Task Handle_WhenRequestInvalid_ReturnsBadRequest()
    {
        var options = TestHelpers.CreateInMemoryOptions(nameof(Handle_WhenRequestInvalid_ReturnsBadRequest));
        await using var context = new BackendContext(options);
        var validator = new LoginUserValidator();
        var service = new LoginService(context, validator);

        var request = new LoginUserRequest("", "short");
        var result = await service.Handle(request);

        var statusCode = await TestHelpers.GetStatusCodeAsync(result);
        Assert.That(statusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task Handle_WhenUserMissing_ReturnsUnauthorized()
    {
        var options = TestHelpers.CreateInMemoryOptions(nameof(Handle_WhenUserMissing_ReturnsUnauthorized));
        await using var context = new BackendContext(options);
        var validator = new LoginUserValidator();
        var service = new LoginService(context, validator);

        var request = new LoginUserRequest("missing", "Password1!");
        var result = await service.Handle(request);

        var statusCode = await TestHelpers.GetStatusCodeAsync(result);
        Assert.That(statusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
    }

    [Test]
    public async Task Handle_WhenPasswordIncorrect_ReturnsUnauthorized()
    {
        var options = TestHelpers.CreateInMemoryOptions(nameof(Handle_WhenPasswordIncorrect_ReturnsUnauthorized));
        await using var context = new BackendContext(options);

        var user = new User(
            Guid.NewGuid(),
            "existing",
            "existing@example.com",
            BCryptHelper.HashPassword("Password1!", BCryptHelper.GenerateSalt()),
            Roles.User,
            DateTime.UtcNow,
            DateTime.UtcNow);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var validator = new LoginUserValidator();
        var service = new LoginService(context, validator);

        var request = new LoginUserRequest("existing", "WrongPass1!");
        var result = await service.Handle(request);

        var statusCode = await TestHelpers.GetStatusCodeAsync(result);
        Assert.That(statusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
    }

    [Test]
    public async Task Handle_WhenCredentialsValid_ReturnsOk()
    {
        var options = TestHelpers.CreateInMemoryOptions(nameof(Handle_WhenCredentialsValid_ReturnsOk));
        await using var context = new BackendContext(options);

        var user = new User(
            Guid.NewGuid(),
            "existing",
            "existing@example.com",
            BCryptHelper.HashPassword("Password1!", BCryptHelper.GenerateSalt()),
            Roles.User,
            DateTime.UtcNow,
            DateTime.UtcNow);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var validator = new LoginUserValidator();
        var service = new LoginService(context, validator);

        var request = new LoginUserRequest("existing", "Password1!");
        var result = await service.Handle(request);

        var statusCode = await TestHelpers.GetStatusCodeAsync(result);
        Assert.That(statusCode, Is.EqualTo(StatusCodes.Status200OK));
    }
}

