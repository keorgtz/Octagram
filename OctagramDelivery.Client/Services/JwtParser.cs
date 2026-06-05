using System.Security.Claims;
using System.Text.Json;

namespace OctagramDelivery.Client.Services;

public static class JwtParser
{
    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var kvp = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes)!;

        return kvp.SelectMany(pair =>
        {
            if (pair.Value.ValueKind == JsonValueKind.Array)
                return pair.Value.EnumerateArray().Select(v => new Claim(pair.Key, v.GetString()!));
            return new[] { new Claim(pair.Key, pair.Value.ToString()) };
        });
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
