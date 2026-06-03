using System.Text;
using VacApp_Bovinova_Platform.AIAssistant.Application.ACL;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Queries;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;

namespace VacApp_Bovinova_Platform.AIAssistant.Infrastructure.ACL;

public class IoTContextFacade(IBovineHealthRecordQueryService healthRecordQueryService) : IIoTContextFacade
{
    private const int MaxReadingsInContext = 5;

    public async Task<string> GetBovineTelemetryContextAsync(int bovineId)
    {
        var records = (await healthRecordQueryService.Handle(new GetHealthRecordsByBovineIdQuery(bovineId)))
            .OrderByDescending(record => record.RecordedAt)
            .Take(MaxReadingsInContext)
            .ToList();

        if (records.Count == 0)
            return "No hay lecturas de telemetria IoT registradas para este bovino.";

        var context = new StringBuilder();
        context.AppendLine($"Lecturas IoT recientes del bovino (maximo {MaxReadingsInContext}, rangos normales: temperatura 38.0-39.5 C, ritmo cardiaco 40-80 bpm):");

        foreach (var record in records)
        {
            var status = record.IsAlert ? "FUERA DE RANGO" : "normal";
            context.AppendLine(
                $"- {record.RecordedAt:u} (dispositivo {record.DeviceId}): temperatura {record.Temperature:0.0} C, ritmo cardiaco {record.HeartRate:0} bpm, estado {status}.");
        }

        return context.ToString();
    }
}
