using UrlShortener.Api.Dtos;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Mappers;

public static class UrlMapper
{
    public static UrlResponse ToDto(this UrlEntity urlEntity)
    {
        return new UrlResponse
        {
            Url = urlEntity.FullUrl
        };
    }
}