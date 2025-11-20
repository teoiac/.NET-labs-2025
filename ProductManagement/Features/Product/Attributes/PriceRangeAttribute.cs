using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Features.Product.Validators;


// ● [ ] Inherit from ValidationAttribute
//     ● [ ] Accept min and max price in constructor (as double, convert to decimal)
//     ● [ ] Generate error message with currency formatting
//     ● [ ] Implement IsValid() method for price range validation

public class PriceRangeAttribute : ValidationAttribute
{
    private readonly decimal _minPrice;
    private readonly decimal _maxPrice;

    public PriceRangeAttribute(double minPrice, double maxPrice)
    {
        _minPrice = Convert.ToDecimal(minPrice);
        _maxPrice = Convert.ToDecimal(maxPrice);
        ErrorMessage = $"Price must be between {_minPrice:C} and {_maxPrice:C}.";
    }

    public override bool IsValid(object? value)
    {
        if (value is not decimal price)
        {
            return false;
        }

        return price >= _minPrice && price <= _maxPrice;
    }
    
}