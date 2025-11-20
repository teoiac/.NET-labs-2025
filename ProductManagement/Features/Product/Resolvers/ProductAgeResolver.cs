using AutoMapper;
using ProductManagement.Features.Product;
using ProductManagement.Features.Product.Dto;

namespace ProductManagement.Resolvers;

public class ProductAgeResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        var age = DateTime.UtcNow.Year - source.ReleaseDate.Year;
        if (source.ReleaseDate.Date > DateTime.UtcNow.AddYears(-age)) age--;
        return age switch
        {
            0 => "Less than a year old",
            1 => "1 year old",
            _ => $"{age} years old"
        };
    }
}