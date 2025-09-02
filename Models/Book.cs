namespace Datapac.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public int Available { get; set; }
    public int TotalCopies { get; set; }
}