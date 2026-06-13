using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;
using VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;
using VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Transform;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;

namespace VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST;

[Authorize]
[ApiController]
[Route("/api/v1/stables")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Stables")]
public class StableController(
   IStableCommandService commandService,
   IStableQueryService queryService,
   IStaffAccessService staffAccessService) : ControllerBase
{
    private ObjectResult ForbiddenEdit() =>
        StatusCode(StatusCodes.Status403Forbidden,
            new { message = "Read-only staff cannot create, edit or delete." });

    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new stable",
        Description = "Creates a new stable associated with the effective ranch owner.",
        OperationId = "CreateStables"
    )]
    [SwaggerResponse(StatusCodes.Status201Created, "Stable successfully created", typeof(StableResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
    public async Task<IActionResult> CreateStables([FromBody] CreateStableResource resource)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        if (!await staffAccessService.CanEditAsync(user)) return ForbiddenEdit();
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var command = CreateStableCommandFromResourceAssembler.ToCommandFromResource(resource, effectiveUserId);
        var result = await commandService.Handle(command);
        if (result is null) return BadRequest();
        return CreatedAtAction(nameof(GetStableById), new { id = result.Id },
            StableResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all stables",
        Description = "Get all stables",
        OperationId = "GetAllStable")]
    [SwaggerResponse(StatusCodes.Status200OK, "The list of stables were found", typeof(IEnumerable<StableResource>))]
    public async Task<IActionResult> GetAllStable()
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var stables = await queryService.Handle(new GetAllStablesQuery(effectiveUserId));
        var stableResources = stables.Select(StableResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(stableResources);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetStableById(int id)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var getStableById = new GetStablesByIdQuery(id);
        var result = await queryService.Handle(getStableById);
        // NotFound (not Forbidden) when the stable belongs to another ranch.
        if (result is null || result.UserId != effectiveUserId) return NotFound();
        var resources = StableResourceFromEntityAssembler.ToResourceFromEntity(result);
        return Ok(resources);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStable(int id, [FromBody] UpdateStableResource resource)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        if (!await staffAccessService.CanEditAsync(user)) return ForbiddenEdit();
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var existing = await queryService.Handle(new GetStablesByIdQuery(id));
        if (existing is null || existing.UserId != effectiveUserId) return NotFound();

        var command = UpdateStableCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await commandService.Handle(command);
        if (result is null) return BadRequest();
        return Ok(StableResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStable(int id)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        if (!await staffAccessService.CanEditAsync(user)) return ForbiddenEdit();
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var existing = await queryService.Handle(new GetStablesByIdQuery(id));
        if (existing is null || existing.UserId != effectiveUserId)
            return NotFound(new { message = "Stable not found" });

        var command = new DeleteStableCommand(id);
        var result = await commandService.Handle(command);
        if (result is null)
            return NotFound(new { message = "Stable not found" });
        return Ok(new { message = "Deleted successfully" });
    }
}
