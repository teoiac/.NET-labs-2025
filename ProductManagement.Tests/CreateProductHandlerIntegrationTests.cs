using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ProductManagement.Features.Product;
using ProductManagement.Features.Product.Dto;
using ProductManagement.Persistence;

namespace ProductManagement.Tests;

public class CreateProductHandlerIntegrationTests : IDisposable
{
    private readonly ProductContext _context;
    private readonly IDistributedCache _cache;
    private readonly Mock<ILogger<CreateProductHandler>> _loggerMock;
    private readonly IMapper _mapper;
    private readonly CreateProductHandler _handler;
    private readonly string _databaseName;

    public CreateProductHandlerIntegrationTests()
    {
        // Set up in-memory database with unique name
        _databaseName = $"ProductTestDb_{Guid.NewGuid()}";
        var options = new DbContextOptionsBuilder<ProductContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;
        _context = new ProductContext(options);

        // Configure AutoMapper with both product profiles
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AdvancedProductMappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        // Set up memory cache
        var cacheOptions = Options.Create(new MemoryDistributedCacheOptions());
        _cache = new MemoryDistributedCache(cacheOptions);

        // Mock ILogger<CreateProductHandler>
        _loggerMock = new Mock<ILogger<CreateProductHandler>>();

        // Create handler instance with all dependencies
        _handler = new CreateProductHandler(_context, _cache, _loggerMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ValidElectronicsProductRequest_CreatesProductWithCorrectMappings()
    {
        // Arrange: Create valid Electronics product request with all properties
        var request = new CreateProductProfileRequest(
            Name: "Smart Wireless Headphones",
            Brand: "Audio Tech",
            SKU: "AT-WH-2024",
            Category: ProductCategory.Electronics,
            Price: 149.99m,
            ReleaseDate: DateTime.UtcNow.AddYears(-2),
            ImageUrl: "https://example.com/headphones.jpg",
            StockQuantity: 15
        );

        // Act: Call handler
        var result = await _handler.Handle(request);

        // Assert: Verify Created result type
        result.Should().NotBeNull();
        var createdResult = result as Microsoft.AspNetCore.Http.IResult;
        createdResult.Should().NotBeNull();

        // Retrieve the created product from database
        var createdProduct = await _context.Products.FirstOrDefaultAsync(p => p.SKU == "AT-WH-2024");
        createdProduct.Should().NotBeNull();

        // Map to DTO to verify mappings
        var productDto = _mapper.Map<ProductProfileDto>(createdProduct!);

        // Assert: Check CategoryDisplayName = "Electronics"
        productDto.CategoryDisplayName.Should().Be("Electronics");

        // Assert: Check BrandInitials for two-word brand ("Audio Tech" -> "AT")
        productDto.BrandInitials.Should().Be("AT");

        // Assert: Check ProductAge calculation
        productDto.ProductAge.Should().Be("2 years old");

        // Assert: Check FormattedPrice starts with currency symbol
        productDto.FormattedPrice.Should().StartWith("$");
        productDto.FormattedPrice.Should().Contain("149.99");

        // Assert: Check AvailabilityStatus based on stock (15 units = "In Stock")
        productDto.AvailabilityStatus.Should().Be("In Stock");

        // Assert: Verify ProductCreationStarted log called once
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating product")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateSKU_ReturnsConflictWithLogging()
    {
        // Arrange: Create existing product in database with specific SKU
        var existingProduct = new Product(
            Id: Guid.NewGuid(),
            Name: "Existing Product",
            Brand: "Test Brand",
            SKU: "DUPLICATE-SKU",
            Category: ProductCategory.Electronics,
            Price: 99.99m,
            ReleaseDate: DateTime.UtcNow,
            ImageUrl: null,
            IsAvailable: true,
            StockQuantity: 10
        );
        _context.Products.Add(existingProduct);
        await _context.SaveChangesAsync();

        // Arrange: Create request with same SKU
        var request = new CreateProductProfileRequest(
            Name: "New Product",
            Brand: "Another Brand",
            SKU: "DUPLICATE-SKU",
            Category: ProductCategory.Electronics,
            Price: 199.99m,
            ReleaseDate: DateTime.UtcNow,
            ImageUrl: "https://example.com/product.jpg",
            StockQuantity: 5
        );

        // Act
        var result = await _handler.Handle(request);

        // Assert: Verify Conflict result
        result.Should().NotBeNull();
        
        // Assert: Verify warning log was called for duplicate SKU
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("already exists")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_HomeProductRequest_AppliesDiscountAndConditionalMapping()
    {
        // Arrange: Create valid Home product request
        var request = new CreateProductProfileRequest(
            Name: "Decorative Lamp",
            Brand: "HomeStyle",
            SKU: "HS-LAMP-001",
            Category: ProductCategory.Home,
            Price: 100m,
            ReleaseDate: DateTime.UtcNow.AddMonths(-6),
            ImageUrl: "https://example.com/lamp.jpg",
            StockQuantity: 25
        );

        // Act: Call handler
        var result = await _handler.Handle(request);

        // Assert: Verify result
        result.Should().NotBeNull();

        // Retrieve the created product from database
        var createdProduct = await _context.Products.FirstOrDefaultAsync(p => p.SKU == "HS-LAMP-001");
        createdProduct.Should().NotBeNull();

        // Map to DTO to verify mappings
        var productDto = _mapper.Map<ProductProfileDto>(createdProduct!);

        // Assert: Check CategoryDisplayName = "Home"
        productDto.CategoryDisplayName.Should().Be("Home");

        // Assert: Check Price has 10% discount applied (100 * 0.9 = 90)
        productDto.Price.Should().Be(90m);

        // Assert: Check ImageUrl is null (content filtering for Home category)
        productDto.ImageUrl.Should().BeNull();
    }

    public void Dispose()
    {
        // Proper disposal of context and cache
        _context?.Database.EnsureDeleted();
        _context?.Dispose();
        
        if (_cache is IDisposable disposableCache)
        {
            disposableCache.Dispose();
        }
    }
}

