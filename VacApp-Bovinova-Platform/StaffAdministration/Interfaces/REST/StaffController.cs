using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Commands;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Queries;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;
using VacApp_Bovinova_Platform.StaffAdministration.Interfaces.REST.Resources;
using VacApp_Bovinova_Platform.StaffAdministration.Interfaces.REST.Transform;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Queries;
using VacApp_Bovinova_Platform.IAM.Domain.Services;

namespace VacApp_Bovinova_Platform.StaffAdministration.Interfaces.REST;

/// <summary>
/// Staff access management. The whole module is restricted to the ranch owner
/// or staff with Manager access; ReadOnly and Editor staff get 403.
/// </summary>
[Authorize]
[ApiController]
[Route("/api/v1/staff")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Staffs")]
public class StaffController(IStaffCommandService commandService,
    IStaffQueryService queryService,
    IStaffAccessService staffAccessService,
    IUserQueryService userQueryService) : ControllerBase
{
    /// <summary>403 result used when the caller cannot manage staff. Returns null when allowed.</summary>
    private async Task<IActionResult?> ForbidUnlessCanManageStaffAsync(User user)
    {
        if (!await staffAccessService.CanManageStaffAsync(user))
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "Only the ranch owner or a manager can manage staff." });
        return null;
    }

    [HttpPost]
    public async Task<IActionResult> CreateStaffs([FromBody] CreateStaffResource resource)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized();

        if (await ForbidUnlessCanManageStaffAsync(user) is { } forbidden) return forbidden;
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var command = CreateStaffCommandFromResourceAssembler.ToCommandFromResource(resource, effectiveUserId);
        var result = await commandService.Handle(command);
        if (result is null) return BadRequest();

        return CreatedAtAction(nameof(GetStaffById), new { id = result.Id },
            StaffResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all staffs",
        Description = "Get all staffs of the effective ranch owner. Owner or Manager only.",
        OperationId = "GetAllStaff")]
    [SwaggerResponse(StatusCodes.Status200OK, "The list of staffs were found", typeof(IEnumerable<StaffResource>))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "ReadOnly/Editor staff cannot manage staff")]
    public async Task<IActionResult> GetAllStaff()
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized();

        if (await ForbidUnlessCanManageStaffAsync(user) is { } forbidden) return forbidden;
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var staffs = await queryService.Handle(new GetAllStaffQuery(effectiveUserId));
        var staffResources = staffs.Select(StaffResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(staffResources);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetStaffById(int id)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized();

        if (await ForbidUnlessCanManageStaffAsync(user) is { } forbidden) return forbidden;
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var result = await queryService.Handle(new GetStaffByIdQuery(id));
        if (result is null || result.UserId != effectiveUserId) return NotFound();

        var resources = StaffResourceFromEntityAssembler.ToResourceFromEntity(result);
        return Ok(resources);
    }

    [HttpGet("search-by-employee-status/{employeeStatus}")]
    [SwaggerOperation(
        Summary = "Get all staffs by employee status",
        Description = "Get the effective owner's staff filtered by employee status",
        OperationId = "GetStaffByEmployeeStatus")]
    public async Task<IActionResult> GetStaffByEmployeeStatus(int employeeStatus)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized();

        if (await ForbidUnlessCanManageStaffAsync(user) is { } forbidden) return forbidden;
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var staffs = (await queryService.Handle(new GetAllStaffQuery(effectiveUserId)))
            .Where(s => s.EmployeeStatus.Value == employeeStatus)
            .ToList();

        if (staffs.Count == 0)
            return NotFound();

        var staffResources = staffs.Select(StaffResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(staffResources);
    }

    /// <summary>Searches an existing platform user by email to grant them staff access.</summary>
    [HttpGet("users/search")]
    [SwaggerOperation(
        Summary = "Search a user by email",
        Description = "Returns minimal public info (id, username, email) of a user. Owner or Manager only.",
        OperationId = "SearchUserByEmail")]
    [SwaggerResponse(StatusCodes.Status200OK, "User found", typeof(UserSearchResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No user with that email")]
    public async Task<IActionResult> SearchUserByEmail([FromQuery] string email)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized();

        if (await ForbidUnlessCanManageStaffAsync(user) is { } forbidden) return forbidden;

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email is required." });

        var found = await userQueryService.Handle(new GetUserByEmailQuery(email.Trim()));
        if (found is null) return NotFound(new { message = "No user found with that email." });

        // Only minimal, non-sensitive data leaves this endpoint.
        return Ok(new UserSearchResource(found.Id, found.Username, found.Email));
    }

    /// <summary>Creates a brand-new User account and registers it as staff with access.</summary>
    [HttpPost("access/create-user")]
    [SwaggerOperation(
        Summary = "Create a new user and grant staff access",
        OperationId = "CreateStaffWithNewUser")]
    [SwaggerResponse(StatusCodes.Status201Created, "Staff created", typeof(StaffResource))]
    public async Task<IActionResult> CreateStaffWithNewUser([FromBody] CreateStaffWithNewUserResource resource)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized();

        if (await ForbidUnlessCanManageStaffAsync(user) is { } forbidden) return forbidden;
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var command = new CreateStaffWithNewUserCommand(
            effectiveUserId, resource.Name, resource.Email, resource.Password, resource.AccessLevel);
        var result = await commandService.Handle(command);
        if (result is null) return BadRequest();

        return CreatedAtAction(nameof(GetStaffById), new { id = result.Id },
            StaffResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    /// <summary>Grants staff access to an existing platform user found by email.</summary>
    [HttpPost("access/existing-user")]
    [SwaggerOperation(
        Summary = "Grant staff access to an existing user",
        OperationId = "GrantStaffAccessToExistingUser")]
    [SwaggerResponse(StatusCodes.Status201Created, "Staff created", typeof(StaffResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No user with that email")]
    public async Task<IActionResult> GrantAccessToExistingUser([FromBody] GrantStaffAccessToExistingUserResource resource)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized();

        if (await ForbidUnlessCanManageStaffAsync(user) is { } forbidden) return forbidden;
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var command = new GrantStaffAccessToExistingUserCommand(
            effectiveUserId, resource.Email, resource.AccessLevel);
        var result = await commandService.Handle(command);
        if (result is null) return BadRequest();

        return CreatedAtAction(nameof(GetStaffById), new { id = result.Id },
            StaffResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    /// <summary>Updates a staff member's employee status (active/inactive) and access level.</summary>
    [HttpPut("{id:int}/access")]
    [SwaggerOperation(
        Summary = "Update staff access",
        OperationId = "UpdateStaffAccess")]
    [SwaggerResponse(StatusCodes.Status200OK, "Access updated", typeof(StaffResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Staff not found for this owner")]
    public async Task<IActionResult> UpdateStaffAccess(int id, [FromBody] UpdateStaffAccessResource resource)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized();

        if (await ForbidUnlessCanManageStaffAsync(user) is { } forbidden) return forbidden;
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var command = new UpdateStaffAccessCommand(
            id, effectiveUserId, resource.EmployeeStatus, resource.AccessLevel);
        var result = await commandService.Handle(command);
        if (result is null) return NotFound();

        return Ok(StaffResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateStaff(int id, [FromBody] UpdateStaffResource resource)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized();

        if (await ForbidUnlessCanManageStaffAsync(user) is { } forbidden) return forbidden;
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var existing = await queryService.Handle(new GetStaffByIdQuery(id));
        if (existing is null || existing.UserId != effectiveUserId) return NotFound();

        var command = UpdateStaffCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await commandService.Handle(command);
        if (result is null) return BadRequest();

        return Ok(StaffResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStaff(int id)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized();

        if (await ForbidUnlessCanManageStaffAsync(user) is { } forbidden) return forbidden;
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var existing = await queryService.Handle(new GetStaffByIdQuery(id));
        if (existing is null || existing.UserId != effectiveUserId)
            return NotFound(new { message = "Staff not found" });

        // Removes the staff access only; the linked User account is never deleted.
        await commandService.Handle(new DeleteStaffCommand(id));
        return NoContent();
    }
}
