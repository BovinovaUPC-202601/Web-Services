using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;
using ValidationException = VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions.ValidationException;

namespace VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Aggregates;

public class Bovine
{
    [Required]
    public int Id { get; private set; }

    [Required]
    [StringLength(100)]
    public string Name { get; private set; }

    [Required]
    [StringLength(100)]
    public string Gender { get; private set; }

    [Required]
    public DateOnly BirthDate { get; private set; }

    [Required]
    [StringLength(100)]
    public string Breed { get; private set; }

    [Required]
    public int StableId { get; private set; }

    [ForeignKey(nameof(StableId))]
    public Stable? Stable { get; private set; }

    [Required]
    [StringLength(300)]
    public string BovineImg { get; private set; }

    public int UserId { get; set; }

    [Required]
    public double MinTemperature { get; private set; }

    [Required]
    public double MaxTemperature { get; private set; }

    [Required]
    public int MinHeartRate { get; private set; }

    [Required]
    public int MaxHeartRate { get; private set; }
    // -------------------------

    private Bovine()
    {
        Name = "";
        Gender = "Male";
        MinTemperature = 38.0;
        MaxTemperature = 39.3;
        MinHeartRate = 40;
        MaxHeartRate = 80;
    }

    public Bovine(string name, string gender, DateOnly birthDate, string breed, int stableId, string bovineImg, int userId, double minTemperature = 38.0, double maxTemperature = 39.3, int minHeartRate = 40, int maxHeartRate = 80)
    {
        Name = name;
        Gender = gender;
        BirthDate = birthDate;
        Breed = breed;
        BovineImg = bovineImg;
        StableId = stableId;
        UserId = userId;
        MinTemperature = minTemperature;
        MaxTemperature = maxTemperature;
        MinHeartRate = minHeartRate;
        MaxHeartRate = maxHeartRate;
    }

    // Constructor with parameters
    public Bovine(CreateBovineCommand command)
    {
        if (!command.Gender.ToLower().Equals("male") && !command.Gender.ToLower().Equals("female"))
            throw new ValidationException("El género debe ser 'male' o 'female'.");

        Name = command.Name;
        Gender = command.Gender;
        BirthDate = command.BirthDate;
        Breed = command.Breed;
        BovineImg = command.BovineImg;
        StableId = command.StableId;
        UserId = command.UserId;

        MinTemperature = command.MinTemperature != 0 ? command.MinTemperature : 38.0;
        MaxTemperature = command.MaxTemperature != 0 ? command.MaxTemperature : 39.3;
        MinHeartRate = command.MinHeartRate != 0 ? command.MinHeartRate : 40;
        MaxHeartRate = command.MaxHeartRate != 0 ? command.MaxHeartRate : 80;
    }

    //Update Bovine
    public void Update(UpdateBovineCommand command)
    {
        if (command.Name is not null) Name = command.Name;
        if (command.Gender is not null)
        {
            if (!command.Gender.ToLower().Equals("male") && !command.Gender.ToLower().Equals("female"))
                throw new ValidationException("El género debe ser 'male' o 'female'.");
            Gender = command.Gender;
        }
        if (command.BirthDate.HasValue)
            BirthDate = command.BirthDate.Value;
        if (command.Breed is not null) Breed = command.Breed;
        if (command.StableId.HasValue) StableId = command.StableId.Value;
        if (command.MinTemperature.HasValue) MinTemperature = command.MinTemperature.Value;
        if (command.MaxTemperature.HasValue) MaxTemperature = command.MaxTemperature.Value;
        if (command.MinHeartRate.HasValue) MinHeartRate = command.MinHeartRate.Value;
        if (command.MaxHeartRate.HasValue) MaxHeartRate = command.MaxHeartRate.Value;
    }
}