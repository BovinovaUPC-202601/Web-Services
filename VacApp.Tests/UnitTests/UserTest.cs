using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Commands;

namespace VacApp.Tests.UnitTests
{
    public class UserTests
    {
        [Fact]
        public void SignUpUser()
        {
            // Arrange
            var command = new SignUpCommand("usuario1", "usuario1@email.com", "password123");

            // Act
            var user = new User(command);

            // Assert
            Assert.Equal("usuario1", user.Username);
            Assert.Equal("usuario1@email.com", user.Email);
            Assert.Equal("password123", user.Password);
        }

        [Fact]
        public void SignUpUser_InvalidEmail()
        {
            // Arrange
            var command = new SignUpCommand("usuario2", "email_invalido", "password123");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new User(command));
        }

        [Fact]
        public void UpdateUser_FullData()
        {
            // Arrange
            var command = new SignUpCommand("usuario3", "usuario3@email.com", "password123");
            var user = new User(command);
            var updateCommand = new UpdateUserCommand(1, "nuevoUsuario", "nuevaPassword");

            // Act
            user.Update(updateCommand);

            // Assert
            Assert.Equal("nuevoUsuario", user.Username);
            Assert.Equal("nuevaPassword", user.Password);
        }

        [Fact]
        public void UpdateUser_OnlyUsername()
        {
            // Arrange
            var command = new SignUpCommand("usuario4", "usuario4@email.com", "password123");
            var user = new User(command);
            var updateCommand = new UpdateUserCommand(1, "usuarioActualizado", null);

            // Act
            user.Update(updateCommand);

            // Assert
            Assert.Equal("usuarioActualizado", user.Username);
            Assert.Equal("password123", user.Password);
        }

        [Fact]
        public void UpdateUser_OnlyPassword()
        {
            // Arrange
            var command = new SignUpCommand("usuario5", "usuario5@email.com", "password123");
            var user = new User(command);
            var updateCommand = new UpdateUserCommand(1, null, "passwordActualizada");

            // Act
            user.Update(updateCommand);

            // Assert
            Assert.Equal("usuario5", user.Username);
            Assert.Equal("passwordActualizada", user.Password);
        }

        [Fact]
        public void NewUser_DefaultsToFreePlan()
        {
            var user = new User(new SignUpCommand("usuario6", "usuario6@email.com", "password123"));

            Assert.Equal("Free", user.SubscriptionPlan);
        }

        [Fact]
        public void ChangeSubscription_ToPlus()
        {
            var user = new User(new SignUpCommand("usuario7", "usuario7@email.com", "password123"));

            user.ChangeSubscription("Plus");

            Assert.Equal("Plus", user.SubscriptionPlan);
        }

        [Fact]
        public void ChangeSubscription_InvalidPlan_Throws()
        {
            var user = new User(new SignUpCommand("usuario8", "usuario8@email.com", "password123"));

            Assert.Throws<ArgumentException>(() => user.ChangeSubscription("Gold"));
        }
    }
}
