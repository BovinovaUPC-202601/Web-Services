using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VacApp_Bovinova_Platform.IAM.Interfaces.REST;
using VacApp_Bovinova_Platform.IAM.Domain.Services;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Interfaces.REST.Resources.UserResources;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Commands;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Services;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Queries;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Queries;

namespace VacApp.Tests.IntegrationTests
{
    public class UserControllerIntegrationTests
    {
        private readonly Mock<IUserCommandService> _commandServiceMock;
        private readonly Mock<IUserQueryService> _queryServiceMock;
        private readonly Mock<IBovineQueryService> _bovineQueryMock;
        private readonly Mock<ICampaignQueryService> _campaignQueryMock;
        private readonly Mock<IProductQueryService> _productQueryMock;
        private readonly Mock<IStaffQueryService> _staffQueryMock;
        private readonly Mock<IStableQueryService> _stableQueryMock;
        private readonly Mock<IStaffAccessService> _staffAccessMock;
        private readonly UserController _controller;

        public UserControllerIntegrationTests()
        {
            _commandServiceMock = new Mock<IUserCommandService>();
            _queryServiceMock = new Mock<IUserQueryService>();
            _bovineQueryMock = new Mock<IBovineQueryService>();
            _campaignQueryMock = new Mock<ICampaignQueryService>();
            _productQueryMock = new Mock<IProductQueryService>();
            _staffQueryMock = new Mock<IStaffQueryService>();
            _stableQueryMock = new Mock<IStableQueryService>();
            _staffAccessMock = new Mock<IStaffAccessService>();

            // Default: the authenticated user is an owner (not staff) with full access.
            _staffAccessMock.Setup(x => x.GetActiveStaffAccessAsync(It.IsAny<User>())).ReturnsAsync((Staff?)null);
            _staffAccessMock.Setup(x => x.GetEffectiveUserIdAsync(It.IsAny<User>())).ReturnsAsync((User u) => u.Id);
            _staffAccessMock.Setup(x => x.GetEffectiveOwnerAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
            _staffAccessMock.Setup(x => x.IsStaffAsync(It.IsAny<User>())).ReturnsAsync(false);
            _staffAccessMock.Setup(x => x.IsOwnerAsync(It.IsAny<User>())).ReturnsAsync(true);
            _staffAccessMock.Setup(x => x.CanReadAsync(It.IsAny<User>())).ReturnsAsync(true);
            _staffAccessMock.Setup(x => x.CanEditAsync(It.IsAny<User>())).ReturnsAsync(true);
            _staffAccessMock.Setup(x => x.CanManageStaffAsync(It.IsAny<User>())).ReturnsAsync(true);

            _controller = new UserController(
                _commandServiceMock.Object,
                _queryServiceMock.Object,
                _bovineQueryMock.Object,
                _campaignQueryMock.Object,
                _productQueryMock.Object,
                _staffQueryMock.Object,
                _stableQueryMock.Object,
                _staffAccessMock.Object
            );
        }

        [Fact]
        public async Task SignUp_ReturnsCreated()
        {
            // Arrange
            var user = new User(new SignUpCommand("testuser", "test@email.com", "123456"));
            _commandServiceMock
                .Setup(x => x.Handle(It.IsAny<SignUpCommand>()))
                .ReturnsAsync("fake_token");

            var resource = new SignUpResource("testuser", "test@email.com", "123456");

            // Act
            var result = await _controller.SignUp(resource);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, created.StatusCode);
            var expected = new UserResource("fake_token", user.Username, user.Email);
            Assert.Equal(expected, created.Value);
        }

        [Fact]
        public async Task SignIn_ReturnsOk()
        {
            // Arrange
            var user = new User(new SignUpCommand("testuser", "test@email.com", "123456"));
            _commandServiceMock
                .Setup(x => x.Handle(It.IsAny<SignInCommand>()))
                .ReturnsAsync("fake_token");
            _queryServiceMock
                .Setup(x => x.Handle(It.IsAny<GetUserByEmailQuery>()))
                .ReturnsAsync(user);

            var resource = new SignInResource("test@email.com", "123456");

            // Act
            var result = await _controller.SignIn(resource);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
            var expected = new UserResource("fake_token", user.Username, user.Email);
            Assert.Equal(expected, ok.Value);
        }

        [Fact]
        public async Task UpdateUser_ReturnsOk()
        {
            // Arrange
            var user = new User(new SignUpCommand("testuser", "test@email.com", "123456"));
            _commandServiceMock
                .Setup(x => x.Handle(It.IsAny<UpdateUserCommand>()))
                .ReturnsAsync(user);

            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
            _controller.ControllerContext.HttpContext.Items["User"] = user;

            var resource = new UpdateUserResource("nuevoUsuario", "nuevaPassword");

            // Act
            var result = await _controller.UpdateUser(resource);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
            var expected = new UserProfileResource(user.Username, user.Email);
            Assert.Equal(expected, ok.Value);
        }

        [Fact]
        public async Task GetInfo_ReturnsOk()
        {
            // Arrange
            var user = new User(new SignUpCommand("testuser", "test@email.com", "123456"));
            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
            _controller.ControllerContext.HttpContext.Items["User"] = user;

            _bovineQueryMock.Setup(x => x.Handle(It.IsAny<GetAllBovinesQuery>())).ReturnsAsync(new List<Bovine>());
            _stableQueryMock.Setup(x => x.Handle(It.IsAny<GetAllStablesQuery>())).ReturnsAsync(new List<Stable>());
            _campaignQueryMock.Setup(x => x.Handle(It.IsAny<GetAllCampaignsQuery>())).ReturnsAsync(new List<Campaign>());
            _productQueryMock.Setup(x => x.Handle(It.IsAny<GetProductsByUserIdQuery>())).ReturnsAsync(new List<Product>());
            _staffQueryMock.Setup(x => x.Handle(It.IsAny<GetAllStaffQuery>())).ReturnsAsync(new List<Staff>());

            // Act
            var result = await _controller.GetInfo();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
            var expected = new UserInfoResource(
                user.Id,
                user.Username,
                user.Email,
                user.SubscriptionPlan,
                false,
                user.Id,
                "Owner",
                true,
                true,
                true,
                true,
                0, 0, 0, 0, 0,
                Array.Empty<CampaignInfoResource>()
            );
            Assert.Equal(expected, ok.Value);
        }
    }
}
