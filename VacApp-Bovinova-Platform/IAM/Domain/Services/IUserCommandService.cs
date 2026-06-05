using VacApp_Bovinova_Platform.IAM.Domain.Model.Commands;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;

namespace VacApp_Bovinova_Platform.IAM.Domain.Services
{
    public interface IUserCommandService
    {
        Task<string> Handle(SignUpCommand command);
        Task<string> Handle(SignInCommand command);
        Task<User?> Handle(UpdateUserCommand command);
        Task<User?> Handle(ChangeSubscriptionCommand command);
    }
}