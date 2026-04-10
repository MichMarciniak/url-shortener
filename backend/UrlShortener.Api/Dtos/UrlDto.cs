namespace UrlShortener.Api.Dtos;

public record UrlResponse
{
    public string Url { get; set; }
}

public record UrlRequest
{
    public string Url { get; set; }
    public string? Custom { get; set; }
}