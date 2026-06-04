using Moq;
using VacApp_Bovinova_Platform.AIAssistant.Application.ACL;
using VacApp_Bovinova_Platform.AIAssistant.Application.Internal.CommandServices;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Commands;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Entities;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Repositories;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Services;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp.Tests.IntegrationTests;

public class AIAssistantCommandServiceIntegrationTests
{
    private readonly Mock<IAISessionRepository> _sessionRepositoryMock = new();
    private readonly Mock<IBovineAnalysisRepository> _analysisRepositoryMock = new();
    private readonly Mock<IAIChatService> _chatServiceMock = new();
    private readonly Mock<IAIVisionService> _visionServiceMock = new();
    private readonly Mock<IRanchContextFacade> _ranchContextFacadeMock = new();
    private readonly Mock<IAlertContextFacade> _alertContextFacadeMock = new();
    private readonly Mock<IIoTContextFacade> _ioTContextFacadeMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task HandleSendGeneralChatCommand_UsesHistoryAndRanchContextAndPersistsResponse()
    {
        // Arrange
        const int userId = 7;
        var session = new GeneralChatSession(userId);
        session.AddMessage(new ChatMessage("user", "How many bovines do I have?", DateTime.UtcNow));
        session.AddMessage(new ChatMessage("assistant", "You have two bovines.", DateTime.UtcNow));

        _sessionRepositoryMock
            .Setup(repository => repository.FindGeneralChatSessionByUserIdAsync(userId))
            .ReturnsAsync(session);
        _ranchContextFacadeMock
            .Setup(facade => facade.GetGeneralRanchContextAsync(userId))
            .ReturnsAsync("Registered bovines: 2\nRegistered campaigns: 1");

        string? capturedPrompt = null;
        string? capturedUserMessage = null;
        List<ChatMessage> capturedHistory = [];
        _chatServiceMock
            .Setup(service => service.GenerateResponseAsync(
                It.IsAny<string>(),
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<string>()))
            .Callback<string, IEnumerable<ChatMessage>, string>((prompt, history, userMessage) =>
            {
                capturedPrompt = prompt;
                capturedHistory = history.ToList();
                capturedUserMessage = userMessage;
            })
            .ReturnsAsync("Farm context response.");

        var service = CreateService();
        var command = new SendGeneralChatCommand(userId, "Summarize my ranch status.");

        // Act
        var response = await service.Handle(command);

        // Assert
        Assert.Equal("Farm context response.", response);
        Assert.Contains("Registered bovines: 2", capturedPrompt);
        Assert.Contains("Registered campaigns: 1", capturedPrompt);
        Assert.Equal("Summarize my ranch status.", capturedUserMessage);
        Assert.Equal(2, capturedHistory.Count);
        Assert.Contains(capturedHistory, message => message.Content == "How many bovines do I have?");
        Assert.Contains(session.GetMessages(), message => message.Content == "Summarize my ranch status.");
        Assert.Contains(session.GetMessages(), message => message.Content == "Farm context response.");

        _sessionRepositoryMock.Verify(repository => repository.UpdateGeneralChatSession(session), Times.Once);
        _sessionRepositoryMock.Verify(repository => repository.AddGeneralChatSessionAsync(It.IsAny<GeneralChatSession>()), Times.Never);
        _alertContextFacadeMock.Verify(
            facade => facade.GetBovineAlertContextAsync(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleSendBovineChatCommand_UsesBovineContextAnalysisHistoryAndPersistsResponse()
    {
        // Arrange
        const int userId = 7;
        const int bovineId = 22;
        var session = new BovineChatSession(userId, bovineId);
        session.AddMessage(new ChatMessage("user", "How was Lola last time?", DateTime.UtcNow));
        session.AddMessage(new ChatMessage("assistant", "She had a yellow visual warning.", DateTime.UtcNow));
        var previousAnalysis = new BovineAnalysis(
            userId,
            bovineId,
            82m,
            "Visible limp on rear leg",
            UrgencyLevel.Yellow,
            "Check mobility and contact a veterinarian if it persists.",
            0.91m);

        _sessionRepositoryMock
            .Setup(repository => repository.FindBovineChatSessionByUserIdAndBovineIdAsync(userId, bovineId))
            .ReturnsAsync(session);
        _ranchContextFacadeMock
            .Setup(facade => facade.GetBovineContextAsync(userId, bovineId))
            .ReturnsAsync("Bovine id: 22\nName: Lola\nBreed: Angus");
        _alertContextFacadeMock
            .Setup(facade => facade.GetBovineAlertContextAsync(userId, bovineId))
            .ReturnsAsync(
                "Alertas recientes del bovino seleccionado (maximo 5):\n" +
                "- 2026-05-30 12:00:00Z: tipo Fever, urgencia Red, estado Unread, mensaje: High temperature detected");
        _analysisRepositoryMock
            .Setup(repository => repository.FindByBovineIdAsync(bovineId))
            .ReturnsAsync(new[] { previousAnalysis });

        string? capturedPrompt = null;
        string? capturedUserMessage = null;
        List<ChatMessage> capturedHistory = [];
        _chatServiceMock
            .Setup(service => service.GenerateResponseAsync(
                It.IsAny<string>(),
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<string>()))
            .Callback<string, IEnumerable<ChatMessage>, string>((prompt, history, userMessage) =>
            {
                capturedPrompt = prompt;
                capturedHistory = history.ToList();
                capturedUserMessage = userMessage;
            })
            .ReturnsAsync("Bovine-specific response.");

        var service = CreateService();
        var command = new SendBovineChatCommand(userId, bovineId, "What should I check for Lola?");

        // Act
        var response = await service.Handle(command);

        // Assert
        Assert.Equal("Bovine-specific response.", response);
        Assert.Contains("Name: Lola", capturedPrompt);
        Assert.Contains("Alert context", capturedPrompt);
        Assert.Contains("High temperature detected", capturedPrompt);
        Assert.Contains("Previous visual analysis context", capturedPrompt);
        Assert.Contains("urgency Yellow", capturedPrompt);
        Assert.Contains("Visible limp on rear leg", capturedPrompt);
        Assert.Equal("What should I check for Lola?", capturedUserMessage);
        Assert.Equal(2, capturedHistory.Count);
        Assert.Contains(session.GetMessages(), message => message.Content == "What should I check for Lola?");
        Assert.Contains(session.GetMessages(), message => message.Content == "Bovine-specific response.");

        _sessionRepositoryMock.Verify(repository => repository.UpdateBovineChatSession(session), Times.Once);
        _sessionRepositoryMock.Verify(repository => repository.AddBovineChatSessionAsync(It.IsAny<BovineChatSession>()), Times.Never);
        _alertContextFacadeMock.Verify(facade => facade.GetBovineAlertContextAsync(userId, bovineId), Times.Once);
        _analysisRepositoryMock.Verify(repository => repository.FindByBovineIdAsync(bovineId), Times.Once);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleAnalyzePhotoCommand_UsesBovineContextPersistsAnalysisAndCommits()
    {
        // Arrange
        const int userId = 7;
        const int bovineId = 22;
        const string imageBase64 = "base64-image-content";
        const string bovineContext = "Bovine id: 22\nName: Lola\nBreed: Angus";
        var analysis = new BovineAnalysis(
            userId,
            bovineId,
            94m,
            "No visible issues",
            UrgencyLevel.Green,
            "Continue routine monitoring.",
            0.96m);

        _ranchContextFacadeMock
            .Setup(facade => facade.GetBovineContextAsync(userId, bovineId))
            .ReturnsAsync(bovineContext);
        _visionServiceMock
            .Setup(service => service.AnalyzeBovinePhotoAsync(userId, bovineId, bovineContext, imageBase64))
            .ReturnsAsync(analysis);

        var service = CreateService();
        var command = new AnalyzePhotoCommand(userId, bovineId, imageBase64);

        // Act
        var result = await service.Handle(command);

        // Assert
        Assert.Same(analysis, result);
        _visionServiceMock.Verify(
            service => service.AnalyzeBovinePhotoAsync(userId, bovineId, bovineContext, imageBase64),
            Times.Once);
        _analysisRepositoryMock.Verify(repository => repository.AddAsync(analysis), Times.Once);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    private AIAssistantCommandService CreateService()
    {
        return new AIAssistantCommandService(
            _sessionRepositoryMock.Object,
            _analysisRepositoryMock.Object,
            _chatServiceMock.Object,
            _visionServiceMock.Object,
            _ranchContextFacadeMock.Object,
            _alertContextFacadeMock.Object,
            _ioTContextFacadeMock.Object,
            _unitOfWorkMock.Object);
    }
}
