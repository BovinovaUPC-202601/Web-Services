using System.ComponentModel.DataAnnotations;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Commands;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.ValueObjects;
using ValidationException = VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions.ValidationException;

namespace VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Aggregates;

public class Staff
{
    [Required]
    public int Id { get; private set; }

    [Required]
    [StringLength(100)]
    public string Name { get; private set; }

    [StringLength(150)]
    public string Email { get; private set; }

    [Required]
    public EmployeeStatus EmployeeStatus { get; private set; }

    [Required]
    public StaffAccessLevel AccessLevel { get; private set; }

    // Existing field. Kept as the owner/rancher id for compatibility (column user_id).
    public int UserId { get; set; }

    // The real User account this staff member signs in with (null = not linked yet).
    public int? LinkedUserId { get; private set; }

    public Staff()
    {
        Name = "";
        Email = "";
        EmployeeStatus = new EmployeeStatus();
        AccessLevel = StaffAccessLevel.ReadOnly;
    }

    public Staff(string name, int employeeStatus, int userId)
    {
        Name = name;
        Email = "";
        EmployeeStatus = new EmployeeStatus(employeeStatus);
        AccessLevel = StaffAccessLevel.ReadOnly;
        UserId = userId;
    }

    public Staff(CreateStaffCommand command) : this(command.Name, command.EmployeeStatus, command.UserId) { }

    /// <summary>Creates a staff member with real access, linked to a User account.</summary>
    public Staff(string name, string email, int accessLevel, int ownerUserId, int linkedUserId)
    {
        ValidateEmail(email);
        ValidateAccessLevel(accessLevel);
        Name = name;
        Email = email;
        EmployeeStatus = new EmployeeStatus(1); // Active
        AccessLevel = (StaffAccessLevel)accessLevel;
        UserId = ownerUserId;
        LinkedUserId = linkedUserId;
    }

    public bool IsActive => EmployeeStatus.Value == 1;

    public void Update(UpdateStaffCommand command)
    {
        Name = command.Name;
        EmployeeStatus = new EmployeeStatus(command.EmployeeStatus);
    }

    public void UpdateAccess(int employeeStatus, int accessLevel)
    {
        ValidateAccessLevel(accessLevel);
        EmployeeStatus = new EmployeeStatus(employeeStatus);
        AccessLevel = (StaffAccessLevel)accessLevel;
    }

    public void LinkUser(int linkedUserId)
    {
        LinkedUserId = linkedUserId;
    }

    public static void ValidateAccessLevel(int accessLevel)
    {
        if (accessLevel is < 1 or > 3)
            throw new ValidationException("AccessLevel must be 1 (ReadOnly), 2 (Editor) or 3 (Manager).");
    }

    public static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            !System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ValidationException("Invalid email format.");
    }
}
