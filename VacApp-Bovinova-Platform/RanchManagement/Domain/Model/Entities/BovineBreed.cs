using System.Globalization;

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
        public int? UserId { get; private set; }

        protected BovineBreed() { Name = string.Empty; }

        public BovineBreed(int id, string name, double minTemp, double maxTemp, int minHr, int maxHr)
        {
            Id = id;
            Name = NormalizeName(name);
            MinTemperature = minTemp;
            MaxTemperature = maxTemp;
            MinHeartRate = minHr;
            MaxHeartRate = maxHr;
        }

        public BovineBreed(string name, double minTemp, double maxTemp, int minHr, int maxHr, int? userId = null)
        {
            Name = NormalizeName(name);
            MinTemperature = minTemp;
            MaxTemperature = maxTemp;
            MinHeartRate = minHr;
            MaxHeartRate = maxHr;
            UserId = userId;
        }

        public void Update(string name, double minTemp, double maxTemp, int minHr, int maxHr)
        {
            Name = NormalizeName(name);
            MinTemperature = minTemp;
            MaxTemperature = maxTemp;
            MinHeartRate = minHr;
            MaxHeartRate = maxHr;
        }

        private static string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return name;
            var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                    words[i] = char.ToUpper(words[i][0], CultureInfo.InvariantCulture)
                               + words[i][1..].ToLower(CultureInfo.InvariantCulture);
            }
            return string.Join(" ", words);
        }
    }
}