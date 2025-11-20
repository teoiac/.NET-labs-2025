using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ProductManagement.Features.Product.Dto;
using ProductManagement.Persistence;

namespace ProductManagement.Features.Product;

public class CreateProductHandler(
    ProductContext productContext,
    IDistributedCache cache,
    ILogger<CreateProductHandler> logger,
    IMapper mapper)
{
    public async Task<IResult> Handle(CreateProductProfileRequest request)
    {
        try
        {
            logger.LogInformation(
                "Creating product with Name: {Name}, Brand: {Brand}, Category: {Category}, SKU: {SKU}",
                request.Name, request.Brand, request.Category, request.SKU);

            var existingSku = await productContext.Products
                .AnyAsync(p => p.SKU == request.SKU);

            if (existingSku)
            {
                logger.LogWarning("Product with SKU: {SKU} already exists", request.SKU);
                return Results.Conflict($"Product with SKU: {request.SKU} already exists");
            }

            var product = mapper.Map<Product>(request);

            productContext.Products.Add(product);
            await productContext.SaveChangesAsync();

            await cache.RemoveAsync("all_products");

            logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);

            var productDto = mapper.Map<ProductProfileDto>(product);

            return Results.Created($"/products/{product.Id}", productDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating product with SKU: {SKU}", request.SKU);
            return Results.Problem("An error occurred while creating the product");
        }
    }
}