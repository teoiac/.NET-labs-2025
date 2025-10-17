using Microsoft.EntityFrameworkCore;
using tema_lab3.Persistence;

namespace tema_lab3.Features.Books;

public class GetAllBooksHandler(BookContext bookContext)
{
    private readonly BookContext _bookContext = bookContext;
    
    public async Task<IResult> Handle()
    {
        var books = await _bookContext.Books.ToListAsync();
        return Results.Ok(books);
    }
}