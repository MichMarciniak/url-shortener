using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using UrlShortener.Api.Data;
using UrlShortener.Api.Services;
using UrlShortener.Api.Services.Background;
using UrlShortener.Api.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddSingleton<ICodeGenerator, Base58Generator>();
builder.Services.AddScoped<IUrlService, UrlService>();
builder.Services.AddHostedService<CleanupWorker>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();