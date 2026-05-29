using Microsoft.Extensions.Options;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.AIAssistant.Infrastructure.AI.Clients;
using VacApp_Bovinova_Platform.AIAssistant.Infrastructure.AI.Services;
using Xunit.Abstractions;

namespace VacApp.Tests.IntegrationTests;

public class AIChatServiceIntegrationTests(ITestOutputHelper output)
{
    [ExternalAIFact]
    [Trait("Category", "ExternalAI")]
    public async Task GenerateResponseAsync_WithSimplePrompt_ReturnsModelResponse()
    {
        // Arrange
        var settings = LocalModelTestSettings.Load();
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(60)
        };
        var client = new OpenAICompatibleModelClient(httpClient, Options.Create(settings));
        var service = new LmStudioChatService(client);

        // Act
        var response = await service.GenerateResponseAsync(
            "You are a test assistant. Reply briefly.",
            Array.Empty<ChatMessage>(),
            "Reply with a short acknowledgement for the Bovinova AI assistant test.");

        output.WriteLine(response);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(response));
    }

}

public sealed class ExternalAIFactAttribute : FactAttribute
{
    public ExternalAIFactAttribute()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("RUN_AI_INTEGRATION_TESTS"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            Skip = "Set RUN_AI_INTEGRATION_TESTS=true and start LM Studio to run this external AI integration test.";
        }
    }
}
