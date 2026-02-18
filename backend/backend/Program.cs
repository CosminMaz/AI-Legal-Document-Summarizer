using backend.API.User;
using backend.Application.User;
using backend.Domain.User.Dto;
using backend.Domain.User.Validators;
using backend.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BackendContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddScoped<RegisterService>();

builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapRegister();

app.Run();
