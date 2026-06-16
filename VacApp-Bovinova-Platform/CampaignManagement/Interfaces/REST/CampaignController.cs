using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Services;
using VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST.Resources;
using VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST.Transform;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;

namespace VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class CampaignController(
    ICampaignCommandService campaignCommandService,
    ICampaignQueryService campaignQueryService,
    IStaffAccessService staffAccessService,
    IStableQueryService stableQueryService)
    : ControllerBase
{
    private ObjectResult ForbiddenEdit() =>
        StatusCode(StatusCodes.Status403Forbidden,
            new { message = "Read-only staff cannot create, edit or delete." });

    private async Task<Dictionary<int, string>> GetStableNameMapAsync(int userId)
    {
        var stables = await stableQueryService.Handle(new GetAllStablesQuery(userId));
        return stables.ToDictionary(s => s.Id, s => s.Name);
    }

    private static List<string> ResolveStableNames(Campaign campaign, Dictionary<int, string> stableNameMap)
    {
        return campaign.CampaignStables
            .Select(cs => stableNameMap.TryGetValue(cs.StableId, out var name) ? name : $"Establo #{cs.StableId}")
            .ToList();
    }

    [HttpPost]
    public async Task<ActionResult> CreateCampaign([FromBody] CreateCampaignResource resource)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        if (!await staffAccessService.CanEditAsync(user)) return ForbiddenEdit();
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var createCampaignCommand = CreateCampaignCommandFromResourceAssembler.ToCommandFromResource(resource, effectiveUserId);
        var result = await campaignCommandService.Handle(createCampaignCommand);
        if (result is null) return BadRequest();

        var stableNameMap = await GetStableNameMapAsync(effectiveUserId);
        return CreatedAtAction(nameof(GetCampaignById), new { id = result.Id },
            CampaignResourceFromEntityAssembler.ToResourceFromEntity(result, ResolveStableNames(result, stableNameMap)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetCampaignById(int id)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var getCampaignByIdQuery = new GetCampaignByIdQuery(id);
        var result = await campaignQueryService.Handle(getCampaignByIdQuery);
        // NotFound (not Forbidden) when the campaign belongs to another ranch.
        if (result is null || result.UserId != effectiveUserId) return NotFound();
        var stableNameMap = await GetStableNameMapAsync(effectiveUserId);
        var resource = CampaignResourceFromEntityAssembler.ToResourceFromEntity(result, ResolveStableNames(result, stableNameMap));
        return Ok(resource);
    }

    [HttpGet("all-campaigns")]
    public async Task<ActionResult> GetAllCampaigns()
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var campaigns = await campaignQueryService.Handle(new GetAllCampaignsQuery(effectiveUserId));
        var stableNameMap = await GetStableNameMapAsync(effectiveUserId);
        var campaignResources = campaigns.Select(c =>
            CampaignResourceFromEntityAssembler.ToResourceFromEntity(c, ResolveStableNames(c, stableNameMap)));
        return Ok(campaignResources);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateCampaign([FromRoute] int id, [FromBody] UpdateCampaignResource resource)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        if (!await staffAccessService.CanEditAsync(user)) return ForbiddenEdit();
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        // Validación temprana antes de llegar al servicio.
        if (resource.EndDate < resource.StartDate)
            return BadRequest(new { message = "La fecha de fin no puede ser anterior a la fecha de inicio." });

        var existing = await campaignQueryService.Handle(new GetCampaignByIdQuery(id));
        if (existing is null || existing.UserId != effectiveUserId)
            return NotFound(new { message = "Campaign not found" });

        var command = UpdateCampaignCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var campaign = await campaignCommandService.Handle(command);

        if (campaign is null)
            return NotFound(new { message = "Campaign not found" });

        var stableNameMap = await GetStableNameMapAsync(effectiveUserId);
        return Ok(CampaignResourceFromEntityAssembler.ToResourceFromEntity(campaign, ResolveStableNames(campaign, stableNameMap)));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteCampaign([FromRoute] int id)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        if (!await staffAccessService.CanEditAsync(user)) return ForbiddenEdit();
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var existing = await campaignQueryService.Handle(new GetCampaignByIdQuery(id));
        if (existing is null || existing.UserId != effectiveUserId)
            return NotFound(new { message = "Campaign not found" });

        var deleted = await campaignCommandService.Handle(new DeleteCampaignCommand(id));

        if (!deleted)
            return NotFound(new { message = "Campaign not found" });

        return Ok(new { message = "Deleted successfully" });
    }
}
