using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;
using VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.RanchManagement.Application.Internal.CommandServices;

public class ProductCommandService(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
    ) : IProductCommandService
{
    public async Task<Product?> Handle(CreateProductCommand command)
    {
        var product = new Product(command);

        try
        {
            await productRepository.AddAsync(product);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return product;
    }

    public async Task<Product?> Handle(UpdateProductCommand command)
    {
        var product = await productRepository.FindByIdAsync(command.Id);
        if (product == null) throw new NotFoundException($"Producto con ID '{command.Id}' no encontrado.");

        product.Update(command);

        try
        {
            productRepository.Update(product);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return product;
    }

    public async Task<Product?> Handle(DeleteProductCommand command)
    {
        var product = await productRepository.FindByIdAsync(command.Id);
        if (product == null) throw new NotFoundException($"Producto con ID '{command.Id}' no encontrado.");

        try
        {
            productRepository.Remove(product);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return product;
    }
}
