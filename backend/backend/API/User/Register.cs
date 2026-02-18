using backend.Application.User;
using backend.Domain.User.Dto;

namespace backend.API.User;

public static class Register
{
    public static void MapRegister(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/register",
            async (RegisterUserRequest request, RegisterService service) => await service.Handle(request));
    }
}