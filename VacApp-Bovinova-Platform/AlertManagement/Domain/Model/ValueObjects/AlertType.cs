namespace VacApp_Bovinova_Platform.AlertManagement.Domain.Model.ValueObjects;

public enum AlertType
{
    Fever,         // triggered by IoTMonitoring (temperature/heartRate out of range)
    VisualAnomaly  // triggered by AIAssistant (high urgency visual diagnosis)
}
