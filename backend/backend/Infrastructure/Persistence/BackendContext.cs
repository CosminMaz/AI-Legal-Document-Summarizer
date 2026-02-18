using backend.Domain.User.Dto;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Persistence;

public class BackendContext (DbContextOptions<BackendContext> options) : DbContext(options) {
    public DbSet<User> Users { get; set; }
}