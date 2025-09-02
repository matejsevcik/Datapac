using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Datapac.Models;

public class LoansContext : DbContext
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("LoansInMemoryDb");
    }
}