using System.Text.Json;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Entities;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Services;
using VacApp_Bovinova_Platform.AIAssistant.Infrastructure.AI.Clients;

namespace VacApp_Bovinova_Platform.AIAssistant.Infrastructure.AI.Services;

public class LmStudioVisionService(ILocalModelClient localModelClient) : IAIVisionService
{
    public async Task<BovineAnalysis> AnalyzeBovinePhotoAsync(
        int userId,
        int bovineId,
        string bovineContext,
        string imageBase64)
    {
        var imageDataUrl = imageBase64.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            ? imageBase64
            : $"data:image/jpeg;base64,{imageBase64}";

        var prompt = """
                     Analyze this bovine image using the provided ranch context.
                     Return JSON only with this schema:
                     {
                       "score": number,
                       "visibleIssues": "string",
                       "urgency": "GREEN|YELLOW|RED",
                       "recommendation": "string",
                       "confidence": number
                     }
                     Do not provide a definitive veterinary diagnosis. Provide preventive observations only.

                     Context:
                     """;

        var messages = new List<object>
        {
            new
            {
                role = "system",
                content = "You are VacApp's bovine health assistant. You help detect visible warning signs, but you never provide definitive veterinary diagnoses."
            },
            new
            {
                role = "user",
                content = new object[]
                {
                    new
                    {
                        type = "text",
                        text = $"{prompt}\n{bovineContext}"
                    },
                    new
                    {
                        type = "image_url",
                        image_url = new
                        {
                            url = imageDataUrl
                        }
                    }
                }
            }
        };

        var response = await localModelClient.CompleteChatAsync(messages);
        return ToBovineAnalysis(userId, bovineId, response);
    }

    private static BovineAnalysis ToBovineAnalysis(int userId, int bovineId, string response)
    {
        using var json = JsonDocument.Parse(ExtractJsonObject(response));
        var root = json.RootElement;

        var urgencyText = root.GetProperty("urgency").GetString();
        if (!Enum.TryParse<UrgencyLevel>(urgencyText, true, out var urgencyLevel))
            throw new InvalidOperationException($"Unknown urgency level returned by local model: {urgencyText}");

        return new BovineAnalysis(
            userId,
            bovineId,
            root.GetProperty("score").GetDecimal(),
            root.GetProperty("visibleIssues").GetString() ?? string.Empty,
            urgencyLevel,
            root.GetProperty("recommendation").GetString() ?? string.Empty,
            root.GetProperty("confidence").GetDecimal());
    }

    private static string ExtractJsonObject(string response)
    {
        var start = response.IndexOf('{');
        var end = response.LastIndexOf('}');

        if (start < 0 || end < start)
            throw new InvalidOperationException("Local model vision response did not include a JSON object.");

        return response[start..(end + 1)];
    }
}
