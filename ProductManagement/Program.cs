using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Features.Product;
using ProductManagement.Features.Product.Validators;
using ProductManagement.Middleware;
using ProductManagement.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Database Context
builder.Services.AddDbContext<ProductContext>(options =>
    options.UseSqlite("Data Source=products.db"));

// Add Distributed Cache
builder.Services.AddDistributedMemoryCache();

// Register AutoMapper with both profiles
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Register CreateProductProfileValidator explicitly as scoped
builder.Services.AddScoped<IValidator<CreateProductProfileRequest>, CreateProductProfileValidator>();

// Register CreateProductHandler
builder.Services.AddScoped<CreateProductHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Add CorrelationMiddleware to pipeline
app.UseCorrelationId();

// Product endpoints
app.MapPost("/products", async (CreateProductProfileRequest request, CreateProductHandler handler) =>
    {
        return await handler.Handle(request);
    })
    .WithName("CreateProduct")
    .WithDescription("Creates a new product with advanced validation and mapping")
    .WithSummary("Create a new product")
    .WithTags("Products")
    .Produces<ProductManagement.Features.Product.Dto.ProductProfileDto>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status409Conflict);

app.Run();
