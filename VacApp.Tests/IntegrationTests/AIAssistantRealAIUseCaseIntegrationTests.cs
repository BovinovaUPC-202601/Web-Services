using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using VacApp_Bovinova_Platform.AIAssistant.Application.ACL;
using VacApp_Bovinova_Platform.AIAssistant.Application.Internal.CommandServices;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Commands;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Entities;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Services;
using VacApp_Bovinova_Platform.AIAssistant.Infrastructure.AI.Clients;
using VacApp_Bovinova_Platform.AIAssistant.Infrastructure.AI.Services;
using VacApp_Bovinova_Platform.AIAssistant.Infrastructure.Persistence.EFC.Repositories;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Repositories;
using Xunit.Abstractions;

namespace VacApp.Tests.IntegrationTests;

public class AIAssistantRealAIUseCaseIntegrationTests(ITestOutputHelper output)
{
    [ExternalAIFact]
    [Trait("Category", "ExternalAI")]
    public async Task SendGeneralChatCommand_WithInMemoryPersistenceMockedFarmContextAndRealAI_PersistsAndRetrievesHistory()
    {
        // Arrange
        const int userId = 42;
        const string farmContext = """
                                   Authenticated ranch user id: 42
                                   Registered bovines: 2
                                   - Bovine #10: Lola, female, Angus, born 2022-01-15, stable #3
                                   - Bovine #11: Mateo, male, Holstein, born 2021-09-03, stable #3
                                   Registered stables: 1
                                   - Stable #3: North Stable, capacity 20
                                   Registered campaigns: 1
                                   - Campaign #5: Parasite Control May, Deworming campaign from 2026-05-01 to 2026-05-31
                                   """;

        using var context = CreateInMemoryContext();
        var sessionRepository = new AISessionRepository(context);
        var analysisRepository = new BovineAnalysisRepository(context);
        var unitOfWork = new UnitOfWork(context);
        var ranchContextFacadeMock = new Mock<IRanchContextFacade>();
        ranchContextFacadeMock
            .Setup(facade => facade.GetGeneralRanchContextAsync(userId))
            .ReturnsAsync(farmContext);
        var alertContextFacadeMock = new Mock<IAlertContextFacade>();

        using var chatService = CreateRealCapturingChatService();
        var service = new AIAssistantCommandService(
            sessionRepository,
            analysisRepository,
            chatService,
            Mock.Of<IAIVisionService>(),
            ranchContextFacadeMock.Object,
            alertContextFacadeMock.Object,
            unitOfWork);

        // Act
        var firstResponse = await service.Handle(new SendGeneralChatCommand(
            userId,
            "What is the active campaign name? Answer briefly."));
        var persistedAfterFirstMessage = await sessionRepository.FindGeneralChatSessionByUserIdAsync(userId);
        var persistedMessageCountAfterFirstMessage = persistedAfterFirstMessage?.GetMessages().Count ?? 0;

        var secondResponse = await service.Handle(new SendGeneralChatCommand(
            userId,
            "What was my previous question about? Answer briefly."));
        var persistedAfterSecondMessage = await sessionRepository.FindGeneralChatSessionByUserIdAsync(userId);

        output.WriteLine(firstResponse);
        output.WriteLine(secondResponse);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(firstResponse));
        Assert.False(string.IsNullOrWhiteSpace(secondResponse));
        Assert.NotNull(persistedAfterFirstMessage);
        Assert.NotNull(persistedAfterSecondMessage);
        Assert.Contains("Parasite Control May", chatService.Calls[0].SystemPrompt);
        Assert.Empty(chatService.Calls[0].ConversationHistory);
        Assert.Equal(2, persistedMessageCountAfterFirstMessage);
        Assert.Equal(2, chatService.Calls[1].ConversationHistory.Count);
        Assert.Equal(4, persistedAfterSecondMessage.GetMessages().Count);
        Assert.Contains(
            persistedAfterSecondMessage.GetMessages(),
            message => message.Role == "user" && message.Content.Contains("active campaign name"));
        ranchContextFacadeMock.Verify(facade => facade.GetGeneralRanchContextAsync(userId), Times.Exactly(2));
        alertContextFacadeMock.Verify(
            facade => facade.GetBovineAlertContextAsync(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }

    [ExternalAIFact]
    [Trait("Category", "ExternalAI")]
    public async Task SendBovineChatCommand_WithInMemoryPersistencePreviousAnalysisMockedBovineContextAndRealAI_UsesAnalysisContext()
    {
        // Arrange
        const int userId = 42;
        const int bovineId = 10;
        const string bovineContext = """
                                     Authenticated ranch user id: 42
                                     Bovine id: 10
                                     Name: Lola
                                     Gender: female
                                     Breed: Angus
                                     Birth date: 2022-01-15
                                     Stable id: 3
                                     """;

        using var context = CreateInMemoryContext();
        var sessionRepository = new AISessionRepository(context);
        var analysisRepository = new BovineAnalysisRepository(context);
        var unitOfWork = new UnitOfWork(context);
        await analysisRepository.AddAsync(new BovineAnalysis(
            userId,
            bovineId,
            82m,
            "Visible limp on rear leg",
            UrgencyLevel.Yellow,
            "Check mobility and contact a veterinarian if it persists.",
            0.91m));
        await unitOfWork.CompleteAsync();

        var ranchContextFacadeMock = new Mock<IRanchContextFacade>();
        ranchContextFacadeMock
            .Setup(facade => facade.GetBovineContextAsync(userId, bovineId))
            .ReturnsAsync(bovineContext);
        var alertContextFacadeMock = new Mock<IAlertContextFacade>();
        alertContextFacadeMock
            .Setup(facade => facade.GetBovineAlertContextAsync(userId, bovineId))
            .ReturnsAsync("""
                          Alertas recientes del bovino seleccionado (maximo 5):
                          - 2026-05-30 12:00:00Z: tipo Fever, urgencia Red, estado Unread, mensaje: High temperature detected
                          """);

        using var chatService = CreateRealCapturingChatService();
        var service = new AIAssistantCommandService(
            sessionRepository,
            analysisRepository,
            chatService,
            Mock.Of<IAIVisionService>(),
            ranchContextFacadeMock.Object,
            alertContextFacadeMock.Object,
            unitOfWork);

        // Act
        var response = await service.Handle(new SendBovineChatCommand(
            userId,
            bovineId,
            "What should I check for Lola today? Answer briefly."));
        var persistedSession = await sessionRepository.FindBovineChatSessionByUserIdAndBovineIdAsync(userId, bovineId);

        output.WriteLine(response);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(response));
        Assert.NotNull(persistedSession);
        Assert.Contains("Name: Lola", chatService.Calls[0].SystemPrompt);
        Assert.Contains("High temperature detected", chatService.Calls[0].SystemPrompt);
        Assert.Contains("Visible limp on rear leg", chatService.Calls[0].SystemPrompt);
        Assert.Contains("urgency Yellow", chatService.Calls[0].SystemPrompt);
        Assert.Empty(chatService.Calls[0].ConversationHistory);
        Assert.Equal(2, persistedSession.GetMessages().Count);
        Assert.Contains(
            persistedSession.GetMessages(),
            message => message.Role == "assistant" && !string.IsNullOrWhiteSpace(message.Content));
        ranchContextFacadeMock.Verify(facade => facade.GetBovineContextAsync(userId, bovineId), Times.Once);
        alertContextFacadeMock.Verify(facade => facade.GetBovineAlertContextAsync(userId, bovineId), Times.Once);
    }

    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"ai-assistant-{Guid.NewGuid()}")
            .Options;
        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static CapturingAIChatService CreateRealCapturingChatService()
    {
        var settings = LocalModelTestSettings.Load();
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(90)
        };
        var localModelClient = new OpenAICompatibleModelClient(httpClient, Options.Create(settings));
        return new CapturingAIChatService(new LmStudioChatService(localModelClient), httpClient);
    }

    private sealed class CapturingAIChatService(IAIChatService inner, IDisposable disposableResource) : IAIChatService, IDisposable
    {
        public List<CapturedAIChatCall> Calls { get; } = [];

        public async Task<string> GenerateResponseAsync(
            string systemPrompt,
            IEnumerable<ChatMessage> conversationHistory,
            string userMessage)
        {
            var history = conversationHistory.ToList();
            Calls.Add(new CapturedAIChatCall(systemPrompt, history, userMessage));
            return await inner.GenerateResponseAsync(systemPrompt, history, userMessage);
        }

        public void Dispose()
        {
            disposableResource.Dispose();
        }
    }

    private record CapturedAIChatCall(
        string SystemPrompt,
        IReadOnlyCollection<ChatMessage> ConversationHistory,
        string UserMessage);
}
