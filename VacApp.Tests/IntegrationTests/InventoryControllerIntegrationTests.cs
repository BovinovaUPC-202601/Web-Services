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
    public class InventoryControllerIntegrationTests
    {
        private readonly Mock<ICategoryCommandService> _categoryCommandServiceMock;
        private readonly Mock<ICategoryQueryService> _categoryQueryServiceMock;
        private readonly Mock<IProductCommandService> _productCommandServiceMock;
        private readonly Mock<IProductQueryService> _productQueryServiceMock;
        private readonly Mock<IStaffAccessService> _staffAccessMock;
        private readonly InventoryController _controller;
        private readonly User _user;

        public InventoryControllerIntegrationTests()
        {
            _categoryCommandServiceMock = new Mock<ICategoryCommandService>();
            _categoryQueryServiceMock = new Mock<ICategoryQueryService>();
            _productCommandServiceMock = new Mock<IProductCommandService>();
            _productQueryServiceMock = new Mock<IProductQueryService>();
            _staffAccessMock = new Mock<IStaffAccessService>();

            _user = new User(new SignUpCommand("usuario", "email@email.com", "pass"));

            // Default: owner with full access operating on its own data.
            _staffAccessMock.Setup(x => x.CanEditAsync(It.IsAny<User>())).ReturnsAsync(true);
            _staffAccessMock.Setup(x => x.GetEffectiveUserIdAsync(It.IsAny<User>())).ReturnsAsync((User u) => u.Id);

            _controller = new InventoryController(
                _categoryCommandServiceMock.Object,
                _categoryQueryServiceMock.Object,
                _productCommandServiceMock.Object,
                _productQueryServiceMock.Object,
                _staffAccessMock.Object
            );

            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
            _controller.ControllerContext.HttpContext.Items["User"] = _user;
        }

        [Fact]
        public async Task CreateCategory_ReturnsCreated()
        {
            // Arrange
            var category = new Category(new CreateCategoryCommand("Cat A", _user.Id));
            _categoryCommandServiceMock.Setup(x => x.Handle(It.IsAny<CreateCategoryCommand>()))
                .ReturnsAsync(category);

            var resource = new CreateCategoryResource("Cat A");
            var expectedResource = CategoryResourceFromEntityAssembler.ToResourceFromEntity(category);

            // Act
            var result = await _controller.CreateCategory(resource);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(expectedResource, createdResult.Value);
        }

        [Fact]
        public async Task GetAllCategories_ReturnsOk()
        {
            // Arrange
            var categoryList = new List<Category> { new Category(new CreateCategoryCommand("Cat A", _user.Id)) };
            _categoryQueryServiceMock.Setup(x => x.Handle(It.IsAny<GetCategoriesByUserIdQuery>()))
                .ReturnsAsync(categoryList);
            var expectedResources = categoryList.Select(CategoryResourceFromEntityAssembler.ToResourceFromEntity);

            // Act
            var result = await _controller.GetAllCategories();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResources, okResult.Value);
        }

        [Fact]
        public async Task GetCategoryById_ReturnsOk()
        {
            // Arrange
            var category = new Category(new CreateCategoryCommand("Cat A", _user.Id));
            _categoryQueryServiceMock.Setup(x => x.Handle(It.IsAny<GetCategoryByIdQuery>()))
                .ReturnsAsync(category);
            var expectedResource = CategoryResourceFromEntityAssembler.ToResourceFromEntity(category);

            // Act
            var result = await _controller.GetCategoryById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResource, okResult.Value);
        }

        [Fact]
        public async Task DeleteCategory_ReturnsOk()
        {
            // Arrange
            var category = new Category(new CreateCategoryCommand("Cat A", _user.Id));
            _categoryQueryServiceMock.Setup(x => x.Handle(It.IsAny<GetCategoryByIdQuery>()))
                .ReturnsAsync(category);
            _categoryCommandServiceMock.Setup(x => x.Handle(It.IsAny<DeleteCategoryCommand>()))
                .ReturnsAsync(category);

            // Act
            var result = await _controller.DeleteCategory(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equivalent(new { message = "Category deleted successfully" }, okResult.Value);
        }

        [Fact]
        public async Task CreateProduct_ReturnsCreated()
        {
            // Arrange
            var product = new Product(new CreateProductCommand("Prod A", 1, 5, _user.Id, null));
            _productCommandServiceMock.Setup(x => x.Handle(It.IsAny<CreateProductCommand>()))
                .ReturnsAsync(product);

            var resource = new CreateProductResource("Prod A", 1, 5, null);
            var expectedResource = ProductResourceFromEntityAssembler.ToResourceFromEntity(product);

            // Act
            var result = await _controller.CreateProduct(resource);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(expectedResource, createdResult.Value);
        }

        [Fact]
        public async Task GetAllProducts_ReturnsOk()
        {
            // Arrange
            var productList = new List<Product> { new Product(new CreateProductCommand("Prod A", 1, 5, _user.Id, null)) };
            _productQueryServiceMock.Setup(x => x.Handle(It.IsAny<GetProductsByUserIdQuery>()))
                .ReturnsAsync(productList);
            var expectedResources = productList.Select(ProductResourceFromEntityAssembler.ToResourceFromEntity);

            // Act
            var result = await _controller.GetAllProducts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResources, okResult.Value);
        }

        [Fact]
        public async Task GetProductById_ReturnsOk()
        {
            // Arrange
            var product = new Product(new CreateProductCommand("Prod A", 1, 5, _user.Id, null));
            _productQueryServiceMock.Setup(x => x.Handle(It.IsAny<GetProductByIdQuery>()))
                .ReturnsAsync(product);
            var expectedResource = ProductResourceFromEntityAssembler.ToResourceFromEntity(product);

            // Act
            var result = await _controller.GetProductById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResource, okResult.Value);
        }

        [Fact]
        public async Task DeleteProduct_ReturnsOk()
        {
            // Arrange
            var product = new Product(new CreateProductCommand("Prod A", 1, 5, _user.Id, null));
            _productQueryServiceMock.Setup(x => x.Handle(It.IsAny<GetProductByIdQuery>()))
                .ReturnsAsync(product);
            _productCommandServiceMock.Setup(x => x.Handle(It.IsAny<DeleteProductCommand>()))
                .ReturnsAsync(product);

            // Act
            var result = await _controller.DeleteProduct(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equivalent(new { message = "Product deleted successfully" }, okResult.Value);
        }
    }
}
