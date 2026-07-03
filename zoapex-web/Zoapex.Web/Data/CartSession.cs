using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Zoapex.Web.Data;

public static class CartSession
{
    private const string Key = "ZoapexCart";

    public static List<CartLineDto> GetLines(ISession session)
    {
        var json = session.GetString(Key);
        return string.IsNullOrEmpty(json)
            ? []
            : JsonSerializer.Deserialize<List<CartLineDto>>(json) ?? [];
    }

    public static void SaveLines(ISession session, List<CartLineDto> lines)
        => session.SetString(Key, JsonSerializer.Serialize(lines));

    public static void Clear(ISession session)
        => session.Remove(Key);
}
