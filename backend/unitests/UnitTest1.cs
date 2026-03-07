namespace unitests;

using backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

internal static class TestHelpers
{
    internal static DbContextOptions<BackendContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<BackendContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
    }

    internal static async Task<int> GetStatusCodeAsync(IResult result)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services,
            Response = { Body = new MemoryStream() }
        };

        await result.ExecuteAsync(httpContext);
        return httpContext.Response.StatusCode;
    }
}