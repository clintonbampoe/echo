using Echo.Application.Models;

namespace Echo.Application.Services.Email;

public interface IEmailService
{
    Task SendAsync(string to, IEmailContent content);
}
