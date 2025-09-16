using Datapac.Models;
using Datapac.Requests;
using Datapac.Responses;
using Datapac.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Datapac.Endpoints;

public static class BooksEndpoints
{
    public static RouteGroupBuilder MapBooksEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllBooks);
        group.MapGet("/{id}", GetBookById);
        group.MapPost("/", CreateBook);
        group.MapPut("/{id}", UpdateBook);
        group.MapDelete("/{id}", DeleteBook);
        
        return group;
    }
    
    public static async Task<Ok<List<Book>>> GetAllBooks([FromServices] LoansContext context)
        => TypedResults.Ok(await context.Books.ToListAsync());

    public static async Task<Results<Ok<Book>, NotFound>> GetBookById(int id, [FromServices] LoansContext context)
    {
        var book = await context.Books.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == id);
        return book is null ? TypedResults.NotFound() : TypedResults.Ok(book);
    }

    public static async Task<Results<Created<Book>, BadRequest<ErrorResponse>>> CreateBook(
        [FromBody] BookCreationRequest request, 
        [FromServices] LoansContext context)
    {
        var errors = ValidationUtil.Validate(request);
        if (errors is not null)
            return TypedResults.BadRequest(new ErrorResponse(
                Message: "Validation failed",
                Errors: errors
            ));

        var book = new Book
        {
            Author = request.Author,
            Title = request.Title,
            TotalCopies = request.TotalCopies,
            Available = request.TotalCopies
        };

        context.Books.Add(book);
        await context.SaveChangesAsync();

        return TypedResults.Created($"/books/{book.Id}", book);
    }

    public static async Task<Results<NoContent, NotFound, Conflict<ErrorResponse>, BadRequest<ErrorResponse>>> UpdateBook(
        int id, 
        [FromBody] BookUpdateRequest request, 
        [FromServices] LoansContext context)
    {
        var errors = ValidationUtil.Validate(request);
        if (errors is not null) 
            return TypedResults.BadRequest(new ErrorResponse(
                Message: "Validation failed",
                Errors: errors
            ));

        var originalBook = await context.Books.FindAsync(id);
        if (originalBook == null) return TypedResults.NotFound();

        if (request.Title != null) originalBook.Title = request.Title;
        if (request.Author != null) originalBook.Author = request.Author;

        if (request.TotalCopies.HasValue)
        {
            originalBook.Available += (int)(request.TotalCopies - originalBook.TotalCopies);
            originalBook.TotalCopies = request.TotalCopies.Value;
            if (originalBook.Available < 0)
                return TypedResults.Conflict(new ErrorResponse(
                    Message: "Cannot remove unreturned book copies.", 
                    Errors: null
                ));
        }

        await context.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    public static async Task<Results<NoContent, NotFound, Conflict<ErrorResponse>>> DeleteBook(
        int id, 
        [FromServices] LoansContext context)
    {
        var book = await context.Books.FindAsync(id);
        if (book == null) return TypedResults.NotFound();

        var hasActiveLoans = await context.Loans.AnyAsync(l => l.Book.Id == id && l.ReturnDate == null);
        if (hasActiveLoans)
            return TypedResults.Conflict(new ErrorResponse(
                Message: "Book has an active loan, it cannot be deleted.", 
                Errors: null
            ));

        context.Books.Remove(book);
        await context.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}
