using System.Net.Http.Json;

namespace OctagramDelivery.Client.Services;

public record MigrationStatus(List<string> Pending, int Applied);

public class MigrationService(HttpClient http)
{
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
