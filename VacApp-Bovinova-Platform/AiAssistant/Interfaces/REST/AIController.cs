using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Model.Queries;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Services;
using VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Resources;
using VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Transform;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;

namespace VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST;

[Authorize]
[RequiresPlus]
[ApiController]
[Route("/api/v1/ai")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("AI Assistant")]
public class AIController(
    IAIAssistantCommandService commandService,
    IAIAssistantQueryService queryService,
    IStaffAccessService staffAccessService) : ControllerBase
{
    [HttpPost("general-chat")]
    [SwaggerOperation(
        Summary = "Send a general farm chat message",
        Description = "Sends a general farm, bovine, alert, or campaign question to the AI assistant.",
        OperationId = "SendGeneralChatMessage")]
    [SwaggerResponse(StatusCodes.Status200OK, "AI response generated", typeof(ChatResponseResource))]
    public async Task<IActionResult> SendGeneralChatMessage([FromBody] GeneralChatMessageResource resource)
    {

        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);
        var command = SendGeneralChatCommandFromResourceAssembler.ToCommandFromResource(resource, user.Id, effectiveUserId);
        var result = await commandService.Handle(command);
        return Ok(new ChatResponseResource(result, "GENERAL"));
    }

    [HttpGet("general-chat")]
    [SwaggerOperation(
        Summary = "Get the general chat history",
        Description = "Returns the stored message history of the authenticated user's general conversation.",
        OperationId = "GetGeneralChatHistory")]
    [SwaggerResponse(StatusCodes.Status200OK, "General chat history", typeof(ChatHistoryResource))]
    public async Task<IActionResult> GetGeneralChatHistory()
    {
        
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        var messages = await queryService.Handle(new GetGeneralChatHistoryQuery(user.Id));
        var resource = new ChatHistoryResource(
            "GENERAL",
            null,
            messages.Select(ChatMessageResourceFromValueObjectAssembler.ToResourceFromValueObject));
        return Ok(resource);
    }

    [HttpPost("bovine-chat")]
    [SwaggerOperation(
        Summary = "Send a bovine-specific chat message",
        Description = "Sends a question about one bovine to the AI assistant.",
        OperationId = "SendBovineChatMessage")]
    [SwaggerResponse(StatusCodes.Status200OK, "AI response generated", typeof(ChatResponseResource))]
    public async Task<IActionResult> SendBovineChatMessage([FromBody] BovineChatMessageResource resource)
    {
        
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);
        var command = SendBovineChatCommandFromResourceAssembler.ToCommandFromResource(resource, user.Id, effectiveUserId);
        var result = await commandService.Handle(command);
        return Ok(new ChatResponseResource(result, "BOVINE"));
    }

    [HttpGet("bovine-chat/{bovineId:int}")]
    [SwaggerOperation(
        Summary = "Get the chat history for a bovine",
        Description = "Returns the stored message history of the authenticated user's conversation about one bovine.",
        OperationId = "GetBovineChatHistory")]
    [SwaggerResponse(StatusCodes.Status200OK, "Bovine chat history", typeof(ChatHistoryResource))]
    public async Task<IActionResult> GetBovineChatHistory(int bovineId)
    {
        
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        var messages = await queryService.Handle(new GetBovineChatHistoryQuery(user.Id, bovineId));
        var resource = new ChatHistoryResource(
            "BOVINE",
            bovineId,
            messages.Select(ChatMessageResourceFromValueObjectAssembler.ToResourceFromValueObject));
        return Ok(resource);
    }

    [HttpGet("analyses/{bovineId:int}")]
    [SwaggerOperation(
        Summary = "Get photo analyses for a bovine",
        Description = "Returns the stored photo analyses for one bovine, most recent first.",
        OperationId = "GetBovineAnalyses")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of bovine analyses", typeof(IEnumerable<BovineAnalysisResource>))]
    public async Task<IActionResult> GetBovineAnalyses(int bovineId)
    {
        
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);
        var analyses = await queryService.Handle(new GetBovineAnalysesQuery(effectiveUserId, bovineId));
        var resources = analyses.Select(BovineAnalysisResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpPost("analyze-photo")]
    [SwaggerOperation(
        Summary = "Analyze a bovine photo",
        Description = "Analyzes a bovine image and returns possible visible issues, urgency, and recommendation.",
        OperationId = "AnalyzeBovinePhoto")]
    [SwaggerResponse(StatusCodes.Status200OK, "Photo analysis completed", typeof(AnalysisResultResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Analysis could not be completed")]
    public async Task<IActionResult> AnalyzeBovinePhoto([FromBody] AnalyzePhotoResource resource)
    {
        
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);
        var command = AnalyzePhotoCommandFromResourceAssembler.ToCommandFromResource(resource, user.Id, effectiveUserId);
        var result = await commandService.Handle(command);
        if (result is null) return BadRequest();

        return Ok(AnalysisResultResourceFromEntityAssembler.ToResourceFromEntity(result));
    }
}
