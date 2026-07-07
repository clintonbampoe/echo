namespace Echo.Application.Models;

public interface IEmailContent
{
    string Subject { get; }
    string HtmlBody { get; }
}
