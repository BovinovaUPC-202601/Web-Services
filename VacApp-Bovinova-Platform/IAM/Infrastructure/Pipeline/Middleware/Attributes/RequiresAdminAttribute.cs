using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VacApp_Bovinova_Platform.IAM.Domain.Model;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;

namespace VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes
{
    /// <summary>
    ///     Authorization filter that restricts an endpoint to Admin users.
    ///     Relies on the auth middleware having placed the user in HttpContext.Items["User"].
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequiresAdminAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.Items["User"] as User;

            if (user is null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (user.Role != UserRole.Admin)
                context.Result = new ForbidResult();
        }
    }
}
