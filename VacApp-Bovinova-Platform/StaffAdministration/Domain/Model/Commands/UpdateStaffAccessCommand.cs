namespace VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Commands;

public record UpdateStaffAccessCommand(
    int Id,
    int OwnerUserId,
    int EmployeeStatus,
    int AccessLevel);
