using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Services;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST.Resources;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Commands;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;

namespace VacApp.Tests.IntegrationTests
{
    public class CampaignControllerIntegrationTests
    {
        private readonly Mock<ICampaignCommandService> _commandServiceMock;
        private readonly Mock<ICampaignQueryService> _queryServiceMock;
        private readonly Mock<IStaffAccessService> _staffAccessMock;
        private readonly Mock<IStableQueryService> _stableQueryServiceMock;
        private readonly Mock<IBovineQueryService> _bovineQueryServiceMock;
        private readonly CampaignController _controller;
        private readonly User _user;

        public CampaignControllerIntegrationTests()
        {
            _commandServiceMock = new Mock<ICampaignCommandService>();
            _queryServiceMock = new Mock<ICampaignQueryService>();
            _staffAccessMock = new Mock<IStaffAccessService>();
            _stableQueryServiceMock = new Mock<IStableQueryService>();
            _bovineQueryServiceMock = new Mock<IBovineQueryService>();

            _user = new User(new SignUpCommand("usuario", "email@email.com", "pass"));

            // Default: owner with full access operating on its own data.
            _staffAccessMock.Setup(x => x.CanEditAsync(It.IsAny<User>())).ReturnsAsync(true);
            _staffAccessMock.Setup(x => x.GetEffectiveUserIdAsync(It.IsAny<User>())).ReturnsAsync((User u) => u.Id);

            // Name lookups used when building the campaign resource (no stables/bovines registered).
            _stableQueryServiceMock.Setup(x => x.Handle(It.IsAny<GetAllStablesQuery>())).ReturnsAsync(new List<Stable>());
            _bovineQueryServiceMock.Setup(x => x.Handle(It.IsAny<GetAllBovinesQuery>())).ReturnsAsync(new List<Bovine>());

            _controller = new CampaignController(
                _commandServiceMock.Object, _queryServiceMock.Object, _staffAccessMock.Object,
                _stableQueryServiceMock.Object, _bovineQueryServiceMock.Object);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
            _controller.ControllerContext.HttpContext.Items["User"] = _user;
        }

        [Fact]
        public async Task CreateCampaign_ReturnsCreated()
        {
            // Arrange
            var campaign = new Campaign(new CreateCampaignCommand(
                "Campaña A", "Desc", DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddDays(10)), _user.Id,
                new List<int> { 1 }, new List<int>()
            ));
            _commandServiceMock.Setup(x => x.Handle(It.IsAny<CreateCampaignCommand>())).ReturnsAsync(campaign);

            var resource = new CreateCampaignResource(
                "Campaña A", "Desc", DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
                new List<int> { 1 }, new List<int>()
            );

            // Act
            var result = await _controller.CreateCampaign(resource);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            var returned = Assert.IsType<CampaignResource>(createdResult.Value);
            Assert.Equal("Campaña A", returned.Name);
        }

        [Fact]
        public async Task GetCampaignById_ReturnsOk()
        {
            // Arrange
            var campaign = new Campaign(new CreateCampaignCommand(
                "Campaña A", "Desc", DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddDays(10)), _user.Id,
                new List<int> { 1 }, new List<int>()
            ));
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetCampaignByIdQuery>())).ReturnsAsync(campaign);

            // Act
            var result = await _controller.GetCampaignById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            var returned = Assert.IsType<CampaignResource>(okResult.Value);
            Assert.Equal("Campaña A", returned.Name);
        }

        [Fact]
        public async Task GetAllCampaigns_ReturnsOk()
        {
            // Arrange
            var campaignList = new List<Campaign>
            {
                new Campaign(new CreateCampaignCommand(
                    "Campaña A", "Desc", DateOnly.FromDateTime(DateTime.Today),
                    DateOnly.FromDateTime(DateTime.Today.AddDays(10)), _user.Id,
                    new List<int> { 1 }, new List<int>()
                ))
            };
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetAllCampaignsQuery>())).ReturnsAsync(campaignList);

            // Act
            var result = await _controller.GetAllCampaigns();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            var returned = Assert.IsAssignableFrom<IEnumerable<CampaignResource>>(okResult.Value);
            Assert.Single(returned);
        }

        [Fact]
        public async Task DeleteCampaign_ReturnsOk()
        {
            // Arrange
            var campaign = new Campaign(new CreateCampaignCommand(
                "Campaña A", "Desc", DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddDays(10)), _user.Id,
                new List<int> { 1 }, new List<int>()
            ));
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetCampaignByIdQuery>())).ReturnsAsync(campaign);
            _commandServiceMock.Setup(x => x.Handle(It.IsAny<DeleteCampaignCommand>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteCampaign(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equivalent(new { message = "Campaña eliminada correctamente." }, okResult.Value);
        }

        [Fact]
        public async Task DeleteCampaign_ReturnsNotFound()
        {
            // Arrange: the campaign does not exist for this user, so deletion returns NotFound.
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetCampaignByIdQuery>())).ReturnsAsync((Campaign?)null);

            // Act
            var result = await _controller.DeleteCampaign(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equivalent(new { message = "Campaña no encontrada." }, notFoundResult.Value);
        }
    }
}
