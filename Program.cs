using System.Text.Json;
using Datapac.Endpoints;
using Datapac.Models;
using Datapac.Requests;
using Datapac.Responses;
using Datapac.Utils;
using Datapac.Utils.Notifications;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddSingleton<MockEmailService>();
builder.Services.AddHostedService<ReminderService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<SoftDeleteInterceptor>();

builder.Services.AddDbContext<LoansContext>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Ensure DB created and seeded
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

        if (books != null && users != null)
        {
            dbContext.Users.AddRange(users);
            dbContext.Books.AddRange(books);
            await dbContext.SaveChangesAsync();
        }
    }
}

// Map endpoint groups
app.MapGroup("/books").MapBooksEndpoints();
app.MapGroup("/users").MapUsersEndpoints();
app.MapGroup("/loans").MapLoansEndpoints();

app.Run();

public partial class Program { }