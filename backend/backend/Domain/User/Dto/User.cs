namespace backend.Domain.User.Dto;

public record User (Guid Id, string Username, string Email, string Password, Roles Role, DateTime CreatedAt, DateTime UpdatedAt);