using System.Text;
using VacApp_Bovinova_Platform.AIAssistant.Application.ACL;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Commands;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Entities;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Repositories;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Services;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.AIAssistant.Application.Internal.CommandServices;

public class AIAssistantCommandService(
    IAISessionRepository aiSessionRepository,
    IBovineAnalysisRepository bovineAnalysisRepository,
    IAIChatService aiChatService,
    IAIVisionService aiVisionService,
    IRanchContextFacade ranchContextFacade,
    IUnitOfWork unitOfWork) : IAIAssistantCommandService
{
    public async Task<string> Handle(SendGeneralChatCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Message))
            throw new ArgumentException("Message must not be empty", nameof(command.Message));

        var session = await aiSessionRepository.FindGeneralChatSessionByUserIdAsync(command.UserId);
        var isNewSession = session is null;
        session ??= new GeneralChatSession(command.UserId);

        var systemPrompt = BuildGeneralSystemPrompt(await ranchContextFacade.GetGeneralRanchContextAsync(command.UserId));
        var response = await aiChatService.GenerateResponseAsync(systemPrompt, session.GetMessages(), command.Message);

        session.AddMessage(new ChatMessage("user", command.Message, DateTime.UtcNow));
        session.AddMessage(new ChatMessage("assistant", response, DateTime.UtcNow));

        if (isNewSession)
            await aiSessionRepository.AddGeneralChatSessionAsync(session);
        else
            aiSessionRepository.UpdateGeneralChatSession(session);

        await unitOfWork.CompleteAsync();
        return response;
    }

    public async Task<string> Handle(SendBovineChatCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Message))
            throw new ArgumentException("Message must not be empty", nameof(command.Message));

        var session = await aiSessionRepository.FindBovineChatSessionByUserIdAndBovineIdAsync(command.UserId, command.BovineId);
        var isNewSession = session is null;
        session ??= new BovineChatSession(command.UserId, command.BovineId);

        var bovineContext = await ranchContextFacade.GetBovineContextAsync(command.UserId, command.BovineId);
        var analysisContext = await BuildPreviousAnalysisContext(command.BovineId);
        var systemPrompt = BuildBovineSystemPrompt(bovineContext, analysisContext);
        var response = await aiChatService.GenerateResponseAsync(systemPrompt, session.GetMessages(), command.Message);

        session.AddMessage(new ChatMessage("user", command.Message, DateTime.UtcNow));
        session.AddMessage(new ChatMessage("assistant", response, DateTime.UtcNow));

        if (isNewSession)
            await aiSessionRepository.AddBovineChatSessionAsync(session);
        else
            aiSessionRepository.UpdateBovineChatSession(session);

        await unitOfWork.CompleteAsync();
        return response;
    }

    public async Task<BovineAnalysis?> Handle(AnalyzePhotoCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.ImageBase64))
            throw new ArgumentException("ImageBase64 must not be empty", nameof(command.ImageBase64));

        var bovineContext = await ranchContextFacade.GetBovineContextAsync(command.UserId, command.BovineId);
        var analysis = await aiVisionService.AnalyzeBovinePhotoAsync(
            command.UserId,
            command.BovineId,
            bovineContext,
            command.ImageBase64);

        await bovineAnalysisRepository.AddAsync(analysis);
        await unitOfWork.CompleteAsync();

        return analysis;
    }

    private static string BuildGeneralSystemPrompt(string ranchContext)
    {
        return $"""
                You are VacApp's AI assistant for bovine ranch management.
                Answer using only the provided ranch context and general animal-care guidance.
                Do not provide definitive veterinary diagnoses. Recommend contacting a veterinarian when symptoms or risk are serious.
                If asked about IoT telemetry or alerts that are not present in the context, say that the backend does not have that data available yet.

                Ranch context:
                {ranchContext}
                """;
    }

    private static string BuildBovineSystemPrompt(string bovineContext, string analysisContext)
    {
        return $"""
                You are VacApp's AI assistant for a specific bovine.
                Use the bovine context and previous visual analyses to answer.
                Do not provide definitive veterinary diagnoses. Explain observations as preventive guidance and recommend a veterinarian for serious cases.
                If biometric IoT telemetry is unavailable, state that clearly.

                Bovine context:
                {bovineContext}

                Previous visual analysis context:
                {analysisContext}
                """;
    }

    private async Task<string> BuildPreviousAnalysisContext(int bovineId)
    {
        var analyses = (await bovineAnalysisRepository.FindByBovineIdAsync(bovineId)).Take(5).ToList();
        if (!analyses.Any())
            return "No previous visual analyses are registered for this bovine.";

        var context = new StringBuilder();
        foreach (var analysis in analyses)
        {
            context.AppendLine(
                $"- {analysis.CreatedAt:u}: urgency {analysis.UrgencyLevel}, score {analysis.Score}, confidence {analysis.Confidence}, issues: {analysis.VisibleIssues}, recommendation: {analysis.Recommendation}");
        }

        return context.ToString();
    }
}
