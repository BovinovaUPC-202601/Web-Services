using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VacApp_Bovinova_Platform.IAM.Domain.Model;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;

namespace VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes
{
    /// <summary>
    ///     Authorization filter that restricts an endpoint to users on the Plus plan.
    ///     Relies on <see cref="AuthorizeAttribute"/> / the auth middleware having already
    ///     placed the authenticated user in HttpContext.Items["User"].
    ///     The plan is read from the EFFECTIVE owner: active staff of a Plus rancher
    ///     keep access to Plus features even though their own account is Free.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequiresPlusAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.Items["User"] as User;

            if (user is null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Resolved per request from the DB (inactive staff throws 403 via the
            // exception middleware), so access changes never depend on a stale token.
            var staffAccessService = context.HttpContext.RequestServices
                .GetRequiredService<IStaffAccessService>();
            var owner = await staffAccessService.GetEffectiveOwnerAsync(user);

            // 403 directly (not ForbidResult): the app uses custom JWT middleware,
            // not ASP.NET authentication schemes, so Forbid() would throw → 500.
            if (owner.SubscriptionPlan != SubscriptionPlans.Plus)
                context.Result = new StatusCodeResult(403);
        }
    }
}
