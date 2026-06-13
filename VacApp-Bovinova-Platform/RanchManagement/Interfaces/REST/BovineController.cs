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
[Route("/api/v1/bovines")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Bovines")]
public class BovineController(IBovineCommandService commandService,
    IBovineQueryService queryService,
    IStaffAccessService staffAccessService) : ControllerBase
{
    private ObjectResult ForbiddenEdit() =>
        StatusCode(StatusCodes.Status403Forbidden,
            new { message = "Read-only staff cannot create, edit or delete." });

    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new bovine",
        Description = "Creates a new bovine associated with the effective ranch owner.",
        OperationId = "CreateBovines"
    )]
    [SwaggerResponse(StatusCodes.Status201Created, "Bovine successfully created", typeof(BovineResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateBovines([FromForm] CreateBovineResource resource)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        if (!await staffAccessService.CanEditAsync(user)) return ForbiddenEdit();
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var command = CreateBovineCommandFromResourceAssembler.ToCommandFromResource(resource, effectiveUserId);
        var result = await commandService.Handle(command);
        if (result is null) return BadRequest();
        return CreatedAtAction(nameof(GetBovineById), new { id = result.Id },
            BovineResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all bovines",
        Description = "Get all bovines",
        OperationId = "GetAllBovine")]
    [SwaggerResponse(StatusCodes.Status200OK, "The list of bovines were found", typeof(IEnumerable<BovineResource>))]
    public async Task<IActionResult> GetAllBovine()
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var bovines = await queryService.Handle(new GetAllBovinesQuery(effectiveUserId));
        var bovineResources = bovines.Select(BovineResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(bovineResources);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetBovineById(int id)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var getBovineById = new GetBovinesByIdQuery(id);
        var result = await queryService.Handle(getBovineById);
        // NotFound (not Forbidden) when the bovine belongs to another ranch,
        // so the endpoint does not leak the existence of foreign data.
        if (result is null || result.UserId != effectiveUserId) return NotFound();
        var resources = BovineResourceFromEntityAssembler.ToResourceFromEntity(result);
        return Ok(resources);
    }

    [HttpGet("stable/{stableId}")]
    [SwaggerOperation(
        Summary = "Get all bovines by stable ID",
        Description = "Get all bovines by stable ID",
        OperationId = "GetBovinesByStableId")]
    public async Task<ActionResult> GetBovinesByStableId(int stableId)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var getBovinesByStableIdQuery = new GetBovinesByStableIdQuery(stableId);
        var bovines = (await queryService.Handle(getBovinesByStableIdQuery))
            ?.Where(b => b.UserId == effectiveUserId).ToList();
        if (bovines == null || bovines.Count == 0)
            return NotFound();
        var bovineResources = bovines.Select(BovineResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(bovineResources);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBovine(int id, UpdateBovineResource resource)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        if (!await staffAccessService.CanEditAsync(user)) return ForbiddenEdit();
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var existing = await queryService.Handle(new GetBovinesByIdQuery(id));
        if (existing is null || existing.UserId != effectiveUserId) return NotFound();

        var command = UpdateBovineCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await commandService.Handle(command);
        if (result is null) return BadRequest();
        return Ok(BovineResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBovine(int id)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        if (!await staffAccessService.CanEditAsync(user)) return ForbiddenEdit();
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var existing = await queryService.Handle(new GetBovinesByIdQuery(id));
        if (existing is null || existing.UserId != effectiveUserId)
            return NotFound(new { message = "Bovine not found" });

        var command = new DeleteBovineCommand(id);
        var result = await commandService.Handle(command);
        if (result is null)
            return NotFound(new { message = "Bovine not found" });
        return Ok(new { message = "Deleted successfully" });
    }

    [AllowAnonymous]
    [HttpGet("breeds")]
    [SwaggerOperation(
        Summary = "Get all bovine breeds",
        Description = "Get all bovine breeds",
        OperationId = "GetAllBovineBreeds")]
    [SwaggerResponse(StatusCodes.Status200OK, "The list of bovine breeds were found", typeof(IEnumerable<BovineBreedResource>))]
    public async Task<IActionResult> GetAllBovineBreeds()
    {
        var breeds = await queryService.Handle(new GetAllBovineBreedsQuery());
        var breedResources = breeds.Select(BovineBreedResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(breedResources);
    }
}
