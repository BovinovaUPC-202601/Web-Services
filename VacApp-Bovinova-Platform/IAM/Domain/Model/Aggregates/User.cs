using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EntityFrameworkCore.CreatedUpdatedDate.Contracts;
using VacApp_Bovinova_Platform.IAM.Domain.Model;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Commands;
using ValidationException = VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions.ValidationException;
namespace VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates
{
    public class User : IEntityWithCreatedUpdatedDate
    {
        [Column("CreatedAt")] public DateTimeOffset? CreatedDate { get; set; }
        [Column("UpdatedAt")] public DateTimeOffset? UpdatedDate { get; set; }

        [Required]
        public int Id { get; private set; }

        [Required]
        public string Username { get; private set; }

        [Required]
        public string Password { get; private set; }

        [Required]
        [EmailAddress]
        public string Email { get; private set; }

        [Required]
        public string SubscriptionPlan { get; private set; } = SubscriptionPlans.Free;

        [Required]
        public UserRole Role { get; private set; } = UserRole.Rancher;

        private User() { }

        public User(SignUpCommand command)
        {
            Username = command.Username;
            Password = command.Password;
            Email = command.Email;
            if (!System.Text.RegularExpressions.Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                throw new ValidationException("El formato del email no es válido.");
            }
        }

        public void Update(UpdateUserCommand command)
        {
            if (command.Username != null)
                Username = command.Username;

            if (command.Password != null)
                Password = command.Password;
        }

        public void ChangeSubscription(string plan)
        {
            if (!SubscriptionPlans.IsValid(plan))
                throw new ValidationException($"Plan de suscripción inválido: '{plan}'.");

            SubscriptionPlan = plan;
        }
    }
}