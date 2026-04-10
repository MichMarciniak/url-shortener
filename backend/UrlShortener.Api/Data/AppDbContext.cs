using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Data;

public class AppDbContext : DbContext
{

    public AppDbContext(DbContextOptions<AppDbContext>options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public DbSet<UrlEntity> Urls { get; set; }

}