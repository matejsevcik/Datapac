using Datapac.Endpoints;
using Datapac.Models;
using Datapac.Requests;
using Datapac.Responses;
using Datapac.Tests.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Datapac.Tests;

public class BookEndpointTests
{
    // GetAllBooks
    
    [Fact]
    public async Task GetAllBooks_ReturnsAllSeededBooks()
    {
        // Arrange
        await using var context = TestUtils.InitializeContext();
        await TestUtils.SeedContextWithBooksAsync(context);
        
        // Act
        var result = await BooksEndpoints.GetAllBooks(context);
        
        //Assert
        var okResult = Assert.IsType<Ok<List<Book>>>(result);
        Assert.Equal(2, okResult.Value.Count);
        Assert.Contains(okResult.Value, u => u.Title == "C# Basics" && u.Author == "Alice" && u.TotalCopies == 5 &&  u.Available == 5);
        Assert.Contains(okResult.Value, u => u.Title == "Entity Framework Core" && u.Author == "Bob" && u.TotalCopies == 3 &&  u.Available == 3);
    }
    
    // GetBookById

    [Fact]
    public async Task GetBookById_ReturnsBookById()
    {
        // Arrange
        await using var context = TestUtils.InitializeContext();
        await TestUtils.SeedContextWithBooksAsync(context);
        
        // Act
        var result = await BooksEndpoints.GetBookById(1, context);
        
        //Assert
        var okResult = result.Result as Ok<Book>;
        Assert.NotNull(okResult); 
        Assert.Equal("C# Basics", okResult.Value.Title);
    }
    
    [Fact]
    public async Task GetBookById_NotFound_ReturnsNotFound()
    {
        // Arrange
        await using var context = TestUtils.InitializeContext();
        await TestUtils.SeedContextWithBooksAsync(context);
        
        // Act
        var result = await BooksEndpoints.GetBookById(4, context);
        
        //Assert
        var okResult = result.Result as NotFound;
        Assert.NotNull(okResult); 
    }
    
    [Fact]
    public async Task GetBookById_SoftDeletedBook()
    {
        // Arrange
        await using var context = TestUtils.InitializeContext();
        await TestUtils.SeedContextWithBooksAsync(context);
        
        // Act
        var result = await BooksEndpoints.GetBookById(3, context);
        
        //Assert
        var okResult = result.Result as Ok<Book>;
        Assert.NotNull(okResult); 
        Assert.Equal("Deleted", okResult.Value.Title);
        Assert.True(okResult.Value.IsDeleted);
    }
    
    // Create book
    
    [Fact]
    public async Task CreateBook_ReturnsCreatedBook()
    {
        // Arrange
        await using var context = TestUtils.InitializeContext();
        var request = new BookCreationRequest
        {
            Title = "C# Basics",
            Author = "Alice",
            TotalCopies = 5
        };
        
        // Act
        var result = await BooksEndpoints.CreateBook(request, context);
        
        //Assert
        var createdResult = result.Result as Created<Book>;
        Assert.NotNull(createdResult); 
        Assert.Equal("C# Basics", createdResult.Value.Title);
    }
    
    [Fact]
    public async Task CreateBook_BadRequest_ReturnsBadRequest()
    {
        // Arrange
        await using var context = TestUtils.InitializeContext();
        var request = new BookCreationRequest
        {
            Title = "C# Basics",
            Author = "Alice",
            TotalCopies = -1
        };

        // Act
        var result = await BooksEndpoints.CreateBook(request, context);
        
        //Assert
        var createdResult = result.Result as BadRequest<ErrorResponse>;
        Assert.NotNull(createdResult); 
    }
    
     // Update book
    
     [Fact]
     public async Task UpdateBook_UpdatesExistingBook()
     {
         // Arrange
         await using var context = TestUtils.InitializeContext();
         await TestUtils.SeedContextWithBooksAsync(context);

         var request = new BookUpdateRequest
         {
             Title = "C# Advanced",
             Author = "Alice Updated",
             TotalCopies = 10
         };

         // Act
         var result = await BooksEndpoints.UpdateBook(1, request, context);

         // Assert
         var noContentResult = Assert.IsType<NoContent>(result.Result);

         // Verify that changes persisted
         var updatedBook = await context.Books.FindAsync(1);
         Assert.NotNull(updatedBook);
         Assert.Equal("C# Advanced", updatedBook.Title);
         Assert.Equal("Alice Updated", updatedBook.Author);
         Assert.Equal(10, updatedBook.TotalCopies);
         Assert.Equal(10, updatedBook.Available);
     }

    [Fact]
    public async Task UpdateBook_BookNotFound_ReturnsNotFound()
    {
        // Arrange
        await using var context = TestUtils.InitializeContext();
        var request = new BookUpdateRequest
        {
            Title = "Nonexistent",
            Author = "Nobody",
            TotalCopies = 1
        };

        // Act
        var result = await BooksEndpoints.UpdateBook(99, request, context);

        // Assert
        var notFoundResult = result.Result as NotFound;
        Assert.NotNull(notFoundResult);
    }

    [Fact]
    public async Task UpdateBook_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        await using var context = TestUtils.InitializeContext();
        await TestUtils.SeedContextWithBooksAsync(context);

        var request = new BookUpdateRequest
        {
            Title = "Broken Book",
            Author = "Bad Author",
            TotalCopies = -5 // invalid
        };

        // Act
        var result = await BooksEndpoints.UpdateBook(1, request, context);

        // Assert
        var badRequestResult = result.Result as BadRequest<ErrorResponse>;
        Assert.NotNull(badRequestResult);
    }

    // Delete book
    
    [Fact]
    public async Task DeleteBook_DeletesExistingBook()
    {
        // Arrange
        await using var context = TestUtils.InitializeContext();
        await TestUtils.SeedContextWithBooksAsync(context);

        // Act
        var result = await BooksEndpoints.DeleteBook(1, context);

        // Assert
        var noContentResult = result.Result as NoContent;
        Assert.NotNull(noContentResult);
    }

    [Fact]
    public async Task DeleteBook_BookNotFound_ReturnsNotFound()
    {
        // Arrange
        await using var context = TestUtils.InitializeContext();

        // Act
        var result = await BooksEndpoints.DeleteBook(42, context);

        // Assert
        var notFoundResult = result.Result as NotFound;
        Assert.NotNull(notFoundResult);
    }
}