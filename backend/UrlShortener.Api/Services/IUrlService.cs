using UrlShortener.Api.Dtos;

namespace UrlShortener.Api.Services;

public interface IUrlService
{
    public Task<UrlResponse?> ShortenUrlAsync(UrlRequest request);
    public Task<UrlResponse?> GetFullUrlAsync(string shortUrl);

}