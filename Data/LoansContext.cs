using System.Text.Json;
using Datapac.Utils;
using Microsoft.EntityFrameworkCore;

namespace Datapac.Models;

public class LoansContext : DbContext
{
    public LoansContext(DbContextOptions<LoansContext> options)
        : base(options)
    {
    }
    
    public DbSet<Book> Books { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseInMemoryDatabase("LoansInMemoryDb")
                .AddInterceptors(new SoftDeleteInterceptor());
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>()
            .HasQueryFilter(x => !x.IsDeleted);
    }
}