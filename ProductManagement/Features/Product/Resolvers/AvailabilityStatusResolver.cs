using AutoMapper;
using ProductManagement.Features.Product;
using ProductManagement.Features.Product.Dto;

namespace ProductManagement.Resolvers;

public class AvailabilityStatusResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        if (!source.IsAvailable) return "Out of Stock";
        return source.StockQuantity switch
        {
            <= 0 => "Out of Stock",
            <= 10 => "Low Stock",
            _ => "In Stock"
        };
    }
}