using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Services;
using VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST.Resources;
using VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST.Transform;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes;

namespace VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class CampaignController(ICampaignCommandService campaignCommandService, ICampaignQueryService campaignQueryService)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> CreateCampaign([FromBody] CreateCampaignResource resource)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        var createCampaignCommand = CreateCampaignCommandFromResourceAssembler.ToCommandFromResource(resource, user.Id);
        var result = await campaignCommandService.Handle(createCampaignCommand);
        if (result is null) return BadRequest();
        return CreatedAtAction(nameof(GetCampaignById), new { id = result.Id },
            CampaignResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetCampaignById(int id)
    {
        var getCampaignByIdQuery = new GetCampaignByIdQuery(id);
        var result = await campaignQueryService.Handle(getCampaignByIdQuery);
        if (result is null) return NotFound();
        var resource = CampaignResourceFromEntityAssembler.ToResourceFromEntity(result);
        return Ok(resource);
    }

    [HttpGet("all-campaigns")]
    public async Task<ActionResult> GetAllCampaigns()
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized("User not found in context.");

        var campaigns = await campaignQueryService.Handle(new GetAllCampaignsQuery(user.Id));
        var campaignResources = campaigns.Select(CampaignResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(campaignResources);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateCampaign([FromRoute] int id, [FromBody] UpdateCampaignResource resource)
    {
        // Validación temprana antes de llegar al servicio.
        if (resource.EndDate < resource.StartDate)
            return BadRequest(new { message = "La fecha de fin no puede ser anterior a la fecha de inicio." });

        var command = UpdateCampaignCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var campaign = await campaignCommandService.Handle(command);

        if (campaign is null)
            return NotFound(new { message = "Campaign not found" });

        return Ok(CampaignResourceFromEntityAssembler.ToResourceFromEntity(campaign));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteCampaign([FromRoute] int id)
    {
        var deleted = await campaignCommandService.Handle(new DeleteCampaignCommand(id));

        if (!deleted)
            return NotFound(new { message = "Campaign not found" });

        return Ok(new { message = "Deleted successfully" });
    }
}