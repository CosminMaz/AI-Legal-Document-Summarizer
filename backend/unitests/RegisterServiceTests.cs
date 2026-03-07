using Microsoft.AspNetCore.Http;

namespace unitests;

using backend.Application.User;
using backend.Domain.User.Dto;
using backend.Domain.User.Validators;
using backend.Infrastructure.Persistence;
using DevOne.Security.Cryptography.BCrypt;
using Microsoft.EntityFrameworkCore;

public class RegisterServiceTests
{
    [Test]
    public async Task Handle_WhenRequestInvalid_ReturnsBadRequest()
    {
        var options = TestHelpers.CreateInMemoryOptions(nameof(Handle_WhenRequestInvalid_ReturnsBadRequest));
        await using var context = new BackendContext(options);
        var validator = new RegisterUserValidator();
        var service = new RegisterService(context, validator);

        var request = new RegisterUserRequest("", "invalid", "short");
        var result = await service.Handle(request);

        var statusCode = await TestHelpers.GetStatusCodeAsync(result);
        Assert.That(statusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task Handle_WhenUserExists_ReturnsConflict()
    {
        var options = TestHelpers.CreateInMemoryOptions(nameof(Handle_WhenUserExists_ReturnsConflict));
        await using var context = new BackendContext(options);

        var existing = new User(
            Guid.NewGuid(),
            "existing",
            "existing@example.com",
            BCryptHelper.HashPassword("Password1!", BCryptHelper.GenerateSalt()),
            Roles.User,
            DateTime.UtcNow,
            DateTime.UtcNow);

        context.Users.Add(existing);
        await context.SaveChangesAsync();

        var validator = new RegisterUserValidator();
        var service = new RegisterService(context, validator);
        var request = new RegisterUserRequest("existing", "existing@example.com", "Password1!");

        var result = await service.Handle(request);

        var statusCode = await TestHelpers.GetStatusCodeAsync(result);
        Assert.That(statusCode, Is.EqualTo(StatusCodes.Status409Conflict));
    }

    [Test]
    public async Task Handle_WhenRequestValid_CreatesUser()
    {
        var options = TestHelpers.CreateInMemoryOptions(nameof(Handle_WhenRequestValid_CreatesUser));
        await using var context = new BackendContext(options);
        var validator = new RegisterUserValidator();
        var service = new RegisterService(context, validator);

        var request = new RegisterUserRequest("newuser", "newuser@example.com", "Password1!");
        var result = await service.Handle(request);

        var statusCode = await TestHelpers.GetStatusCodeAsync(result);
        Assert.That(statusCode, Is.EqualTo(StatusCodes.Status201Created));
        Assert.That(await context.Users.CountAsync(), Is.EqualTo(1));

        var saved = await context.Users.SingleAsync();
        Assert.That(saved.Password, Is.Not.EqualTo(request.Password));
        Assert.That(BCryptHelper.CheckPassword(request.Password, saved.Password), Is.True);
    }
}

