using System.ComponentModel.DataAnnotations;

namespace VacApp_Bovinova_Platform.IAM.Interfaces.REST.Resources
{
    public record ForgotPasswordResource(
        [Required][EmailAddress] string Email);
}
