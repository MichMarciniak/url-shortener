namespace UrlShortener.Api.Models;

public class UrlEntity
{
    public int Id { get; set; }
    public string FullUrl { get; set; }
    public string ShortUrl { get; set; }

    public DateTime LastAccessed { get; set; } = DateTime.UtcNow;
}