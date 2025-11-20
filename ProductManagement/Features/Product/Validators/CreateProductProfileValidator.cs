using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductManagement.Persistence;
using System.Text.RegularExpressions;

namespace ProductManagement.Features.Product.Validators;

public class CreateProductProfileValidator : AbstractValidator<CreateProductProfileRequest>
{
    private readonly ProductContext _context;
    private readonly ILogger<CreateProductProfileValidator> _logger;
    private readonly List<string> _inappropriateWords = new() { "badword", "profanity", "inappropriate" };
    private readonly List<string> _homeRestrictedWords = new() { "restricted", "sensitive" };
    private readonly List<string> _technologyKeywords = new() { "tech", "digital", "electronic", "smart", "wireless", "bluetooth", "wifi", "computer", "processor", "memory", "display", "screen" };
    private readonly List<string> _homeInappropriateWords = new() { "violent", "weapon", "dangerous", "hazard" };

    public CreateProductProfileValidator(ProductContext context, ILogger<CreateProductProfileValidator> logger)
    {
        _context = context;
        _logger = logger;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .Length(1, 200)
            .Must(BeValidName).WithMessage("Product name contains inappropriate content.")
            .MustAsync(BeUniqueName).WithMessage("Product name must be unique for the same brand.");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Brand is required.")
            .Length(2, 100)
            .Must(BeValidBrandName).WithMessage("Brand contains invalid characters.");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required.")
            .Must(BeValidSKU).WithMessage("SKU must be 5-20 alphanumeric characters or hyphens.")
            .MustAsync(BeUniqueSKU).WithMessage("SKU must be unique.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid product category.");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .LessThan(10000);

        RuleFor(x => x.ReleaseDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Release date cannot be in the future.")
            .Must(date => date.Year >= 1900).WithMessage("Release date cannot be before the year 1900.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(100000);

        RuleFor(x => x.ImageUrl)
            .Must(BeValidImageUrl).When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("Image URL must be a valid HTTP/HTTPS URL ending with a valid image extension.");

        // Electronics Product Conditions
        When(x => x.Category == ProductCategory.Electronics, () =>
        {
            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(50.00m)
                .WithMessage("Electronics products must have a minimum price of $50.00.");

            RuleFor(x => x.Name)
                .Must(ContainTechnologyKeywords)
                .WithMessage("Electronics products must contain technology-related keywords in the name.");

            RuleFor(x => x.ReleaseDate)
                .Must(date => (DateTime.UtcNow - date).TotalDays <= 365 * 5)
                .WithMessage("Electronics products must be released within the last 5 years.");
        });

        // Home Product Conditions
        When(x => x.Category == ProductCategory.Home, () =>
        {
            RuleFor(x => x.Price)
                .LessThanOrEqualTo(200.00m)
                .WithMessage("Home products must have a maximum price of $200.00.");

            RuleFor(x => x.Name)
                .Must(BeAppropriateForHome)
                .WithMessage("Home product name contains inappropriate content.");
        });

        // Clothing Product Conditions
        When(x => x.Category == ProductCategory.Clothing, () =>
        {
            RuleFor(x => x.Brand)
                .MinimumLength(3)
                .WithMessage("Clothing products must have a brand name with at least 3 characters.");
        });

        // Cross-Field Validation: Expensive products must have limited stock
        RuleFor(x => x)
            .Must(x => x.Price <= 100m || x.StockQuantity <= 20)
            .WithMessage("Products priced over $100 must have stock quantity of 20 units or less.");

        RuleFor(x => x)
            .MustAsync(PassBusinessRules).WithMessage("Product failed business rule validation.");
    }

    private bool BeValidName(string name) =>
        !_inappropriateWords.Any(word => name.Contains(word, StringComparison.OrdinalIgnoreCase));

    private async Task<bool> BeUniqueName(CreateProductProfileRequest request, string name, CancellationToken cancellationToken)
    {
        var isUnique = !await _context.Products.AnyAsync(p => p.Name == name && p.Brand == request.Brand, cancellationToken);
        if (!isUnique)
        {
            _logger.LogWarning("Validation failed: Product name '{Name}' already exists for brand '{Brand}'.", name, request.Brand);
        }
        return isUnique;
    }

    private bool BeValidBrandName(string brand) =>
        Regex.IsMatch(brand, "^[a-zA-Z0-9\\s\\-'\\.]+$");

    private bool BeValidSKU(string sku) =>
        Regex.IsMatch(sku, "^[a-zA-Z0-9\\-]{5,20}$");

    private async Task<bool> BeUniqueSKU(string sku, CancellationToken cancellationToken)
    {
        var isUnique = !await _context.Products.AnyAsync(p => p.SKU == sku, cancellationToken);
        if (!isUnique)
        {
            _logger.LogWarning("Validation failed: SKU '{SKU}' already exists.", sku);
        }
        return isUnique;
    }

    private bool BeValidImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult)) return false;
        var extensionRegex = new Regex(@"\.(jpg|jpeg|png|gif|webp)$", RegexOptions.IgnoreCase);
        return (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps) && extensionRegex.IsMatch(url);
    }

    private bool ContainTechnologyKeywords(string name) =>
        _technologyKeywords.Any(keyword => name.Contains(keyword, StringComparison.OrdinalIgnoreCase));

    private bool BeAppropriateForHome(string name) =>
        !_homeInappropriateWords.Any(word => name.Contains(word, StringComparison.OrdinalIgnoreCase));

    private async Task<bool> PassBusinessRules(CreateProductProfileRequest request, CancellationToken cancellationToken)
    {
        // Rule 1: Daily product addition limit
        var today = DateTime.UtcNow.Date;
        var productsToday = await _context.Products.CountAsync(p => p.ReleaseDate.Date == today, cancellationToken);
        if (productsToday >= 500)
        {
            _logger.LogWarning("Business Rule Failed: Daily product addition limit of 500 reached.");
            return false;
        }

        // Rule 2: Electronics minimum price
        if (request.Category == ProductCategory.Electronics && request.Price < 50.00m)
        {
            _logger.LogWarning("Business Rule Failed: Electronics product '{Name}' has price {Price}, which is below the $50 minimum.", request.Name, request.Price);
            return false;
        }

        // Rule 3: Home product content restrictions
        if (request.Category == ProductCategory.Home && _homeRestrictedWords.Any(word => request.Name.Contains(word, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("Business Rule Failed: Home product '{Name}' contains restricted words.", request.Name);
            return false;
        }

        // Rule 4: High-value product stock limit
        if (request.Price > 500m && request.StockQuantity > 10)
        {
            _logger.LogWarning("Business Rule Failed: High-value product '{Name}' (Price: {Price}) has stock {StockQuantity}, exceeding the limit of 10.", request.Name, request.Price, request.StockQuantity);
            return false;
        }

        _logger.LogInformation("All business rules passed for product '{Name}'.", request.Name);
        return true;
    }
}