using System.Text.Json;
using Datapac.Models;
using Datapac.Requests;
using Datapac.Responses;
using Datapac.Utils;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<SoftDeleteInterceptor>();

builder.Services.AddDbContext<LoansContext>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LoansContext>();
    await dbContext.Database.EnsureCreatedAsync();
    
    if (!dbContext.Books.Any())
    {
        var bookJson = File.ReadAllText("Data/books_mock_data.json");
        var userJson = File.ReadAllText("Data/users_mock_data.json");
        var books = JsonSerializer.Deserialize<List<Book>>(bookJson);
        var users = JsonSerializer.Deserialize<List<User>>(userJson);
        if (books != null  && users != null)
        {
            dbContext.Users.AddRange(users);
            dbContext.Books.AddRange(books);
            await dbContext.SaveChangesAsync();
        }
    }
}



// BOOK
var booksGroup = app.MapGroup("/books");

booksGroup.MapGet("/", async (LoansContext context) => await context.Books
    .ToListAsync());

booksGroup.MapGet("/{id}", async (int id, LoansContext context) => 
{
    var currentBook = await context.Books
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(b => b.Id == id);
    return currentBook == null ? Results.NotFound() : Results.Ok(currentBook);
});

booksGroup.MapPost("/", async (BookCreationRequest bookCreationRequest, LoansContext context) =>
{
    var errors = ValidationUtil.Validate(bookCreationRequest);
    if (errors is not null) return Results.BadRequest(new { Message = "Validation failed", Errors = errors });
    
    var book = new Book
    {
        Author = bookCreationRequest.Author,
        Title = bookCreationRequest.Title,
        TotalCopies = bookCreationRequest.TotalCopies
    };
    book.Available = book.TotalCopies;
    
    context.Books.Add(book);
    await context.SaveChangesAsync();
    return Results.Created(booksGroup + $"/{book.Id}", book);
});

booksGroup.MapPut("/{id}", async (int id, BookUpdateRequest bookUpdateRequest, LoansContext context) =>
{
    var errors = ValidationUtil.Validate(bookUpdateRequest);
    if (errors is not null) return Results.BadRequest(new { Message = "Validation failed", Errors = errors });
    
    var originalBook = await context.Books.FindAsync(id);
    if (originalBook == null)
    {
        return Results.NotFound();
    }

    if (bookUpdateRequest.Title != null)
        originalBook.Title = bookUpdateRequest.Title;

    if (bookUpdateRequest.Author != null)
        originalBook.Author = bookUpdateRequest.Author;

    if (bookUpdateRequest.TotalCopies.HasValue)
    {
        originalBook.Available += (int)(bookUpdateRequest.TotalCopies - originalBook.TotalCopies);
        originalBook.TotalCopies = bookUpdateRequest.TotalCopies.Value;
        
        if(originalBook.Available < 0)
            return Results.Conflict(new { Message = "Cannot remove unreturned book copies."});
    }
    
    await context.SaveChangesAsync();
    return Results.NoContent();
});

booksGroup.MapDelete("/{id}", async (int id, LoansContext context) =>
{
    var originalBook = await context.Books.FindAsync(id);

    if (originalBook == null)
    {
        return Results.NotFound();
    }
    
    var hasActiveLoans = await context.Loans.AnyAsync(
        l => l.Book.Id == id && l.ReturnDate == null);

    if (hasActiveLoans)
    {
        return Results.Conflict(new { Message = "Book has an active loan, it cannot be deleted." });
    }

    context.Books.Remove(originalBook);
    
    await context.SaveChangesAsync();
    return Results.NoContent();
});



// USER
var usersGroup = app.MapGroup("/users");
    
usersGroup.MapGet("/", async (LoansContext context) => await context.Users.ToListAsync());



// LOAN
var loansGroup = app.MapGroup("/loans");

loansGroup.MapGet("/", async (LoansContext context) =>
{
    var allLoans = await context.Loans
        .Include(l => l.Book)
        .Include(l => l.User)
        .IgnoreQueryFilters()
        .ToListAsync();
    var response = new List<Responses.LoanDetailResponse>();
    
    foreach (var loan in allLoans)
    {
        response.Add(new Responses.LoanDetailResponse(
            LoanId:  loan.Id,
            UserId: loan.User.Id,
            BookId: loan.Book.Id,
            UserEmail: loan.User.Email,
            BookTitle: loan.Book.Title,
            StartDate:  loan.StartDate,
            ExpirationDate: loan.ExpirationDate,
            ReturnDate: loan.ReturnDate
            ));
    }
    
    return Results.Ok(response);
});

loansGroup.MapPost("/", async (LoanCreationRequest loanCreationRequest, LoansContext context) =>
{
    var user = await context.Users.FindAsync(loanCreationRequest.UserId);
    if (user == null)
        return Results.NotFound(new { message = "User not found" });

    var book = await context.Books.FindAsync(loanCreationRequest.BookId);
    if (book == null)
        return Results.NotFound(new { message = "Book not found" });

    if(book.Available <= 0)
        return Results.Conflict(new { message = "No available copies of the book." });
    
    var loan = new Loan
    {
        Book = book,
        User =  user,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        ExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(1),
        ReturnDate = null
    };

    book.Available -= 1;

    context.Loans.Add(loan);
    await context.SaveChangesAsync();
    return Results.Created($"/{loan.Id}", loan);
});

loansGroup.MapPut("/{id}/return", async (int id, LoansContext context) =>
{
    var originalLoan = await context.Loans
        .Include(l => l.Book)
        .Include(l => l.User)
        .FirstOrDefaultAsync(l => l.Id == id);
    
    if (originalLoan == null)
    {
        return Results.NotFound();
    }
    
    if(originalLoan.ReturnDate.HasValue)
        return Results.Conflict(new { Message = "Loan already returned." });
    
    originalLoan.ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow);
    originalLoan.Book.Available += 1;
    
    var confirmation = new Responses.LoanReturnResponse(
        LoanId: id,
        BookId: originalLoan.Book.Id,
        ReturnDate: originalLoan.ReturnDate,
        UserId:  originalLoan.User.Id,
        BookTitle: originalLoan.Book.Title,
        UserName: originalLoan.User.Name,
        UserEmail: originalLoan.User.Email,
        StartDate:  originalLoan.StartDate,
        ExpirationDate: originalLoan.ExpirationDate
        );
        
    await context.SaveChangesAsync();
    return Results.Ok(confirmation);
});

app.Run();
