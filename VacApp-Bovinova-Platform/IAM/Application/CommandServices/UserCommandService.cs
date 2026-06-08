using VacApp_Bovinova_Platform.IAM.Application.OutBoundServices;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Commands;
using VacApp_Bovinova_Platform.IAM.Domain.Repositories;
using VacApp_Bovinova_Platform.IAM.Domain.Services;
using VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.IAM.Application.CommandServices
{
    public class UserCommandService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IHashingService hashingService,
        ITokenService tokenService
    ) : IUserCommandService
    {
        public async Task<string> Handle(SignUpCommand command)
        {
            var hashedCommand = command with { Password = hashingService.GenerateHash(command.Password) };
            var user = new User(hashedCommand);

            var existingUser = await userRepository.FindByEmailAsync(user.Email);

            if (existingUser != null)
                throw new ConflictException("User already exists");

            try
            {
                await userRepository.AddAsync(user);
                await unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return tokenService.GenerateToken(user);
        }

        public async Task<string> Handle(SignInCommand command)
        {
            var user = await userRepository.FindByEmailAsync(command.Email);

            if (user == null || !hashingService.VerifyHash(command.Password, user.Password))
                throw new UnauthorizedRequestException("Invalid username or password");

            return tokenService.GenerateToken(user);
        }

        public async Task<User?> Handle(UpdateUserCommand command)
        {
            var user = await userRepository.FindByIdAsync(command.Id);
            if (user == null)
                return null;

            if (command.Password != null)
            {
                command = command with { Password = hashingService.GenerateHash(command.Password) };
            }

            try
            {
                user.Update(command);
                await unitOfWork.CompleteAsync();
                return user;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<User?> Handle(ChangeSubscriptionCommand command)
        {
            var user = await userRepository.FindByIdAsync(command.Id);
            if (user == null)
                return null;

            user.ChangeSubscription(command.SubscriptionPlan);
            await unitOfWork.CompleteAsync();
            return user;
        }
    }
}