namespace VacApp_Bovinova_Platform.IAM.Domain.Model.Commands
{
    /// <summary>Step 2 of RF-03: the user submits the emailed code and a new password.</summary>
    public record ResetPasswordCommand(string Email, string Code, string NewPassword);
}
