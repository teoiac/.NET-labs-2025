using tema_lab3.Persistence;

namespace tema_lab3.Features.Books;

public class GetBookByIdHandler(BookContext bookContext)
{
    private readonly BookContext _bookContext = bookContext;
    public async Task<IResult> Handle(Guid id)
    {
        var book = await _bookContext.Books.FindAsync(id);
        return book is not null ? Results.Ok(book) : Results.NotFound();
    }
    
    
}