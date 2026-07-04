namespace VacApp_Bovinova_Platform.IAM.Domain.Model.Commands
{
    /// <summary>Step 1 of RF-03: the user asks for a recovery code to be emailed.</summary>
    public record RequestPasswordResetCommand(string Email);
}
