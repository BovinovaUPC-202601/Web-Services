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
        IStableQueryService stableQueryService
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

            // Total de bovinos
            var bovines = await bovineQueryService.Handle(new GetAllBovinesQuery(user.Id));
            var totalBovines = bovines.Count();

            // Total de establos
            var stables = await stableQueryService.Handle(new GetAllStablesQuery(user.Id));
            var totalStables = stables.Count();

            // Total de campañas
            var campaigns = await campaignQueryService.Handle(new GetAllCampaignsQuery(user.Id));
            var totalCampaigns = campaigns.Count();

            // Total de productos
            var products = await productQueryService.Handle(new GetProductsByUserIdQuery(user.Id));
            var totalProducts = products.Sum(p => p.Quantity);

            // Total de personal
            var staff = await staffQueryService.Handle(new GetAllStaffQuery(user.Id));
            var totalStaff = staff.Count();

            // Próximas campañas
            var today = DateOnly.FromDateTime(DateTime.Now);
            var nextCampaigns = campaigns
                    .Where(c => c.EndDate >= today)
                    .Select(c => new CampaignInfoResource(c.Id, c.Name, c.StartDate, c.EndDate))
                    .ToArray();

            // Build and return the response
            var resource = new UserInfoResource(
                    user.Id,
                    user.Username,
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