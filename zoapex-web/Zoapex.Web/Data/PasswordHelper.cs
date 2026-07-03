using Microsoft.AspNetCore.Identity;

namespace Zoapex.Web.Data;

public static class PasswordHelper
{
    private static readonly PasswordHasher<string> Hasher = new();

    public static string Hash(string password)
        => Hasher.HashPassword("customer", password);

    public static bool Verify(string hash, string password)
        => Hasher.VerifyHashedPassword("customer", hash, password)
            != PasswordVerificationResult.Failed;
}
