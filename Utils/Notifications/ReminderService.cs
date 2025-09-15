using Datapac.Models;
using Microsoft.EntityFrameworkCore;

namespace Datapac.Utils.Notifications;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

public class ReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MockEmailService _emailService;

    public ReminderService(IServiceProvider serviceProvider, MockEmailService emailService)
    {
        _serviceProvider = serviceProvider;
        _emailService = emailService;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        while (!stoppingToken.IsCancellationRequested)
        {
            // This code would ensure the reminder is sent every day at 8 AM. However, for demonstration, it will be sent every 5 seconds instead
            
            // var nextRun = DateTime.Today.AddHours(8);
            //
            // if (DateTime.Now > nextRun)
            // {
            //     nextRun = nextRun.AddDays(1);
            // }
            //
            // var delay = nextRun - DateTime.Now;
            // await Task.Delay(delay, stoppingToken);
            
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<LoansContext>();
                
                var tomorrow =  DateOnly.FromDateTime(DateTime.Now.AddDays(1));
                
                var expiringLoans = await context.Loans
                    .Where(l => l.ReturnDate == null && l.ExpirationDate == tomorrow)
                    .Include(l => l.User)
                    .Include(l => l.Book)
                    .ToListAsync(stoppingToken);

                foreach (var loan in expiringLoans)
                {
                    var subject = "Loan expiration reminder";
                    var body = $"Dear {loan.User.Name},\n\n" +
                               $"Your loan of \"{loan.Book.Title}\" expires tomorrow ({loan.ExpirationDate}).";

                    await _emailService.SendReminderEmailAsync(loan.User.Email, subject, body);
                }
            }
            
            // For demonstration, we wait for 5 seconds between remainders
            await Task.Delay(5000, stoppingToken); 
        }
    }
}