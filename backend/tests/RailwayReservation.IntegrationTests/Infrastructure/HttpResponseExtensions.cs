using System.Net.Http.Json;
using System.Text.Json;

namespace RailwayReservation.IntegrationTests.Infrastructure;

public static class HttpResponseExtensions
{
    public static async Task<JsonElement> DataAsync(this HttpResponseMessage response)
    {
        var doc = await response.Content.ReadFromJsonAsync<JsonElement>();
        return doc.GetProperty("data");
    }

    public static async Task<(string Code, string Message)> ErrorAsync(this HttpResponseMessage response)
    {
        var doc = await response.Content.ReadFromJsonAsync<JsonElement>();
        var error = doc.GetProperty("error");
        return (error.GetProperty("code").GetString()!, error.GetProperty("message").GetString()!);
    }
}
