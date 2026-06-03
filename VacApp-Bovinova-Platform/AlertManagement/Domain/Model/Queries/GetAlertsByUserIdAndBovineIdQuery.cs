namespace VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Queries;

public record GetAlertsByUserIdAndBovineIdQuery(int UserId, int BovineId, int Limit);
