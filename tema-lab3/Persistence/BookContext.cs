using Microsoft.EntityFrameworkCore;
using tema_lab3.Features.Books;

namespace tema_lab3.Persistence;

public class BookContext(DbContextOptions<BookContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; }
}