using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Services;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Queries;
using VacApp_Bovinova_Platform.IAM.Domain.Services;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using VacApp_Bovinova_Platform.IAM.Interfaces.REST.Resources.UserResources;
using VacApp_Bovinova_Platform.IAM.Interfaces.REST.Transform;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Queries;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;
using VacApp_Bovinova_Platform.IAM.Interfaces.REST.Resources;


namespace VacApp_Bovinova_Platform.IAM.Interfaces.REST
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [Tags("Users")]
    public class UserController(
        IUserCommandService commandService,
        IUserQueryService queryService,
        IBovineQueryService bovineQueryService,
        ICampaignQueryService campaignQueryService,
    IProductQueryService productQueryService,
        IStaffQueryService staffQueryService,
        IStableQueryService stableQueryService,
        IStaffAccessService staffAccessService
        ) : ControllerBase
    {
        [HttpPost("sign-up")]
        [AllowAnonymous]
        public async Task<IActionResult> SignUp([FromBody] SignUpResource resource)
        {
            var command = SignUpCommandFromResourceAssembler.ToCommandFromResource(resource);
            var result = await commandService.Handle(command);

            if (result is null) return BadRequest("User already exists");

            var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(result, resource.Username, resource.Email);

            return CreatedAtAction(nameof(SignUp), userResource);
        }

        [HttpPost("sign-in")]
        [AllowAnonymous]
        public async Task<ActionResult> SignIn([FromBody] SignInResource resource)
        {
            var command = SignInCommandFromResourceAssembler.ToCommandFromResource(resource);
            var result = await commandService.Handle(command);

            if (result is null) return BadRequest("Invalid credentials.");

            var user = await queryService.Handle(new GetUserByEmailQuery(resource.Email));
            if (user is null) return NotFound("User not found.");
            var userName = user.Username;
            var email = user.Email;

            var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(result, userName, email);

            return Ok(userResource);
        }

        [HttpGet("profile")]
        [SwaggerResponse(StatusCodes.Status200OK, "User info", typeof(UserInfoResource))]
        public async Task<ActionResult> GetInfo()
        {
            var user = HttpContext.Items["User"] as User;
            if (user is null)
                return Unauthorized("User not found in context.");

            // Resolve who the user operates as. Permissions are read from the DB
            // (never from the JWT) so access changes apply without a new token.
            // Throws 403 when the user is staff with inactive access.
            var staffAccess = await staffAccessService.GetActiveStaffAccessAsync(user);
            var isStaff = staffAccess is not null;
            var effectiveUserId = staffAccess?.UserId ?? user.Id;
            var owner = await staffAccessService.GetEffectiveOwnerAsync(user);

            var accessLevel = staffAccess is null ? "Owner" : staffAccess.AccessLevel.ToString();
            var canEdit = staffAccess is null || staffAccess.AccessLevel >= StaffAccessLevel.Editor;
            var canManageStaff = staffAccess is null || staffAccess.AccessLevel == StaffAccessLevel.Manager;
            var canManageSubscription = staffAccess is null;

            // Total de bovinos
            var bovines = await bovineQueryService.Handle(new GetAllBovinesQuery(effectiveUserId));
            var totalBovines = bovines.Count();

            // Total de establos
            var stables = await stableQueryService.Handle(new GetAllStablesQuery(effectiveUserId));
            var totalStables = stables.Count();

            // Total de campañas
            var campaigns = await campaignQueryService.Handle(new GetAllCampaignsQuery(effectiveUserId));
            var totalCampaigns = campaigns.Count();

            // Total de productos
            var products = await productQueryService.Handle(new GetProductsByUserIdQuery(effectiveUserId));
            var totalProducts = products.Sum(p => p.Quantity);

            // Total de personal
            var staff = await staffQueryService.Handle(new GetAllStaffQuery(effectiveUserId));
            var totalStaff = staff.Count();

            // Próximas campañas
            var today = DateOnly.FromDateTime(DateTime.Now);
            var nextCampaigns = campaigns
                    .Where(c => c.EndDate >= today)
                    .Select(c => new CampaignInfoResource(c.Id, c.Name, c.StartDate, c.EndDate))
                    .ToArray();

            // Build and return the response. subscriptionPlan is the effective
            // owner's plan so staff of a Plus rancher keep Plus features unlocked.
            var resource = new UserInfoResource(
                    user.Id,
                    user.Username,
                    user.Email,
                    owner.SubscriptionPlan,
                    isStaff,
                    effectiveUserId,
                    accessLevel,
                    true,
                    canEdit,
                    canManageStaff,
                    canManageSubscription,
                    totalBovines,
                    totalCampaigns,
                    totalStaff,
                    totalProducts,
                    totalStables,
                    nextCampaigns);
            return Ok(resource);
        }

        [HttpPut("update")]
        [SwaggerResponse(StatusCodes.Status200OK, "Usuario actualizado", typeof(UserResource))]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserResource resource)
        {
            var user = HttpContext.Items["User"] as User;
            if (user is null)
                return Unauthorized("User not found in context.");

            var command = UpdateUserCommandFromResourceAssembler.ToCommandFromResource(resource, user.Id);
            var updatedUser = await commandService.Handle(command);

            if (updatedUser is null) return NotFound("User not found or update failed.");

            var userResource = new UserProfileResource(updatedUser.Username, updatedUser.Email);
            return Ok(userResource);
        }

        [HttpPut("subscription")]
        [SwaggerResponse(StatusCodes.Status200OK, "Subscription updated")]
        public async Task<IActionResult> UpdateSubscription([FromBody] UpdateSubscriptionResource resource)
        {
            var user = HttpContext.Items["User"] as User;

            if (user is null)
                return Unauthorized("User not found in context.");

            // Subscription is owner-only: staff of any level must not manage it.
            if (await staffAccessService.IsStaffAsync(user))
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Only the ranch owner can manage the subscription." });

            try
            {
                var command = ChangeSubscriptionCommandFromResourceAssembler.ToCommandFromResource(resource, user.Id);
                var updatedUser = await commandService.Handle(command);

                if (updatedUser is null) return NotFound("User not found.");

                return Ok(new
                {
                    message = "Subscription updated",
                    subscription = updatedUser.SubscriptionPlan
                });
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}