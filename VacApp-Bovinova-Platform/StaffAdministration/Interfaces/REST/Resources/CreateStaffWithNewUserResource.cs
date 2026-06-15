namespace VacApp_Bovinova_Platform.StaffAdministration.Interfaces.REST.Resources;

public record CreateStaffWithNewUserResource(
    string Name,
    string Email,
    string Password,
    int AccessLevel
    );
