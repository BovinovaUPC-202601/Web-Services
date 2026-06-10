namespace VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources
{
    public record BovineBreedResource(
        int Id,
        string Name,
        double MinTemperature,
        double MaxTemperature,
        int MinHeartRate,
        int MaxHeartRate
    )
    { }
}