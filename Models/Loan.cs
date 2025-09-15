namespace Datapac.Models;

public class Loan
{
    public int Id { get; set; }
    public required Book Book { get; set; }
    public required User User { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public DateOnly? ReturnDate { get; set; }
}