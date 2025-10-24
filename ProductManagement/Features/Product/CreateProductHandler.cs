using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ProductManagement.Features.Product.Dto;
using ProductManagement.Persistence;

namespace ProductManagement.Features.Product;

public class CreateProductHandler(
    ProductContext productContext,
    IDistributedCache cache,
    ILogger<CreateProductHandler> logger)
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

            var product = new Product(
                Guid.NewGuid(),
                request.Name,
                request.Brand,
                request.SKU,
                request.Category,
                request.Price,
                request.ReleaseDate,
                request.ImageUrl,
                request.IsAvailable,
                request.StockQuantity
            );

            productContext.Products.Add(product);
            await productContext.SaveChangesAsync();

            await cache.RemoveAsync("all_products");

            logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);

            var productDto = new ProductProfileDto(
                product.Id,
                product.Name,
                product.Brand,
                product.SKU,
                product.Category.ToString(),
                product.Price,
                product.Price.ToString("C"),
                product.ReleaseDate,
                DateTime.UtcNow,
                product.ImageUrl,
                product.IsAvailable,
                product.StockQuantity,
                CalculateProductAge(product.ReleaseDate),
                GetBrandInitials(product.Brand),
                product.IsAvailable ? "In Stock" : "Out of Stock"
            );

            return Results.Created($"/products/{product.Id}", productDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating product with SKU: {SKU}", request.SKU);
            return Results.Problem("An error occurred while creating the product");
        }
        
    }
    
    private static string CalculateProductAge(DateTime releaseDate)
    {
        var age = DateTime.UtcNow - releaseDate;
        return age.TotalDays switch
        {
            < 1 => "New",
            < 30 => $"{(int)age.TotalDays} days old",
            < 365 => $"{(int)(age.TotalDays / 30)} months old",
            _ => $"{(int)(age.TotalDays / 365)} years old"
        };
    }
    
    private static string GetBrandInitials(string brand)
    {
        var words = brand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(words.Select(w => char.ToUpper(w[0])));
    }
}