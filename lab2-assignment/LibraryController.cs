namespace lab2_assignment;

public class LibraryController
{
    Library library;
    public LibraryController(Library library)
    {
        this.library = library;
    }
    
    public void HandleUserInput()
    {
        while (true)
        {
            Console.WriteLine("What would you like to search for? (book/borrower/exit):");
            string choice = Console.ReadLine();
            if (choice.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            if (choice.Equals("book", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Enter a book title to search:");
                string userInput = Console.ReadLine();
                var book = library.SearchBookByTitle(userInput);
                if (book != null)
                {
                    Console.WriteLine($"Book found: Title: {book.Title}, Author: {book.Author}, Year Published: {book.YearPublished}");

                    Console.WriteLine("Would you like to add this book to your wishlist? (yes/no):");
                    string addChoice = Console.ReadLine();

                    if (addChoice.Equals("yes", StringComparison.OrdinalIgnoreCase))
                    {
                        bool added = library.AddBookToWishList(book.Title);
                        if (added)
                            Console.WriteLine($"'{book.Title}' has been added to your wishlist.");
                        else
                            Console.WriteLine("Failed to add the book to your wishlist.");
                    }
                }
                else
                {
                    Console.WriteLine("Book not found.");
                }
                Console.WriteLine("Your current wishlist:");
                library.DisplayWishlist();
            }
            else if (choice.Equals("borrower", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Enter borrower name to search:");
                string userInput = Console.ReadLine();
                var borrower = library.SearchBorrowerByName(userInput);
                if (borrower != null)
                {
                    library.DisplayObjectInfo(borrower);
                }
                else
                {
                    Console.WriteLine("Borrower not found.");
                }
            }
            else
            {
                Console.WriteLine("Unknown option.");
            }
        }
    }

    
}