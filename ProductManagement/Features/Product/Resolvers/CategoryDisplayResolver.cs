using AutoMapper;
using ProductManagement.Features.Product;
using ProductManagement.Features.Product.Dto;

namespace ProductManagement.Resolvers;

public class CategoryDisplayResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        return source.Category.ToString().Replace("_", " ");
    }
}