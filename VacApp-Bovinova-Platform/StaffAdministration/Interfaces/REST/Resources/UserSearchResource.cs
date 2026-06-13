namespace VacApp_Bovinova_Platform.StaffAdministration.Interfaces.REST.Resources;

/// <summary>Minimal public view of a User for the staff "add existing user" flow. Never expose password or sensitive data here.</summary>
public record UserSearchResource(
    int Id,
    string Username,
    string Email
    );
