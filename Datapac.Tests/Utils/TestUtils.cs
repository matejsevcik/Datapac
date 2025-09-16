using Datapac.Models;
using Microsoft.EntityFrameworkCore;

namespace Datapac.Tests.Utils;

public static class TestUtils
{
    public static LoansContext InitializeContext(string? dbName = null)
    {
        return new LoansContext(
            new DbContextOptionsBuilder<LoansContext>()
                .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
                .Options
        );
    }

    public static async Task SeedContextWithBooksAsync(LoansContext context)
    {
        var books = new[]
        {
            new Book 
            { 
                Title = "C# Basics", 
                Author = "Alice", 
                TotalCopies = 5, 
                Available = 5 
            },
            new Book 
            { 
                Title = "Entity Framework Core", 
                Author = "Bob", 
                TotalCopies = 3, 
                Available = 3 
            },
            new Book 
            { 
                Title = "Deleted", 
                Author = "No one", 
                TotalCopies = 1, 
                Available = 1,
                DeletedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                IsDeleted = true
            }
        };

        context.Books.AddRange(books);
        await context.SaveChangesAsync();
    }
    
    public static async Task SeedContextWithUsersAsync(LoansContext context)
    {
        var users = new[]
        {
            new User { Name = "Alice", Email = "alice@example.com" },
            new User { Name = "Bob", Email = "bob@example.com" }
        };
        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }
}