using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Data.Config;

public class UrlEntityConfiguration : IEntityTypeConfiguration<UrlEntity>
{
    public void Configure(EntityTypeBuilder<UrlEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.ShortUrl)
            .IsUnique();

        builder.Property(x => x.FullUrl)
            .IsRequired();

        builder.Property(x => x.LastAccessed)
            .IsRequired();
        
    }
}