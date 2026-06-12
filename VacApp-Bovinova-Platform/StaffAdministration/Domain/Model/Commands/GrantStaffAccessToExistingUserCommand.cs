namespace VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Commands;

public record GrantStaffAccessToExistingUserCommand(
    int OwnerUserId,
    string Email,
    int AccessLevel);
