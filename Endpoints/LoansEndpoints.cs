using Datapac.Models;
using Datapac.Requests;
using Datapac.Responses;
using Datapac.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Datapac.Endpoints;

public static class LoansEndpoints
{
    public static RouteGroupBuilder MapLoansEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllLoans);
        group.MapPost("/", CreateLoan);
        group.MapPut("/{id}/return", ReturnLoan);
        
        return group;
    }

    public static async Task<Ok<List<LoanDetailResponse>>> GetAllLoans([FromServices] LoansContext context)
    {
        var allLoans = await context.Loans
            .Include(l => l.Book)
            .Include(l => l.User)
            .IgnoreQueryFilters()
            .ToListAsync();

        var response = allLoans.Select(l => new LoanDetailResponse(
            LoanId: l.Id,
            UserId: l.User.Id,
            BookId: l.Book.Id,
            UserEmail: l.User.Email,
            BookTitle: l.Book.Title,
            StartDate: l.StartDate,
            ExpirationDate: l.ExpirationDate,
            ReturnDate: l.ReturnDate
        )).ToList();

        return TypedResults.Ok(response);
    }

    public static async Task<Results<Created<Loan>, NotFound<ErrorResponse>, Conflict<ErrorResponse>, BadRequest<ErrorResponse>>> CreateLoan(
        [FromBody] LoanCreationRequest request, 
        [FromServices] LoansContext context)
    {
        var errors = ValidationUtil.Validate(request);
        if (errors is not null)
            return TypedResults.BadRequest(new ErrorResponse(
                Message: "Validation failed",
                Errors: errors
            ));

        var user = await context.Users.FindAsync(request.UserId);
        if (user == null) 
            return TypedResults.NotFound(new ErrorResponse(Message: "User not found", Errors: null));

        var book = await context.Books.FindAsync(request.BookId);
        if (book == null) 
            return TypedResults.NotFound(new ErrorResponse(Message: "Book not found", Errors: null));

        if (book.Available <= 0)
            return TypedResults.Conflict(new ErrorResponse(Message: "No available copies of the book.", Errors: null)); 

        var loan = new Loan
        {
            Book = book,
            User = user,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ExpirationDate = request.ExpirationDate ?? DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(1),
            ReturnDate = null
        };

        book.Available -= 1;
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        return TypedResults.Created($"/loans/{loan.Id}", loan);
    }

    public static async Task<Results<Ok<LoanReturnResponse>, NotFound, Conflict<ErrorResponse>>> ReturnLoan(
        int id, 
        [FromServices] LoansContext context)
    {
        var loan = await context.Loans
            .Include(l => l.Book)
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan == null) 
            return TypedResults.NotFound();

        if (loan.ReturnDate.HasValue) 
            return TypedResults.Conflict(new ErrorResponse(Message: "Loan already returned.", Errors: null)); 

        loan.ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow);
        loan.Book.Available += 1;

        var confirmation = new LoanReturnResponse(
            LoanId: id,
            BookId: loan.Book.Id,
            ReturnDate: loan.ReturnDate,
            UserId: loan.User.Id,
            BookTitle: loan.Book.Title,
            UserName: loan.User.Name,
            UserEmail: loan.User.Email,
            StartDate: loan.StartDate,
            ExpirationDate: loan.ExpirationDate
        );

        await context.SaveChangesAsync();
        return TypedResults.Ok(confirmation);
    }
}
