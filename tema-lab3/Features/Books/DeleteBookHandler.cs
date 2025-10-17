using tema_lab3.Persistence;

namespace tema_lab3.Features.Books;

public class DeleteBookHandler(BookContext bookContext)
{
    private readonly BookContext _bookContext = bookContext;
    
    public async Task<IResult> Handle(Guid id)
    {
        var book = await _bookContext.Books.FindAsync(id);
        if (book is null)
            return Results.NotFound();
        
        _bookContext.Books.Remove(book);
        await _bookContext.SaveChangesAsync();
        return Results.NoContent();
    }
}