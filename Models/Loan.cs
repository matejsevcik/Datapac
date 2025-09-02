namespace Datapac.Models;

public class Loan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int UserId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public DateOnly? ReturnDate { get; set; }
}