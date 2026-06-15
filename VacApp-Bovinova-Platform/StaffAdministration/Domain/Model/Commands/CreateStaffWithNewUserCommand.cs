namespace VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Commands;

public record CreateStaffWithNewUserCommand(
    int OwnerUserId,
    string Name,
    string Email,
    string Password,
    int AccessLevel);
