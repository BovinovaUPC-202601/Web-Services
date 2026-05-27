namespace VacApp_Bovinova_Platform.AlertManagement.Domain.Model.ValueObjects;

public enum UrgencyLevel
{
    Green,  // normal — not persisted
    Yellow, // moderate alert
    Red     // critical — immediate vet required
}
