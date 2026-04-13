# URL Shortener API
Url shortening service built in .NET 10

## Key Features
- **Link Shortening:** Generates unique Base58 short codes.
- **Custom Aliases:** Users can define personalized codes.
- **Auto-Redirects:** 302 redirects to original destination.
- **Automated Cleanup:** Background service removes inactive links after 30 days.
- **Usage Tracking:** Monitors `LastAccessed` timestamps for every link.

## Tech
- **Framework:** .NET 10 (C#)
- **Database:** SQLite
- **Tests:** xUnit, Moq

## How to run
1. Clone this repository
2. Run `dotnet ef database update`
3. Start app using IDE or `dotnet run`

> API Doc is available at `/scalar`