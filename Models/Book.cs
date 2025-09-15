using Datapac.Interfaces;

namespace Datapac.Models;

public class Book : ISoftDelete
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public int Available { get; set; }
    public int TotalCopies { get; set; }
    public bool IsDeleted { get; set; }
    public DateOnly? DeletedAt { get; set; }
}