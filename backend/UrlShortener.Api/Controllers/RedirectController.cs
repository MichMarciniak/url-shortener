using Microsoft.AspNetCore.Mvc;
using UrlShortener.Api.Services;

namespace UrlShortener.Api.Controllers;

[ApiController]
public class RedirectController : ControllerBase
{
    private readonly IUrlService _service;

    public RedirectController(IUrlService service)
    {
        _service = service;
    }

    [HttpGet("{url}")]
    public async Task<IActionResult> RedirectUrl(string url)
    {
        var response = await _service.GetFullUrlAsync(url);
        if (response == null) return NotFound("Url doesn't exist");

        return Redirect(response.Url);
    }
    
}