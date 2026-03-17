using Cosmin.Application.Abstractions;
using Cosmin.Infrastructure.Persistence;
using Cosmin.Infrastructure.Repositories;
using Cosmin.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5159", "https://localhost:7202")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Cosmin.Application.Users.Commands.RegisterUserCommand).Assembly));

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDocumentSummaryRepository, DocumentSummaryRepository>();

// Add HttpClient for Python AI Summarizer
builder.Services.AddHttpClient("AiSummarizer", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AiSummarizer:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(60);
});

// Add AI Summarizer service
builder.Services.AddScoped<IAiSummarizerService, AiSummarizerService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

