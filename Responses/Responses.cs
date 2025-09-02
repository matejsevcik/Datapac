namespace Datapac.Responses;

public class Responses
{
    public record LoanReturnResponse(
        int LoanId,
        int BookId,
        int UserId,
        string UserName,
        string UserEmail,
        string BookTitle,
        DateOnly? ReturnDate);
}