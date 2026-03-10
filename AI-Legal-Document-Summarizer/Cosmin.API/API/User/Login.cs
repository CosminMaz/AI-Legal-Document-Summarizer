using backend.Application.User;
using backend.Domain.User.Dto;
using Microsoft.AspNetCore.Mvc;

namespace backend.API.User;

public static class Login
{
    public static void MapLogin(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/login",
            async ([FromBody] LoginUserRequest request, [FromServices] LoginService service) => await service.Handle(request));
    }
}