namespace lab2_assignment;

public record Book(string Title, string Author, int YearPublished)
{
    public bool IsBorrowed { get; set; }
}