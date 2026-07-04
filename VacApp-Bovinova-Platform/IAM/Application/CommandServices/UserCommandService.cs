using System.Security.Cryptography;
using VacApp_Bovinova_Platform.IAM.Application.OutBoundServices;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Commands;
using VacApp_Bovinova_Platform.IAM.Domain.Repositories;
using VacApp_Bovinova_Platform.IAM.Domain.Services;
using VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;
using VacApp_Bovinova_Platform.SubscriptionManagement.Application.Outbound;

namespace VacApp_Bovinova_Platform.IAM.Application.CommandServices
{
    public class UserCommandService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IHashingService hashingService,
        ITokenService tokenService,
        IPasswordResetTokenRepository resetTokenRepository,
        IEmailSender emailSender
    ) : IUserCommandService
    {
        // How long an emailed recovery code stays valid.
        private const int ResetCodeTtlMinutes = 15;

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

        public async Task Handle(RequestPasswordResetCommand command)
        {
            var user = await userRepository.FindByEmailAsync(command.Email);

            // Silently succeed when the email is unknown: the endpoint must never reveal
            // whether an account exists (prevents user enumeration).
            if (user is null) return;

            // Only the newest code should ever be usable.
            await resetTokenRepository.InvalidateAllForUserAsync(user.Id);

            var code = GenerateSixDigitCode();
            var token = new PasswordResetToken(
                user.Id,
                hashingService.GenerateHash(code),
                DateTime.UtcNow.AddMinutes(ResetCodeTtlMinutes));

            await resetTokenRepository.AddAsync(token);
            await unitOfWork.CompleteAsync();

            // Email delivery must not roll back the token: the code is already persisted.
            await emailSender.SendAsync(
                user.Email,
                "Código de recuperación de contraseña — VacApp",
                BuildResetEmailHtml(user.Username, code, ResetCodeTtlMinutes));
        }

        public async Task<bool> Handle(ResetPasswordCommand command)
        {
            var user = await userRepository.FindByEmailAsync(command.Email);
            if (user is null) return false;

            var token = await resetTokenRepository.FindActiveByUserIdAsync(user.Id);
            if (token is null || token.ExceededAttempts) return false;

            if (!hashingService.VerifyHash(command.Code, token.CodeHash))
            {
                token.RegisterFailedAttempt();
                await unitOfWork.CompleteAsync();
                return false;
            }

            // Reuse the update path so the new password is BCrypt-hashed the same way.
            user.Update(new UpdateUserCommand(user.Id, null, hashingService.GenerateHash(command.NewPassword)));
            token.MarkUsed();
            await unitOfWork.CompleteAsync();
            return true;
        }

        /// <summary>Cryptographically-strong 6-digit code (000000–999999), zero-padded.</summary>
        private static string GenerateSixDigitCode()
            => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        private static string BuildResetEmailHtml(string username, string code, int ttlMinutes) => $@"
<div style=""font-family: Arial, Helvetica, sans-serif; max-width: 480px; margin: 0 auto; color: #1f2937;"">
  <h2 style=""color: #047857;"">Recuperación de contraseña</h2>
  <p>Hola {username},</p>
  <p>Usa este código para restablecer tu contraseña en VacApp:</p>
  <p style=""font-size: 34px; font-weight: bold; letter-spacing: 8px; color: #047857; margin: 24px 0;"">{code}</p>
  <p>El código vence en {ttlMinutes} minutos y solo puede usarse una vez.</p>
  <p style=""color: #6b7280; font-size: 13px;"">Si no solicitaste este cambio, ignora este correo; tu contraseña seguirá igual.</p>
</div>";
    }
}