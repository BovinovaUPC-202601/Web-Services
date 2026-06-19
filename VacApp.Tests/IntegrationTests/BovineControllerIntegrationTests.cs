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
    public class BovineControllerIntegrationTests
    {
        private readonly Mock<IBovineCommandService> _commandServiceMock;
        private readonly Mock<IBovineQueryService> _queryServiceMock;
        private readonly Mock<IBovineBreedCommandService> _breedCommandServiceMock;
        private readonly Mock<IStaffAccessService> _staffAccessMock;
        private readonly BovineController _controller;
        private readonly User _user;

        public BovineControllerIntegrationTests()
        {
            _commandServiceMock = new Mock<IBovineCommandService>();
            _queryServiceMock = new Mock<IBovineQueryService>();
            _breedCommandServiceMock = new Mock<IBovineBreedCommandService>();
            _staffAccessMock = new Mock<IStaffAccessService>();

            _user = new User(new SignUpCommand("usuario", "email@email.com", "pass"));

            // Default: owner with full access operating on its own data.
            _staffAccessMock.Setup(x => x.CanEditAsync(It.IsAny<User>())).ReturnsAsync(true);
            _staffAccessMock.Setup(x => x.GetEffectiveUserIdAsync(It.IsAny<User>())).ReturnsAsync((User u) => u.Id);

            _controller = new BovineController(
                _commandServiceMock.Object, _queryServiceMock.Object,
                _breedCommandServiceMock.Object, _staffAccessMock.Object);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
            _controller.ControllerContext.HttpContext.Items["User"] = _user;
        }

        [Fact]
        public async Task CreateBovine_ReturnsCreated()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.OpenReadStream()).Returns(Stream.Null);

            var bovine = new Bovine(new CreateBovineCommand("Bovi", "male", DateOnly.FromDateTime(DateTime.Today),
                "Angus", 1, "img.jpg", _user.Id, Stream.Null));
            _commandServiceMock.Setup(x => x.Handle(It.IsAny<CreateBovineCommand>())).ReturnsAsync(bovine);

            var resource = new CreateBovineResource("Bovi", "male", DateOnly.FromDateTime(DateTime.Today), "Angus", fileMock.Object, 1);
            var expectedResource = BovineResourceFromEntityAssembler.ToResourceFromEntity(bovine);

            // Act
            var result = await _controller.CreateBovines(resource);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(expectedResource, createdResult.Value);
        }

        [Fact]
        public async Task GetAllBovine_ReturnsOk()
        {
            // Arrange
            var bovineList = new List<Bovine>
            {
                new Bovine(new CreateBovineCommand("Bovi", "male", DateOnly.FromDateTime(DateTime.Today),
                    "Angus", 1, "img.jpg", _user.Id, Stream.Null))
            };
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetAllBovinesQuery>())).ReturnsAsync(bovineList);
            var expectedResources = bovineList.Select(BovineResourceFromEntityAssembler.ToResourceFromEntity);

            // Act
            var result = await _controller.GetAllBovine();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResources, okResult.Value);
        }

        [Fact]
        public async Task GetBovineById_ReturnsOk()
        {
            // Arrange
            var bovine = new Bovine(new CreateBovineCommand("Bovi", "male", DateOnly.FromDateTime(DateTime.Today),
                "Angus", 1, "img.jpg", _user.Id, Stream.Null));
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetBovinesByIdQuery>())).ReturnsAsync(bovine);
            var expectedResource = BovineResourceFromEntityAssembler.ToResourceFromEntity(bovine);

            // Act
            var result = await _controller.GetBovineById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResource, okResult.Value);
        }

        [Fact]
        public async Task GetBovinesByStableId_ReturnsOk()
        {
            // Arrange
            var bovineList = new List<Bovine>
            {
                new Bovine(new CreateBovineCommand("Bovi", "male", DateOnly.FromDateTime(DateTime.Today),
                    "Angus", 1, "img.jpg", _user.Id, Stream.Null))
            };
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetBovinesByStableIdQuery>())).ReturnsAsync(bovineList);
            var expectedResources = bovineList.Select(BovineResourceFromEntityAssembler.ToResourceFromEntity);

            // Act
            var result = await _controller.GetBovinesByStableId(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResources, okResult.Value);
        }

        [Fact]
        public async Task UpdateBovine_ReturnsOk()
        {
            // Arrange
            var bovine = new Bovine(new CreateBovineCommand("Bovi", "male", DateOnly.FromDateTime(DateTime.Today),
                "Angus", 1, "img.jpg", _user.Id, Stream.Null));
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetBovinesByIdQuery>())).ReturnsAsync(bovine);
            _commandServiceMock.Setup(x => x.Handle(It.IsAny<UpdateBovineCommand>())).ReturnsAsync(bovine);

            var resource = new UpdateBovineResource
            {
                Name = "Bovi",
                Gender = "male",
                BirthDate = DateOnly.FromDateTime(DateTime.Today),
                Breed = "Angus",
                StableId = 1
            };
            var expectedResource = BovineResourceFromEntityAssembler.ToResourceFromEntity(bovine);

            // Act
            var result = await _controller.UpdateBovine(1, resource);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResource, okResult.Value);
        }

        [Fact]
        public async Task DeleteBovine_ReturnsOk()
        {
            // Arrange
            var bovine = new Bovine(new CreateBovineCommand("Bovi", "male", DateOnly.FromDateTime(DateTime.Today),
                "Angus", 1, "img.jpg", _user.Id, Stream.Null));
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetBovinesByIdQuery>())).ReturnsAsync(bovine);
            _commandServiceMock.Setup(x => x.Handle(It.IsAny<DeleteBovineCommand>())).ReturnsAsync(bovine);

            // Act
            var result = await _controller.DeleteBovine(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equivalent(new { message = "Deleted successfully" }, okResult.Value);
        }
    }
}
