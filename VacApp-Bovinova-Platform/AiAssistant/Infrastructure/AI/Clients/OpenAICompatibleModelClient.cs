using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using VacApp_Bovinova_Platform.AIAssistant.Infrastructure.AI.Configuration;
using VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions;

namespace VacApp_Bovinova_Platform.AIAssistant.Infrastructure.AI.Clients;

public class OpenAICompatibleModelClient(
    HttpClient httpClient,
    IOptions<LocalModelSettings> options) : ILocalModelClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly LocalModelSettings settings = options.Value;

    public async Task<string> CompleteChatAsync(IEnumerable<object> messages)
    {
        var baseUrl = settings.BaseUrl.TrimEnd('/');
        var request = new
        {
            model = settings.Model,
            messages,
            temperature = settings.Temperature
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(request, JsonOptions), Encoding.UTF8, "application/json")
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);

        using var response = await httpClient.SendAsync(httpRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Surfaced as a 429 (not a generic 500) so the frontend can tell the user the
        // assistant is rate-limited and to retry, instead of "an unexpected error".
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            throw new RateLimitedException(
                "El asistente de IA está recibiendo demasiadas solicitudes en este momento. Espera unos segundos e inténtalo de nuevo.");

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Local model request failed with status {(int)response.StatusCode}: {responseBody}");

        using var json = JsonDocument.Parse(responseBody);
        var choices = json.RootElement.GetProperty("choices");
        if (choices.GetArrayLength() == 0)
            throw new InvalidOperationException("Local model response did not include choices.");

        var message = choices[0].GetProperty("message");
        return message.GetProperty("content").GetString() ?? string.Empty;
    }
}
