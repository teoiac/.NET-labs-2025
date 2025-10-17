using Microsoft.EntityFrameworkCore;
using tema_lab3.Persistence;

namespace tema_lab3.Features.Books;

public record PaginatedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);

public class GetBooksWithPaginationHandler(BookContext bookContext)
{
    private readonly BookContext _bookContext = bookContext;
    
    public async Task<IResult> Handle(int page = 1, int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;
        
        var totalCount = await _bookContext.Books.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        var books = await _bookContext.Books
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var result = new PaginatedResult<Book>(books, page, pageSize, totalCount, totalPages);
        return Results.Ok(result);
    }
}