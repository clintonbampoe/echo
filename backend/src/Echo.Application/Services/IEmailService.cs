using Echo.Application.Models;

namespace Echo.Application.Services;

public interface IEmailService
{
    Task SendAsync(string to, IEmailContent content);
}
