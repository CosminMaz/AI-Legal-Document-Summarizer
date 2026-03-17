using Cosmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cosmin.Infrastructure.Persistence;

public class AppDbContext : DbContext
{   
    //Dont modify this contructor or consequences
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<SavedSummary> DocumentSummaries => Set<SavedSummary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SavedSummary>(e =>
        {
            e.ToTable("document_summaries");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasColumnName("id");
            e.Property(s => s.UserId).HasColumnName("user_id").IsRequired();
            e.Property(s => s.DocumentTitle).HasColumnName("document_title");
            e.Property(s => s.Summary).HasColumnName("summary").IsRequired();
            e.Property(s => s.Model).HasColumnName("model").IsRequired();
            e.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
            e.HasOne<User>().WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");

            e.HasKey(a => a.Id);
            e.Property(a => a.Id).HasColumnName("id");

            e.Property(a => a.Username).HasColumnName("username").IsRequired();
            e.HasIndex(a => a.Username).IsUnique();

            e.Property(a => a.Email).HasColumnName("email").IsRequired();
            e.HasIndex(a => a.Email).IsUnique();

            e.Property(a => a.Password).HasColumnName("password").IsRequired();
            e.Property(a => a.Role).HasColumnName("role").IsRequired();
            e.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
            e.Property(a => a.UpdatedAt).HasColumnName("updated_at").IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}