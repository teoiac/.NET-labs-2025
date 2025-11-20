using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Features.Product.Validators;
// ValidSKUAttribute Requirements:
// ● [ ] Inherit from ValidationAttribute and implement IClientModelValidator
//     ● [ ] Implement IsValid() method to validate SKU format (alphanumeric with
//     hyphens, 5-20 chars)
//     ● [ ] Remove spaces before validation
//     ● [ ] Implement AddValidation() for client-side validation
//     ● [ ] Add data attributes for client SKU validation
//     ProductCategoryAttribute Requirements:
//     ● [ ] Inherit from ValidationAttribute
//     ● [ ] Accept allowed categories in constructor
//     ● [ ] Generate error message with allowed categories list
//     ● [ ] Implement IsValid() method to check category against allowed list
//     PriceRangeAttribute Requirements:
//     ● [ ] Inherit from ValidationAttribute
//     ● [ ] Accept min and max price in constructor (as double, convert to decimal)
//     ● [ ] Generate error message with currency formatting
//     ● [ ] Implement IsValid() method for price range validation

public class ProductCategoryAttribute: ValidationAttribute
{
    private readonly HashSet<ProductCategory> _allowedCategories;

    public ProductCategoryAttribute(params ProductCategory[] allowedCategories)
    {
        _allowedCategories = allowedCategories.ToHashSet();
        ErrorMessage = $"Category must be one of the following: {string.Join(", ", _allowedCategories)}.";
    }

    public override bool IsValid(object? value)
    {
        if (value is not ProductCategory category)
        {
            return false;
        }

        return _allowedCategories.Contains(category);
    }
    
    
    
    
}