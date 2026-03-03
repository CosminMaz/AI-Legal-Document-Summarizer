namespace unitests;

using backend.Domain.User.Dto;
using backend.Domain.User.Validators;

public class RegisterUserValidatorTests
{
    [Test]
    public void Validate_WhenRequestValid_ReturnsValid()
    {
        var validator = new RegisterUserValidator();
        var request = new RegisterUserRequest("user", "user@example.com", "Password1!");

        var result = validator.Validate(request);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_WhenEmailInvalid_ReturnsInvalid()
    {
        var validator = new RegisterUserValidator();
        var request = new RegisterUserRequest("user", "not-an-email", "Password1!");

        var result = validator.Validate(request);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == nameof(RegisterUserRequest.Email)), Is.True);
    }

    [Test]
    public void Validate_WhenPasswordMissingComplexity_ReturnsInvalid()
    {
        var validator = new RegisterUserValidator();
        var request = new RegisterUserRequest("user", "user@example.com", "password");

        var result = validator.Validate(request);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == nameof(RegisterUserRequest.Password)), Is.True);
    }
}

