namespace VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Commands;

/// <summary>
/// Raises an account-level alert telling the user to return their IoT collars after the
/// Plus plan ended (subscription suspended). <paramref name="CollarCount"/> is how many
/// collars they currently hold and must return.
/// </summary>
public record RegisterCollarReturnAlertCommand(int UserId, int CollarCount);
