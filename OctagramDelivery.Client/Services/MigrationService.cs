using System.Net.Http.Json;

namespace OctagramDelivery.Client.Services;

public record MigrationStatus(List<string> Pending, int Applied);

public record HealthResult(bool Ok, string? Error, string ApiUrl);

public class MigrationService(HttpClient http)
{
    public string ApiUrl => http.BaseAddress?.ToString() ?? "(sin configurar)";

    public async Task<HealthResult> CheckHealthAsync()
    {
        try
        {
            var res = await http.GetAsync("api/health");
            if (res.IsSuccessStatusCode)
                return new HealthResult(true, null, ApiUrl);

            return new HealthResult(false, $"HTTP {(int)res.StatusCode} {res.ReasonPhrase}", ApiUrl);
        }
        catch (HttpRequestException ex)
        {
            return new HealthResult(false, ex.Message, ApiUrl);
        }
        catch (Exception ex)
        {
            return new HealthResult(false, ex.Message, ApiUrl);
        }
    }

    public async Task<MigrationStatus?> GetStatusAsync()
    {
        try
        {
            return await http.GetFromJsonAsync<MigrationStatus>("api/migrations/status");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> ApplyAsync()
    {
        try
        {
            var res = await http.PostAsync("api/migrations/apply", null);
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
