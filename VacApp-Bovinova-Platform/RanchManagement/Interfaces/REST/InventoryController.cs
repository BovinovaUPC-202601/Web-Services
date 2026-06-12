using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;
using VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;
using VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Transform;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;

namespace VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST;

[Authorize]
[ApiController]
[Route("/api/v1/inventory")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Inventory")]
public class InventoryController(
    ICategoryCommandService categoryCommandService,
    ICategoryQueryService categoryQueryService,
    IProductCommandService productCommandService,
    IProductQueryService productQueryService,
    IStaffAccessService staffAccessService) : ControllerBase
{
    private ObjectResult ForbiddenEdit() =>
        StatusCode(StatusCodes.Status403Forbidden,
            new { message = "Read-only staff cannot create, edit or delete." });

    #region Category Endpoints

    [HttpPost("categories")]
    [SwaggerOperation(
        Summary = "Create a new category",
        Description = "Creates a new category associated with the effective ranch owner.",
        OperationId = "CreateCategory"
    )]
    [SwaggerResponse(StatusCodes.Status201Created, "Category successfully created", typeof(CategoryResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryResource resource)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        if (!await staffAccessService.CanEditAsync(user)) return ForbiddenEdit();
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var command = CreateCategoryCommandFromResourceAssembler.ToCommandFromResource(resource, effectiveUserId);
        var result = await categoryCommandService.Handle(command);
        if (result is null) return BadRequest();
        return CreatedAtAction(nameof(GetCategoryById), new { id = result.Id },
            CategoryResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    [HttpGet("categories")]
    [SwaggerOperation(
        Summary = "Get all categories",
        Description = "Get all categories by user ID",
        OperationId = "GetAllCategories")]
    [SwaggerResponse(StatusCodes.Status200OK, "The list of categories were found", typeof(IEnumerable<CategoryResource>))]
    public async Task<IActionResult> GetAllCategories()
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var categories = await categoryQueryService.Handle(new GetCategoriesByUserIdQuery(effectiveUserId));
        var categoryResources = categories.Select(CategoryResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(categoryResources);
    }

    [HttpGet("categories/{id}")]
    [SwaggerOperation(
        Summary = "Get category by ID",
        Description = "Get category by ID",
        OperationId = "GetCategoryById")]
    public async Task<ActionResult> GetCategoryById(int id)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var getCategoryById = new GetCategoryByIdQuery(id);
        var result = await categoryQueryService.Handle(getCategoryById);
        // NotFound (not Forbidden) when the category belongs to another ranch.
        if (result is null || result.UserId != effectiveUserId) return NotFound();
        var resources = CategoryResourceFromEntityAssembler.ToResourceFromEntity(result);
        return Ok(resources);
    }

    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryResource resource)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        if (!await staffAccessService.CanEditAsync(user)) return ForbiddenEdit();
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var existing = await categoryQueryService.Handle(new GetCategoryByIdQuery(id));
        if (existing is null || existing.UserId != effectiveUserId)
            return NotFound(new { message = "Category not found" });

        var command = UpdateCategoryCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await categoryCommandService.Handle(command);
        if (result is null) return NotFound(new { message = "Category not found" });
        return Ok(CategoryResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    [HttpDelete("categories/{id}")]
    [SwaggerOperation(
        Summary = "Delete category",
        Description = "Delete category by ID",
        OperationId = "DeleteCategory")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        if (!await staffAccessService.CanEditAsync(user)) return ForbiddenEdit();
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var existing = await categoryQueryService.Handle(new GetCategoryByIdQuery(id));
        if (existing is null || existing.UserId != effectiveUserId)
            return NotFound(new { message = "Category not found" });

        var command = new DeleteCategoryCommand(id);
        var result = await categoryCommandService.Handle(command);
        if (result is null)
            return NotFound(new { message = "Category not found" });
        return Ok(new { message = "Category deleted successfully" });
    }

    #endregion

    #region Product Endpoints

    [HttpPost("products")]
    [SwaggerOperation(
        Summary = "Create a new product",
        Description = "Creates a new product associated with the effective ranch owner.",
        OperationId = "CreateProduct"
    )]
    [SwaggerResponse(StatusCodes.Status201Created, "Product successfully created", typeof(ProductResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductResource resource)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        if (!await staffAccessService.CanEditAsync(user)) return ForbiddenEdit();
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var command = CreateProductCommandFromResourceAssembler.ToCommandFromResource(resource, effectiveUserId);
        var result = await productCommandService.Handle(command);
        if (result is null) return BadRequest();
        return CreatedAtAction(nameof(GetProductById), new { id = result.Id },
            ProductResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    [HttpGet("products")]
    [SwaggerOperation(
        Summary = "Get all products",
        Description = "Get all products by user ID",
        OperationId = "GetAllProducts")]
    [SwaggerResponse(StatusCodes.Status200OK, "The list of products were found", typeof(IEnumerable<ProductResource>))]
    public async Task<IActionResult> GetAllProducts()
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var products = await productQueryService.Handle(new GetProductsByUserIdQuery(effectiveUserId));
        var productResources = products.Select(ProductResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(productResources);
    }

    [HttpGet("products/{id}")]
    [SwaggerOperation(
        Summary = "Get product by ID",
        Description = "Get product by ID",
        OperationId = "GetProductById")]
    public async Task<ActionResult> GetProductById(int id)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var getProductById = new GetProductByIdQuery(id);
        var result = await productQueryService.Handle(getProductById);
        // NotFound (not Forbidden) when the product belongs to another ranch.
        if (result is null || result.UserId != effectiveUserId) return NotFound();
        var resources = ProductResourceFromEntityAssembler.ToResourceFromEntity(result);
        return Ok(resources);
    }

    [HttpPut("products/{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductResource resource)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        if (!await staffAccessService.CanEditAsync(user)) return ForbiddenEdit();
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var existing = await productQueryService.Handle(new GetProductByIdQuery(id));
        if (existing is null || existing.UserId != effectiveUserId)
            return NotFound(new { message = "Product not found" });

        var command = UpdateProductCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await productCommandService.Handle(command);
        if (result is null) return NotFound(new { message = "Product not found" });
        return Ok(ProductResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    [HttpDelete("products/{id}")]
    [SwaggerOperation(
        Summary = "Delete product",
        Description = "Delete product by ID",
        OperationId = "DeleteProduct")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null)
            return Unauthorized("User not found in context.");

        if (!await staffAccessService.CanEditAsync(user)) return ForbiddenEdit();
        var effectiveUserId = await staffAccessService.GetEffectiveUserIdAsync(user);

        var existing = await productQueryService.Handle(new GetProductByIdQuery(id));
        if (existing is null || existing.UserId != effectiveUserId)
            return NotFound(new { message = "Product not found" });

        var command = new DeleteProductCommand(id);
        var result = await productCommandService.Handle(command);
        if (result is null)
            return NotFound(new { message = "Product not found" });
        return Ok(new { message = "Product deleted successfully" });
    }

    #endregion
}
