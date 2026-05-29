using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Services;
using VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Resources;
using VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST.Transform;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes;

namespace VacApp_Bovinova_Platform.AIAssistant.Interfaces.REST;

[Authorize]
[ApiController]
[Route("/api/v1/ai")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("AI Assistant")]
public class AIController(IAIAssistantCommandService commandService) : ControllerBase
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

        var command = SendGeneralChatCommandFromResourceAssembler.ToCommandFromResource(resource, user.Id);
        var result = await commandService.Handle(command);
        return Ok(new ChatResponseResource(result, "GENERAL"));
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

        var command = SendBovineChatCommandFromResourceAssembler.ToCommandFromResource(resource, user.Id);
        var result = await commandService.Handle(command);
        return Ok(new ChatResponseResource(result, "BOVINE"));
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

        var command = AnalyzePhotoCommandFromResourceAssembler.ToCommandFromResource(resource, user.Id);
        var result = await commandService.Handle(command);
        if (result is null) return BadRequest();

        return Ok(AnalysisResultResourceFromEntityAssembler.ToResourceFromEntity(result));
    }
}
