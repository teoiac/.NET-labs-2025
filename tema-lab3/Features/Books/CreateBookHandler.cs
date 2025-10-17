using tema_lab3.Persistence;

namespace tema_lab3.Features.Books;

public class CreateBookHandler(BookContext bookContext)
{
    private readonly BookContext _bookContext = bookContext;
    
    public async Task<IResult> Handle(CreateBookRequest request)
    {
        var book = new Book(Guid.NewGuid(), request.Title, request.Author, request.Year);
        _bookContext.Books.Add(book);
        await _bookContext.SaveChangesAsync();
        return Results.Created($"/books/{book.Id}", book);
    }
}