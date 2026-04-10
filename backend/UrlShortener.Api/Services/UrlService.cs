using System.Text;
using Microsoft.EntityFrameworkCore;
using SimpleBase;
using UrlShortener.Api.Data;
using UrlShortener.Api.Dtos;
using UrlShortener.Api.Mappers;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Services;

public class UrlService : IUrlService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UrlService> _logger;

    public UrlService(AppDbContext context, ILogger<UrlService> logger)
    {
        _context = context;
        _logger = logger;
    }


    public async Task<UrlResponse?> ShortenUrlAsync(UrlRequest request)
    {
        if (request.Custom != null)
        {
            return await GetCustomUrl(request.Url, request.Custom);
        }

        return await GetUrlFromId(request.Url);
    }
    

    public async Task<UrlResponse?> GetFullUrlAsync(string shortUrl)
    {
        _logger.LogInformation($"Getting full url from: {shortUrl}");
        return await _context.Urls
            .Where(x => x.ShortUrl == shortUrl)
            .Select(x => x.ToDto())
            .FirstOrDefaultAsync();
    }

    private async  Task<UrlResponse> GetUrlFromId(string fullUrl)
    {
        var entity = new UrlEntity
        {
            FullUrl = fullUrl
        };
        
        _context.Add(entity);
        await _context.SaveChangesAsync();

        int safetyCounter = 0;
        string shortUrl;
        
        do
        {
            int currentId = entity.Id + safetyCounter;
            var bytes = Encoding.ASCII.GetBytes(currentId.ToString());
            shortUrl = Base58.Bitcoin.Encode(bytes);
        }
        while (await _context.Urls.AnyAsync(x => x.ShortUrl == shortUrl));

        entity.ShortUrl = shortUrl;
        await _context.SaveChangesAsync();

        return new UrlResponse
        {
            Url = shortUrl
        };
    }

    private async Task<UrlResponse?> GetCustomUrl(string fullUrl, string customUrl)
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