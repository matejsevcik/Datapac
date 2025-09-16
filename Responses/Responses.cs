using Datapac.Models;

namespace Datapac.Responses;

public record LoanDetailResponse(
    int LoanId,
    int BookId,
    int UserId,
    string UserEmail,
    string BookTitle,
    DateOnly StartDate,
    DateOnly ExpirationDate,
    DateOnly? ReturnDate );
    
public record LoanReturnResponse(
    int LoanId,
    int BookId,
    int UserId,
    string UserName,
    string UserEmail,
    string BookTitle,
    DateOnly StartDate,
    DateOnly ExpirationDate,
    DateOnly? ReturnDate);
    
public record ErrorResponse(
    string Message, 
    IDictionary<string, string[]>? Errors);