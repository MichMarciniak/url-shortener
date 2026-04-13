namespace UrlShortener.Api.Utils;

public interface ICodeGenerator
{
    public string Generate(int minLength = 4, int maxLength = 8);
}