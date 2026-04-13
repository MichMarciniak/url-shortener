using Microsoft.AspNetCore.Mvc;
using UrlShortener.Api.Dtos;
using UrlShortener.Api.Services;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UrlController : ControllerBase
{
    private readonly IUrlService _service;

    public UrlController(IUrlService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<UrlResponse>> GetFullUrl(string shortUrl)
    {
        var response = await _service.GetFullUrlAsync(shortUrl);
        if (response == null) return NotFound("Url doesn't exist");

        return Ok(response.Url);
    }

    [HttpPost]
    public async Task<ActionResult<UrlResponse>> ShortenUrl(UrlRequest request)
    {
        var response = await _service.ShortenUrlAsync(request);
        if (response == null && !string.IsNullOrEmpty(request.Custom))
        {
            return BadRequest("Url already exists.");
        }

        var fullShortUrl = Url.Action(
            action: "RedirectUrl",
            controller: "Redirect",
            values: new { url = response!.Url },
            protocol: Request.Scheme);
        
        response.Url = fullShortUrl;

        return Ok(response.Url);
    }

}