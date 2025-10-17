using FluentValidation;
using Microsoft.EntityFrameworkCore;
using tema_lab3.Features.Books;
using tema_lab3.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<tema_lab3.Persistence.BookContext>(options =>
    options.UseSqlite("Data Source = books.db"));
builder.Services.AddScoped<CreateBookHandler>();
builder.Services.AddScoped<UpdateBookHandler>();
builder.Services.AddScoped<DeleteBookHandler>();
builder.Services.AddScoped<GetAllBooksHandler>();
builder.Services.AddScoped<GetBookByIdHandler>();
builder.Services.AddScoped<GetBooksWithPaginationHandler>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateBookValidator>();



var app = builder.Build();


//ensure the database is created at runtime
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<tema_lab3.Persistence.BookContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/books", async (CreateBookRequest request, CreateBookHandler handler, 
    IValidator<CreateBookRequest> validator) =>
{
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
        return Results.ValidationProblem(validationResult.ToDictionary());
    
    return await handler.Handle(request);
});


app.MapGet("/books/{id:guid}", async (Guid id, GetBookByIdHandler handler) =>
    await handler.Handle(id));

app.MapGet("/books", async (GetBooksWithPaginationHandler handler, int? page, int? pageSize) =>
    await handler.Handle(page ?? 1, pageSize ?? 10));

app.MapPut("/books/{id:guid}", async (Guid id, UpdateBookRequest request, 
    UpdateBookHandler handler, IValidator<UpdateBookRequest> validator) =>
{
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
        return Results.ValidationProblem(validationResult.ToDictionary());
    
    return await handler.Handle(id, request);
});

app.MapDelete("/books/{id:guid}", async (Guid id, DeleteBookHandler handler) =>
    await handler.Handle(id));

app.Run();

