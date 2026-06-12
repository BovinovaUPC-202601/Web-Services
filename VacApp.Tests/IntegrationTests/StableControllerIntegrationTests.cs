using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;
using VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Transform;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Commands;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;

namespace VacApp.Tests.IntegrationTests
{
    public class StableControllerIntegrationTests
    {
        private readonly Mock<IStableCommandService> _commandServiceMock;
        private readonly Mock<IStableQueryService> _queryServiceMock;
        private readonly Mock<IStaffAccessService> _staffAccessMock;
        private readonly StableController _controller;
        private readonly User _user;

        public StableControllerIntegrationTests()
        {
            _commandServiceMock = new Mock<IStableCommandService>();
            _queryServiceMock = new Mock<IStableQueryService>();
            _staffAccessMock = new Mock<IStaffAccessService>();

            _user = new User(new SignUpCommand("usuario", "email@email.com", "pass"));

            // Default: owner with full access operating on its own data.
            _staffAccessMock.Setup(x => x.CanEditAsync(It.IsAny<User>())).ReturnsAsync(true);
            _staffAccessMock.Setup(x => x.GetEffectiveUserIdAsync(It.IsAny<User>())).ReturnsAsync((User u) => u.Id);

            _controller = new StableController(
                _commandServiceMock.Object, _queryServiceMock.Object, _staffAccessMock.Object);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
            _controller.ControllerContext.HttpContext.Items["User"] = _user;
        }

        [Fact]
        public async Task CreateStable_ReturnsCreated()
        {
            // Arrange
            var stable = new Stable(new CreateStableCommand("Stable A", 10, _user.Id));
            _commandServiceMock.Setup(x => x.Handle(It.IsAny<CreateStableCommand>())).ReturnsAsync(stable);

            var resource = new CreateStableResource("Stable A", 10);
            var expectedResource = StableResourceFromEntityAssembler.ToResourceFromEntity(stable);

            // Act
            var result = await _controller.CreateStables(resource);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(expectedResource, createdResult.Value);
        }

        [Fact]
        public async Task GetAllStable_ReturnsOk()
        {
            // Arrange
            var stableList = new List<Stable> { new Stable(new CreateStableCommand("Stable A", 10, _user.Id)) };
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetAllStablesQuery>())).ReturnsAsync(stableList);
            var expectedResources = stableList.Select(StableResourceFromEntityAssembler.ToResourceFromEntity);

            // Act
            var result = await _controller.GetAllStable();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResources, okResult.Value);
        }

        [Fact]
        public async Task GetStableById_ReturnsOk()
        {
            // Arrange
            var stable = new Stable(new CreateStableCommand("Stable A", 10, _user.Id));
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetStablesByIdQuery>())).ReturnsAsync(stable);
            var expectedResource = StableResourceFromEntityAssembler.ToResourceFromEntity(stable);

            // Act
            var result = await _controller.GetStableById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResource, okResult.Value);
        }

        [Fact]
        public async Task UpdateStable_ReturnsOk()
        {
            // Arrange
            var stable = new Stable(new CreateStableCommand("Stable A", 10, _user.Id));
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetStablesByIdQuery>())).ReturnsAsync(stable);
            _commandServiceMock.Setup(x => x.Handle(It.IsAny<UpdateStableCommand>())).ReturnsAsync(stable);

            var resource = new UpdateStableResource { Name = "Stable Updated", Limit = 20 };
            var expectedResource = StableResourceFromEntityAssembler.ToResourceFromEntity(stable);

            // Act
            var result = await _controller.UpdateStable(1, resource);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResource, okResult.Value);
        }

        [Fact]
        public async Task DeleteStable_ReturnsOk()
        {
            // Arrange
            var stable = new Stable(new CreateStableCommand("Stable A", 10, _user.Id));
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetStablesByIdQuery>())).ReturnsAsync(stable);
            _commandServiceMock.Setup(x => x.Handle(It.IsAny<DeleteStableCommand>())).ReturnsAsync(stable);

            // Act
            var result = await _controller.DeleteStable(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equivalent(new { message = "Deleted successfully" }, okResult.Value);
        }
    }
}
