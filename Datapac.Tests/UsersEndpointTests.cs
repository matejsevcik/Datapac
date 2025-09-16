using Datapac.Endpoints;
using Datapac.Models;
using Datapac.Tests.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Datapac.Tests.Utils;
using Xunit;

namespace Datapac.Tests;

public class UsersEndpointTests
{
    [Fact]
    public async Task GetAllUsers_ReturnsAllSeededUsers()
    {
        // Arrange
        await using var context = TestUtils.InitializeContext("UsersTestDb");
        await TestUtils.SeedContextWithUsersAsync(context);
        
        // Act
        var result = await UsersEndpoints.GetAllUsers(context);
        
        //Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<Ok<List<User>>>(result);
        Assert.Equal(2, okResult.Value.Count);
        Assert.Contains(okResult.Value, u => u.Name == "Alice" && u.Email == "alice@example.com");
        Assert.Contains(okResult.Value, u => u.Name == "Bob" && u.Email == "bob@example.com");
    }
}