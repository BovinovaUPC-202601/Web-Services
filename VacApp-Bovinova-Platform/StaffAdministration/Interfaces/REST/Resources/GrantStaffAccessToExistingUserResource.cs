namespace VacApp_Bovinova_Platform.StaffAdministration.Interfaces.REST.Resources;

public record GrantStaffAccessToExistingUserResource(
    string Email,
    int AccessLevel
    );
