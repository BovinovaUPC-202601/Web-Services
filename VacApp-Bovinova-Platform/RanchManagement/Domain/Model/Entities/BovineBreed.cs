namespace VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Entities
{
    public class BovineBreed
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public double MinTemperature { get; private set; }
        public double MaxTemperature { get; private set; }
        public int MinHeartRate { get; private set; }
        public int MaxHeartRate { get; private set; }

        protected BovineBreed() { }

        public BovineBreed(int id, string name, double minTemp, double maxTemp, int minHr, int maxHr)
        {
            Id = id;
            Name = name;
            MinTemperature = minTemp;
            MaxTemperature = maxTemp;
            MinHeartRate = minHr;
            MaxHeartRate = maxHr;
        }
    }
}