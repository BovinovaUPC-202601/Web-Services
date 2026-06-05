using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VacApp_Bovinova_Platform.IAM.Domain.Model;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;

namespace VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes
{
    /// <summary>
    ///     Authorization filter that restricts an endpoint to users on the Plus plan.
    ///     Relies on <see cref="AuthorizeAttribute"/> / the auth middleware having already
    ///     placed the authenticated user in HttpContext.Items["User"].
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequiresPlusAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.Items["User"] as User;

            if (user is null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // 403 directly (not ForbidResult): the app uses custom JWT middleware,
            // not ASP.NET authentication schemes, so Forbid() would throw → 500.
            if (user.SubscriptionPlan != SubscriptionPlans.Plus)
                context.Result = new StatusCodeResult(403);
        }
    }
}
