using Datapac.Endpoints;
using Datapac.Models;
using Datapac.Requests;
using Datapac.Responses;
using Datapac.Tests.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Datapac.Tests;

public class LoansEndpointsTests
{
    // GetAllLoans
    [Fact]
    public async Task GetAllLoans_ReturnsLoansWithDetails()
    {
        // Arrange
        await using var context = TestUtils.InitializeContext();
        await TestUtils.SeedContextWithUsersAsync(context);
        await TestUtils.SeedContextWithBooksAsync(context);

        var user = await context.Users.FirstAsync();
        var book = await context.Books.FirstAsync(b => !b.IsDeleted);

        var loan = new Loan
        {
            User = user,
            Book = book,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        // Act
        var result = await LoansEndpoints.GetAllLoans(context);

        // Assert
        var okResult = Assert.IsType<Ok<List<LoanDetailResponse>>>(result);
        Assert.Single(okResult.Value);
        Assert.Equal(user.Email, okResult.Value[0].UserEmail);
        Assert.Equal(book.Title, okResult.Value[0].BookTitle);
    }

    // CreateLoan
    
    [Fact]
    public async Task CreateLoan_ReturnsCreated_WhenValid()
    {
        // Arrange
        await using var context = TestUtils.InitializeContext();
        await TestUtils.SeedContextWithUsersAsync(context);
        await TestUtils.SeedContextWithBooksAsync(context);

        var user = await context.Users.FirstAsync();
        var book = await context.Books.FirstAsync(b => !b.IsDeleted);

        var request = new LoanCreationRequest { UserId = user.Id, BookId = book.Id };

        // Act
        var result = await LoansEndpoints.CreateLoan(request, context);

        // Assert
        var createdResult = Assert.IsType<Created<Loan>>(result.Result);
        Assert.Equal(book.Id, createdResult.Value.Book.Id);
        Assert.Equal(user.Id, createdResult.Value.User.Id);

        var updatedBook = await context.Books.FindAsync(book.Id);
        Assert.Equal(book.TotalCopies - 1, updatedBook.Available);
    }

    [Fact]
    public async Task CreateLoan_ReturnsConflict_WhenNoCopies()
    {
        // Arrange
        await using var context = TestUtils.InitializeContext();
        await TestUtils.SeedContextWithUsersAsync(context);
        await TestUtils.SeedContextWithBooksAsync(context);

        var user = await context.Users.FirstAsync();
        var book = await context.Books.FirstAsync(b => !b.IsDeleted);
        book.Available = 0;
        await context.SaveChangesAsync();

        var request = new LoanCreationRequest { UserId = user.Id, BookId = book.Id };

        // Act
        var result = await LoansEndpoints.CreateLoan(request, context);

        // Assert
        var conflictResult = Assert.IsType<Conflict<ErrorResponse>>(result.Result);
        Assert.Equal("No available copies of the book.", conflictResult.Value.Message);
    }

    // ReturnLoan
    
    [Fact]
    public async Task ReturnLoan_ReturnsOk_WhenValid()
    {
        // Arrange
        await using var context = TestUtils.InitializeContext();
        await TestUtils.SeedContextWithUsersAsync(context);
        await TestUtils.SeedContextWithBooksAsync(context);

        var user = await context.Users.FirstAsync();
        var book = await context.Books.FirstAsync(b => !b.IsDeleted);
        book.Available = 0;

        var loan = new Loan
        {
            User = user,
            Book = book,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        // Act
        var result = await LoansEndpoints.ReturnLoan(loan.Id, context);

        // Assert
        var okResult = Assert.IsType<Ok<LoanReturnResponse>>(result.Result);
        Assert.Equal(book.Id, okResult.Value.BookId);
        Assert.Equal(user.Id, okResult.Value.UserId);
        Assert.NotNull(okResult.Value.ReturnDate);

        var updatedBook = await context.Books.FindAsync(book.Id);
        Assert.Equal(1, updatedBook.Available);
    }

    [Fact]
    public async Task ReturnLoan_ReturnsConflict_WhenAlreadyReturned()
    {
        // Arrange
        await using var context = TestUtils.InitializeContext();
        await TestUtils.SeedContextWithUsersAsync(context);
        await TestUtils.SeedContextWithBooksAsync(context);

        var user = await context.Users.FirstAsync();
        var book = await context.Books.FirstAsync(b => !b.IsDeleted);

        var loan = new Loan
        {
            User = user,
            Book = book,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        // Act
        var result = await LoansEndpoints.ReturnLoan(loan.Id, context);

        // Assert
        var conflictResult = Assert.IsType<Conflict<ErrorResponse>>(result.Result);
        Assert.Equal("Loan already returned.", conflictResult.Value.Message);
    }
}
