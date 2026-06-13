namespace VacApp_Bovinova_Platform.StaffAdministration.Interfaces.REST.Resources;

public record UpdateStaffAccessResource(
    int EmployeeStatus,
    int AccessLevel
    );
