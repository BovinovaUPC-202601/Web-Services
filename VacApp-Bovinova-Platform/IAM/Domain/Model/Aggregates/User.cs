using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EntityFrameworkCore.CreatedUpdatedDate.Contracts;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Commands;
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
        public string SubscriptionPlan { get; private set; } = "Free";

        private User() { }

        public User(SignUpCommand command)
        {
            Username = command.Username;
            Password = command.Password;
            Email = command.Email;
            if (!System.Text.RegularExpressions.Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                throw new ArgumentException("Invalid email format.", nameof(command.Email));
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
            SubscriptionPlan = plan;
        }
    }
}