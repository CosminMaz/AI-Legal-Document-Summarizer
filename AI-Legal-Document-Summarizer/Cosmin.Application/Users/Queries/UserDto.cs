using Cosmin.Domain.Enums;

namespace Cosmin.Application.Users.Queries;

public record UserDto(Guid Id, string Username, string Email, string Password, UserRole Role);