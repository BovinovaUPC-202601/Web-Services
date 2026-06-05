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
                     Analiza esta imagen del bovino usando el contexto del rancho proporcionado.
                     Responde siempre en español en los campos de texto del JSON.
                     Devuelve únicamente JSON con este esquema:
                     {
                       "score": number,
                       "visibleIssues": "string en español",
                       "urgency": "GREEN|YELLOW|RED",
                       "recommendation": "string en español",
                       "confidence": number
                     }

                     Reglas de los campos numéricos:
                     - "score": es el Body Condition Score (BCS) del bovino, un número decimal entre 1.0 y 5.0
                       (1.0 = muy delgado/emaciado, 3.0 = condición ideal, 5.0 = obeso). Usa un decimal.
                       NUNCA uses una escala 0 a 1; si el animal se ve sano, el score ronda 3.0.
                     - "confidence": tu nivel de certeza como número decimal entre 0.0 y 1.0.

                     No entregues un diagnóstico veterinario definitivo. Da solo observaciones preventivas y recomienda contactar a un veterinario cuando el riesgo sea serio.

                     Contexto:
                     """;

        var messages = new List<object>
        {
            new
            {
                role = "system",
                content = "Eres el asistente de salud bovina de VacApp. Responde en español. Ayudas a detectar señales visuales de advertencia, pero nunca entregas diagnósticos veterinarios definitivos."
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

        // The Body Condition Score lives on a 1.0–5.0 scale (see prompt). Clamp as a safety
        // net so an out-of-range value from the model never reaches the UI as e.g. 0.9/5.
        var score = Math.Clamp(root.GetProperty("score").GetDecimal(), 1m, 5m);

        var confidence = Math.Clamp(root.GetProperty("confidence").GetDecimal(), 0m, 1m);

        return new BovineAnalysis(
            userId,
            bovineId,
            score,
            root.GetProperty("visibleIssues").GetString() ?? string.Empty,
            urgencyLevel,
            root.GetProperty("recommendation").GetString() ?? string.Empty,
            confidence);
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
