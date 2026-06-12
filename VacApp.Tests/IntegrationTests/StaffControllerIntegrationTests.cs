using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VacApp_Bovinova_Platform.StaffAdministration.Interfaces.REST;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Commands;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Queries;
using VacApp_Bovinova_Platform.StaffAdministration.Interfaces.REST.Resources;
using VacApp_Bovinova_Platform.StaffAdministration.Interfaces.REST.Transform;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Commands;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Queries;
using VacApp_Bovinova_Platform.IAM.Domain.Services;

namespace VacApp.Tests.IntegrationTests
{
    public class StaffControllerIntegrationTests
    {
        private readonly Mock<IStaffCommandService> _commandServiceMock;
        private readonly Mock<IStaffQueryService> _queryServiceMock;
        private readonly Mock<IStaffAccessService> _staffAccessMock;
        private readonly Mock<IUserQueryService> _userQueryMock;
        private readonly StaffController _controller;
        private readonly User _user;

        public StaffControllerIntegrationTests()
        {
            _commandServiceMock = new Mock<IStaffCommandService>();
            _queryServiceMock = new Mock<IStaffQueryService>();
            _staffAccessMock = new Mock<IStaffAccessService>();
            _userQueryMock = new Mock<IUserQueryService>();

            _user = new User(new SignUpCommand(
                "usuario", "email@email.com", "pass"
            ));

            // Default: the authenticated user is the ranch owner with full staff management rights.
            _staffAccessMock.Setup(x => x.CanManageStaffAsync(It.IsAny<User>())).ReturnsAsync(true);
            _staffAccessMock.Setup(x => x.GetEffectiveUserIdAsync(It.IsAny<User>())).ReturnsAsync((User u) => u.Id);

            _controller = new StaffController(
                _commandServiceMock.Object,
                _queryServiceMock.Object,
                _staffAccessMock.Object,
                _userQueryMock.Object);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
            _controller.ControllerContext.HttpContext.Items["User"] = _user;
        }

        [Fact]
        public async Task CreateStaff_ReturnsCreated()
        {
            // Arrange
            var staff = new Staff("Juan Perez", 1, _user.Id);
            _commandServiceMock.Setup(x => x.Handle(It.IsAny<CreateStaffCommand>())).ReturnsAsync(staff);

            var resource = new CreateStaffResource("Juan Perez", 1);
            var expectedResource = StaffResourceFromEntityAssembler.ToResourceFromEntity(staff);

            // Act
            var result = await _controller.CreateStaffs(resource);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(expectedResource, createdResult.Value);
        }

        [Fact]
        public async Task GetAllStaff_ReturnsOk()
        {
            // Arrange
            var staffList = new List<Staff> { new Staff("Juan Perez", 1, _user.Id) };
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetAllStaffQuery>())).ReturnsAsync(staffList);
            var expectedResources = staffList.Select(StaffResourceFromEntityAssembler.ToResourceFromEntity);

            // Act
            var result = await _controller.GetAllStaff();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResources, okResult.Value);
        }

        [Fact]
        public async Task GetAllStaff_Returns403_WhenCallerCannotManageStaff()
        {
            // Arrange: ReadOnly/Editor staff cannot see the staff module at all.
            _staffAccessMock.Setup(x => x.CanManageStaffAsync(It.IsAny<User>())).ReturnsAsync(false);

            // Act
            var result = await _controller.GetAllStaff();

            // Assert
            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, forbidden.StatusCode);
        }

        [Fact]
        public async Task GetStaffById_ReturnsOk()
        {
            // Arrange
            var staff = new Staff("Juan Perez", 1, _user.Id);
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetStaffByIdQuery>())).ReturnsAsync(staff);
            var expectedResource = StaffResourceFromEntityAssembler.ToResourceFromEntity(staff);

            // Act
            var result = await _controller.GetStaffById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResource, okResult.Value);
        }

        [Fact]
        public async Task GetStaffById_ReturnsNotFound_WhenStaffBelongsToAnotherOwner()
        {
            // Arrange: the staff exists but belongs to another ranch (owner id 999).
            var foreignStaff = new Staff("Ajeno", 1, 999);
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetStaffByIdQuery>())).ReturnsAsync(foreignStaff);

            // Act
            var result = await _controller.GetStaffById(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetStaffByEmployeeStatus_ReturnsOk()
        {
            // Arrange: the endpoint now filters the owner's staff in memory.
            var staffList = new List<Staff> { new Staff("Juan Perez", 1, _user.Id) };
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetAllStaffQuery>())).ReturnsAsync(staffList);
            var expectedResources = staffList.Select(StaffResourceFromEntityAssembler.ToResourceFromEntity);

            // Act
            var result = await _controller.GetStaffByEmployeeStatus(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResources, okResult.Value);
        }

        [Fact]
        public async Task UpdateStaff_ReturnsOk()
        {
            // Arrange
            var staff = new Staff("Juan Perez", 1, _user.Id);
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetStaffByIdQuery>())).ReturnsAsync(staff);
            _commandServiceMock.Setup(x => x.Handle(It.IsAny<UpdateStaffCommand>())).ReturnsAsync(staff);

            var resource = new UpdateStaffResource("Juan Actualizado", 2);
            var expectedResource = StaffResourceFromEntityAssembler.ToResourceFromEntity(staff);

            // Act
            var result = await _controller.UpdateStaff(1, resource);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResource, okResult.Value);
        }

        [Fact]
        public async Task UpdateStaffAccess_ReturnsOk()
        {
            // Arrange
            var staff = new Staff("Juan Perez", "juan@email.com", 2, _user.Id, 55);
            _commandServiceMock.Setup(x => x.Handle(It.IsAny<UpdateStaffAccessCommand>())).ReturnsAsync(staff);

            var resource = new UpdateStaffAccessResource(1, 2);
            var expectedResource = StaffResourceFromEntityAssembler.ToResourceFromEntity(staff);

            // Act
            var result = await _controller.UpdateStaffAccess(1, resource);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResource, okResult.Value);
        }

        [Fact]
        public async Task CreateStaffWithNewUser_ReturnsCreated()
        {
            // Arrange
            var staff = new Staff("Juan Perez", "juan@email.com", 2, _user.Id, 55);
            _commandServiceMock.Setup(x => x.Handle(It.IsAny<CreateStaffWithNewUserCommand>())).ReturnsAsync(staff);

            var resource = new CreateStaffWithNewUserResource("Juan Perez", "juan@email.com", "Temporal123", 2);
            var expectedResource = StaffResourceFromEntityAssembler.ToResourceFromEntity(staff);

            // Act
            var result = await _controller.CreateStaffWithNewUser(resource);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(expectedResource, createdResult.Value);
        }

        [Fact]
        public async Task GrantAccessToExistingUser_ReturnsCreated()
        {
            // Arrange
            var staff = new Staff("worker", "worker@email.com", 1, _user.Id, 77);
            _commandServiceMock.Setup(x => x.Handle(It.IsAny<GrantStaffAccessToExistingUserCommand>())).ReturnsAsync(staff);

            var resource = new GrantStaffAccessToExistingUserResource("worker@email.com", 1);
            var expectedResource = StaffResourceFromEntityAssembler.ToResourceFromEntity(staff);

            // Act
            var result = await _controller.GrantAccessToExistingUser(resource);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(expectedResource, createdResult.Value);
        }

        [Fact]
        public async Task SearchUserByEmail_ReturnsOk()
        {
            // Arrange
            var found = new User(new SignUpCommand("worker", "worker@email.com", "pass"));
            _userQueryMock.Setup(x => x.Handle(It.IsAny<GetUserByEmailQuery>())).ReturnsAsync(found);

            // Act
            var result = await _controller.SearchUserByEmail("worker@email.com");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resource = Assert.IsType<UserSearchResource>(okResult.Value);
            Assert.Equal("worker", resource.Username);
            Assert.Equal("worker@email.com", resource.Email);
        }

        [Fact]
        public async Task SearchUserByEmail_ReturnsNotFound_WhenNoUser()
        {
            // Arrange
            _userQueryMock.Setup(x => x.Handle(It.IsAny<GetUserByEmailQuery>())).ReturnsAsync((User?)null);

            // Act
            var result = await _controller.SearchUserByEmail("nadie@email.com");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteStaff_ReturnsNoContent()
        {
            // Arrange
            var staff = new Staff("Juan Perez", 1, _user.Id);
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetStaffByIdQuery>())).ReturnsAsync(staff);
            _commandServiceMock.Setup(x => x.Handle(It.IsAny<DeleteStaffCommand>())).ReturnsAsync(staff);

            // Act
            var result = await _controller.DeleteStaff(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
