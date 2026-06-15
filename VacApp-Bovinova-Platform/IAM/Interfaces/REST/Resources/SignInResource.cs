using System.ComponentModel.DataAnnotations;

namespace VacApp_Bovinova_Platform.IAM.Interfaces.REST.Resources.UserResources
{
    public record SignInResource(
        [Required][EmailAddress] string Email,
        [Required] string Password
    );
}