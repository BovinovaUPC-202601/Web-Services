using System.Text;
using VacApp_Bovinova_Platform.AIAssistant.Application.ACL;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Services;

namespace VacApp_Bovinova_Platform.AIAssistant.Infrastructure.ACL;

public class AlertContextFacade(IAlertQueryService alertQueryService) : IAlertContextFacade
{
    private const int MaxAlertsInContext = 5;

    public async Task<string> GetBovineAlertContextAsync(int userId, int bovineId)
    {
        var alerts = (await alertQueryService.Handle(
                new GetAlertsByUserIdAndBovineIdQuery(userId, bovineId, MaxAlertsInContext)))
            .ToList();

        if (!alerts.Any())
            return "No hay alertas registradas para este bovino.";

        var context = new StringBuilder();
        context.AppendLine($"Alertas recientes del bovino seleccionado (maximo {MaxAlertsInContext}):");

        foreach (var alert in alerts)
        {
            context.AppendLine(
                $"- {alert.CreatedAt:u}: tipo {alert.AlertType}, urgencia {alert.UrgencyLevel}, estado {alert.Status}, mensaje: {alert.Message}");
        }

        return context.ToString();
    }
}
