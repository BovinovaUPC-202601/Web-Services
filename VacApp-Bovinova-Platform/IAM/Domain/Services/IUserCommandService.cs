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

        /// <summary>RF-03 step 1: emails a recovery code. Never throws for unknown emails.</summary>
        Task Handle(RequestPasswordResetCommand command);

        /// <summary>RF-03 step 2: validates the code and sets the new password. Returns false when invalid/expired.</summary>
        Task<bool> Handle(ResetPasswordCommand command);
    }
}