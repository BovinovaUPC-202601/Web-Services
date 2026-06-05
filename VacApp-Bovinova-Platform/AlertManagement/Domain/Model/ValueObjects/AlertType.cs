namespace VacApp_Bovinova_Platform.AlertManagement.Domain.Model.ValueObjects;

public enum AlertType
{
    BiometricAnomaly, // triggered by IoTMonitoring (temperature/heartRate out of range).
                      // The specific condition (fever, hypothermia, tachycardia, …) lives in the alert message.
    VisualAnomaly     // triggered by AIAssistant (high urgency visual diagnosis)
}
