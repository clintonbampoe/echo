namespace Api.Auth.Services.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string html);
}
