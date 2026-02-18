namespace backend.Domain.User.Dto;

public record RegisterUserRequest(string Username, string Email, string Password);