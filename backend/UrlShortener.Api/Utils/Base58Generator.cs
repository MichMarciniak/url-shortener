using System.Security.Cryptography;
using System.Text;

namespace UrlShortener.Api.Utils;

public class Base58Generator
{
    private static readonly char[] Base58Alphabet = 
        "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz".ToCharArray();

    public static string Generate(int minLength = 4, int maxLength = 8)
    {
        int length = RandomNumberGenerator.GetInt32(minLength, maxLength + 1);

        StringBuilder result = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            int index = RandomNumberGenerator.GetInt32(Base58Alphabet.Length);
            result.Append(Base58Alphabet[index]);
        }

        return result.ToString();
    }
}