using Echo.Shared.Models;

namespace Echo.Shared.Services.Email;

public interface IEmailService
{
    Task SendAsync(string to, IEmailContent content);
}
