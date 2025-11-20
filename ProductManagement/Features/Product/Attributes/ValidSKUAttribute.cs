using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProductManagement.Features.Product.Validators;

public class ValidSKUAttribute : ValidationAttribute, IClientModelValidator
{
    public override bool IsValid(object? value)
    {
        if (value is not string sku)
        {
            return false;
        }

        // SKU must be alphanumeric and exactly 8 characters long
        return sku.Length == 8 && sku.All(char.IsLetterOrDigit);
    }
    
    public void AddValidation(ClientModelValidationContext context)
    {
        context.Attributes.Add("data-val", "true");
        context.Attributes.Add("data-val-validsku", "The SKU must be alphanumeric and exactly 8 characters long.");
    }
    
    
}