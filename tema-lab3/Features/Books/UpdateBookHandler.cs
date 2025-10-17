using tema_lab3.Persistence;

namespace tema_lab3.Features.Books;

public class UpdateBookHandler(BookContext bookContext)
{
    private readonly BookContext _bookContext = bookContext;
    
    public async Task<IResult> Handle(Guid id, UpdateBookRequest request)
    {
        var book = await _bookContext.Books.FindAsync(id);
        if (book is null)
            return Results.NotFound();
        
        var updatedBook = book with 
        { 
            Title = request.Title, 
            Author = request.Author, 
            Year = request.Year 
        };
        
        _bookContext.Books.Update(updatedBook);
        await _bookContext.SaveChangesAsync();
        return Results.Ok(updatedBook);
    }
}