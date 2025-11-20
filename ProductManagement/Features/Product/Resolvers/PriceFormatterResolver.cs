using System.Globalization;
using AutoMapper;
using ProductManagement.Features.Product;
using ProductManagement.Features.Product.Dto;

namespace ProductManagement.Resolvers;

public class PriceFormatterResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        return source.Price.ToString("C", new CultureInfo("en-US"));
    }
}