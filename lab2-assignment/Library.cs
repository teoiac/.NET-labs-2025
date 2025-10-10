namespace lab2_assignment;

public class Library
{
    public List<Book> Books { get; set; }
    public List<Borrower> AllBorrowers { get; set; }
    public List<Librarian> AllLibrarians { get; set; }

    public List<Book> Wishlist { get; set; }

    public Library(List<Book> books, List<Borrower> allBorrowers, List<Librarian> allLibrarians)
    {
        Books = books;
        AllBorrowers = allBorrowers;
        AllLibrarians = allLibrarians;
        Wishlist = new List<Book>();
    }

    public Book SearchBookByTitle(string title)
    {
        return Books.FirstOrDefault(book => book.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
    }

    public bool AddBookToWishList(string title)
    {
        var bookSearched = SearchBookByTitle(title);
        if (bookSearched != null)
        {
            Wishlist.Add(bookSearched);
            return true;
        }

        return false;
    }

    public void DisplayWishlist()
    {
        if (Wishlist == null || Wishlist.Count == 0)
        {
            Console.WriteLine("Your wishlist is empty.");
            return;
        }

        Console.WriteLine("Wishlist:");
        foreach (var book in Wishlist)
        {
            Console.WriteLine($"- {book.Title} by {book.Author}");
        }
    }
    
    public void FilterBooksByYear(int year)
    {
        var filteredBooks = Books.Where(book => book.YearPublished >= year).ToList();
        if (filteredBooks.Count == 0)
        {
            Console.WriteLine($"No books found published in or after {year}.");
            return;
        }

        Console.WriteLine($"Books published in or after {year}:");
        foreach (var book in filteredBooks)
        {
            Console.WriteLine($"- {book.Title} by {book.Author}, Year: {book.YearPublished}");
        }
    }


    public void DisplayObjectInfo(object obj)
    {
        switch (obj)
        {
            case Book book:
                Console.WriteLine($"Book: {book.Title}, Year: {book.YearPublished}");
                break;
            case Borrower borrower:
                Console.WriteLine($"Borrower: {borrower.Name}, Borrowed Books: {borrower.BorrowedBooks.Count}");
                break;
            default:
                Console.WriteLine("Unknown type");
                break;
        }
    }

    public Borrower SearchBorrowerByName(string name)
    {
        return AllBorrowers.FirstOrDefault(borrower => borrower.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}