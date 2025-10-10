// See https://aka.ms/new-console-template for more information

using lab2_assignment;


var sampleBooks = new List<Book>
{
    new Book("1984", "George Orwell", 1949),
    new Book("To Kill a Mockingbird", "Harper Lee", 1960),
    new Book("The Great Gatsby", "F. Scott Fitzgerald", 1925),
    new Book("Pride and Prejudice", "Jane Austen", 1813)
};
var sampleBorrowers = new List<Borrower>
{
    new Borrower(1, "John Doe", new List<Book>()),
    new Borrower(2, "Jane Smith", new List<Book>())
};

var sampleLibrarians = new List<Librarian>
{
    new Librarian { Name = "Emily Johnson", Email = "emily@example.com", LibrarySection = "Fiction" },
    new Librarian { Name = "Michael Brown", Email = "michael@example.com", LibrarySection = "Horror" }
};

var library = new Library(sampleBooks, sampleBorrowers, sampleLibrarians);

var libraryController = new LibraryController(library);
libraryController.HandleUserInput();


