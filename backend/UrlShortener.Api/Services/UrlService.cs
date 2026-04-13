using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SimpleBase;
using UrlShortener.Api.Data;
using UrlShortener.Api.Dtos;
using UrlShortener.Api.Mappers;
using UrlShortener.Api.Models;
using UrlShortener.Api.Utils;

namespace UrlShortener.Api.Services;

public class UrlService : IUrlService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UrlService> _logger;
    private readonly bool _useRandom = true;

    public UrlService(AppDbContext context, ILogger<UrlService> logger)
    {
        _context = context;
        _logger = logger;
    }


    public async Task<UrlResponse?> ShortenUrlAsync(UrlRequest request)
    {
        if (!string.IsNullOrEmpty(request.Custom))
        {
            return await GenerateCustomUrl(request.Url, request.Custom);
        }
        
        return _useRandom 
            ? await GenerateRandomUrl(request.Url)
            : await GenerateUrlFromId(request.Url);
    }
    

    public async Task<UrlResponse?> GetFullUrlAsync(string shortUrl)
    {
        _logger.LogInformation($"Getting full url from: {shortUrl}");
        var entity = await _context.Urls.FirstOrDefaultAsync(x => x.ShortUrl == shortUrl);
        if (entity == null) return null;
        
        entity.LastAccessed = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return entity.ToDto();
    }

    public async Task<int> CleanupOldLinksAsync(int daysOld)
    {
        var threshold = DateTime.UtcNow.AddDays(-daysOld);

        int deletedCount = await _context.Urls
            .Where(x => x.LastAccessed < threshold)
            .ExecuteDeleteAsync();

        if (deletedCount > 0)
        {
            _logger.LogInformation($"Deleted {deletedCount} expired urls");
        }

        return deletedCount;
    }

    private async Task<UrlResponse> GenerateRandomUrl(string fullUrl)
    {
        string shortUrl;
        do
        {
            shortUrl = Base58Generator.Generate();
        } while (await _context.Urls.AnyAsync(x => x.ShortUrl == shortUrl));
        
        var entity = new UrlEntity
        {
            FullUrl = fullUrl,
            ShortUrl = shortUrl, 
        };

        _context.Add(entity);
        await _context.SaveChangesAsync();

        return new UrlResponse
        {
            Url = shortUrl
        };

    }

    private async  Task<UrlResponse> GenerateUrlFromId(string fullUrl)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var entity = new UrlEntity
            {
                FullUrl = fullUrl
            };

            _context.Add(entity);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            int safetyCounter = 0;
            string shortUrl;

            do
            {
                int currentId = entity.Id + safetyCounter;
                safetyCounter++;
                
                var bytes = Encoding.ASCII.GetBytes(currentId.ToString());
                shortUrl = Base58.Bitcoin.Encode(bytes);
            } while (await _context.Urls.AnyAsync(x => x.ShortUrl == shortUrl));

            entity.ShortUrl = shortUrl;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Saved {fullUrl} as {shortUrl}");

            return new UrlResponse
            {
                Url = shortUrl
            };

        }
        catch
        {
            await transaction.RollbackAsync();
            _logger.LogWarning("Rollback transaction.");
            throw;
        }
    }

    private async Task<UrlResponse?> GenerateCustomUrl(string fullUrl, string customUrl)
    {
        var exists = await _context.Urls.AnyAsync(x => x.ShortUrl == customUrl);
        if (exists)
        {
            return null;
        }

        var entity = new UrlEntity
        {
            FullUrl = fullUrl,
            ShortUrl = customUrl
        };

        _context.Add(entity);
        await _context.SaveChangesAsync();

        return new UrlResponse
        {
            Url = customUrl
        };

    }
}