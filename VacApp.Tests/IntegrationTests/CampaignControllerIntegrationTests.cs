using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Services;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST.Resources;
using VacApp_Bovinova_Platform.CampaignManagement.Interfaces.REST.Transform;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Commands;

namespace VacApp.Tests.IntegrationTests
{
    public class CampaignControllerIntegrationTests
    {
        private readonly Mock<ICampaignCommandService> _commandServiceMock;
        private readonly Mock<ICampaignQueryService> _queryServiceMock;
        private readonly CampaignController _controller;
        private readonly User _user;

        public CampaignControllerIntegrationTests()
        {
            _commandServiceMock = new Mock<ICampaignCommandService>();
            _queryServiceMock = new Mock<ICampaignQueryService>();

            _user = new User(new SignUpCommand("usuario", "email@email.com", "pass"));

            _controller = new CampaignController(_commandServiceMock.Object, _queryServiceMock.Object);
            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
            _controller.ControllerContext.HttpContext.Items["User"] = _user;
        }

        [Fact]
        public async Task CreateCampaign_ReturnsCreated()
        {
            // Arrange
            var campaign = new Campaign(new CreateCampaignCommand(
                "Campaña A", "Desc", DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddDays(10)), _user.Id
            ));
            _commandServiceMock.Setup(x => x.Handle(It.IsAny<CreateCampaignCommand>())).ReturnsAsync(campaign);

            var resource = new CreateCampaignResource(
                "Campaña A", "Desc", DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddDays(10))
            );
            var expectedResource = CampaignResourceFromEntityAssembler.ToResourceFromEntity(campaign);

            // Act
            var result = await _controller.CreateCampaign(resource);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(expectedResource, createdResult.Value);
        }

        [Fact]
        public async Task GetCampaignById_ReturnsOk()
        {
            // Arrange
            var campaign = new Campaign(new CreateCampaignCommand(
                "Campaña A", "Desc", DateOnly.FromDateTime(DateTime.Today),
                DateOnly.FromDateTime(DateTime.Today.AddDays(10)), _user.Id
            ));
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetCampaignByIdQuery>())).ReturnsAsync(campaign);
            var expectedResource = CampaignResourceFromEntityAssembler.ToResourceFromEntity(campaign);

            // Act
            var result = await _controller.GetCampaignById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResource, okResult.Value);
        }

        [Fact]
        public async Task GetAllCampaigns_ReturnsOk()
        {
            // Arrange
            var campaignList = new List<Campaign>
            {
                new Campaign(new CreateCampaignCommand(
                    "Campaña A", "Desc", DateOnly.FromDateTime(DateTime.Today),
                    DateOnly.FromDateTime(DateTime.Today.AddDays(10)), _user.Id
                ))
            };
            _queryServiceMock.Setup(x => x.Handle(It.IsAny<GetAllCampaignsQuery>())).ReturnsAsync(campaignList);
            var expectedResources = campaignList.Select(CampaignResourceFromEntityAssembler.ToResourceFromEntity);

            // Act
            var result = await _controller.GetAllCampaigns();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResources, okResult.Value);
        }

        [Fact]
        public async Task DeleteCampaign_ReturnsOk()
        {
            // Arrange
            _commandServiceMock.Setup(x => x.Handle(It.IsAny<DeleteCampaignCommand>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteCampaign(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equivalent(new { message = "Deleted successfully" }, okResult.Value);
        }

        [Fact]
        public async Task DeleteCampaign_ReturnsNotFound()
        {
            // Arrange
            _commandServiceMock.Setup(x => x.Handle(It.IsAny<DeleteCampaignCommand>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteCampaign(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equivalent(new { message = "Campaign not found" }, notFoundResult.Value);
        }
    }
}
