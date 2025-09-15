namespace Datapac.Utils.Notifications;

public class MockEmailService
{
    public Task SendReminderEmailAsync(string toEmail, string subject, string message)
    {
        Console.WriteLine("-------------------------");
        Console.WriteLine($"To: {toEmail}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"{message}\n");
        Console.WriteLine("-------------------------");
        return Task.CompletedTask;
    }
}