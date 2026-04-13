using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using UrlShortener.Api.Data;
using UrlShortener.Api.Dtos;
using UrlShortener.Api.Models;
using UrlShortener.Api.Services;
using UrlShortener.Api.Utils;

namespace UrlShortener.Tests;

public class UrlServiceTests : IDisposable, IAsyncDisposable
{
    private readonly AppDbContext _context;
    private readonly SqliteConnection _connection;
    private readonly Mock<ICodeGenerator> _codeGeneratorMock;
    private readonly Mock<ILogger<UrlService>> _loggerMock;
    
    public UrlServiceTests()
    {
        _connection = new SqliteConnection("filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        
        _loggerMock = new Mock<ILogger<UrlService>>();
        _codeGeneratorMock = new Mock<ICodeGenerator>();
    }

    private UrlService CreateService()
    {
        return new UrlService(_context, _loggerMock.Object, _codeGeneratorMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task ShortenUrlAsync_WithCustomUrl_ShouldReturnCustomLink()
    {
        //arrange
        var service = CreateService();
        var request = new UrlRequest { Url = "https://google.com", Custom = "link" };
        
        //act
        var result = await service.ShortenUrlAsync(request);
        
        //assert
        Assert.NotNull(result);
        Assert.Equal("link", result.Url);
        Assert.True(await _context.Urls.AnyAsync(x => x.ShortUrl == "link"));
    }
    
    [Fact]
    public async Task ShortenUrlAsync_WithCustomUrl_ShouldReturnNullWhenExists()
    {
        //arrange
        var service = CreateService();
        var request1 = new UrlRequest { Url = "https://google.com", Custom = "link" };
        var request2 = new UrlRequest { Url = "https://test.com", Custom = "link" };
        
        //act
        await service.ShortenUrlAsync(request1);

        var result = await service.ShortenUrlAsync(request2);
        
        //assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ShortenUrlAsync_Random_ShouldReturnLink()
    {
        var service = CreateService();
        var request = new UrlRequest { Url = "https://google.com" };

        var result = await service.ShortenUrlAsync(request);

        Assert.NotNull(result);
        Assert.True(await _context.Urls.AnyAsync(x => x.FullUrl == request.Url));
    }

    [Fact]
    public async Task CleanupOldLinksAsync_ShouldDeleteOnlyOldLinks()
    {
        var service = CreateService();
        var oldLink = new UrlEntity
        {
            FullUrl = "old", ShortUrl = "old",
            LastAccessed = DateTime.UtcNow.AddDays(-40)
        };

        var newLink = new UrlEntity
        {
            FullUrl = "new", ShortUrl = "new",
            LastAccessed = DateTime.UtcNow.AddDays(-20)
        };
        
        _context.Urls.AddRange(oldLink, newLink);
        await _context.SaveChangesAsync();

        int deleted = await service.CleanupOldLinksAsync(30);
        
        Assert.Equal(1, deleted);
        Assert.False(await _context.Urls.AnyAsync(x => x.ShortUrl == "old"));
        Assert.True(await _context.Urls.AnyAsync(x => x.ShortUrl == "new"));
    }

    [Fact]
    public async Task GetFullUrlAsync_ShouldUpdateLastAccessed()
    {
        var service = CreateService();
        var link = new UrlEntity
        {
            FullUrl = "https://test.com",
            ShortUrl = "abc",
            LastAccessed = DateTime.UtcNow.AddDays(-1)
        };
        _context.Urls.Add(link);
        await _context.SaveChangesAsync();

        await service.GetFullUrlAsync("abc");

        var updatedLink = await _context.Urls.FirstAsync(x => x.ShortUrl == "abc");
        Assert.True(updatedLink.LastAccessed > DateTime.UtcNow.AddSeconds(-5));
    }

    [Fact]
    public async Task ShortenUrlAsync_Random_ShouldCheckCollision()
    {
        _codeGeneratorMock.SetupSequence(x => x.Generate())
            .Returns("taken")
            .Returns("free");
        var service = CreateService();
        var request = new UrlRequest { Url = "https://google.com" };


        _context.Urls.Add(new UrlEntity { ShortUrl = "taken", FullUrl = "doesnt matter" });
        await _context.SaveChangesAsync();

        var result = await service.ShortenUrlAsync(request);
        Assert.Equal("free", result.Url);
    }

}