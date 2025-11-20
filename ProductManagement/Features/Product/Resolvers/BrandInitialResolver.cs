using AutoMapper;
using ProductManagement.Features.Product;
using ProductManagement.Features.Product.Dto;

namespace ProductManagement.Resolvers;

public class BrandInitialsResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        if (string.IsNullOrWhiteSpace(source.Brand)) return string.Empty;
        var parts = source.Brand.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 1)
        {
            return $"{parts[0][0]}{parts[1][0]}".ToUpper();
        }
        return parts[0].Length > 1 ? parts[0].Substring(0, 2).ToUpper() : parts[0].ToUpper();
    }
}