using System.ComponentModel.DataAnnotations;

namespace VacApp_Bovinova_Platform.IAM.Interfaces.REST.Resources
{
    public record ResetPasswordResource(
        [Required][EmailAddress] string Email,
        [Required][StringLength(6, MinimumLength = 6)] string Code,
        [Required][MinLength(6)] string NewPassword);
}
