using System.ComponentModel.DataAnnotations;

namespace VacApp_Bovinova_Platform.IAM.Interfaces.REST.Resources.UserResources
{
    public record SignUpResource(
        [Required][MinLength(3)] string Username,
        [Required][EmailAddress] string Email,
        [Required] string Password
    );
}