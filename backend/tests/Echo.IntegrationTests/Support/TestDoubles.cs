using Echo.Domain.Users;
using Echo.Shared.HttpResults;
using Echo.Shared.Models;
using Echo.Shared.Services.Email;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;

namespace Echo.IntegrationTests.Support;

/// <summary>Records what auth publishes instead of delivering it.</summary>
public sealed class RecordingEmailQueue : IEmailQueue
{
    private readonly List<EmailMessage> _messages = [];

    public bool Accepting { get; set; } = true;

    public IReadOnlyList<EmailMessage> Messages
    {
        get
        {
            lock (_messages)
                return [.. _messages];
        }
    }

    public bool TryEnqueue(EmailMessage message)
    {
        if (!Accepting)
            return false;
        lock (_messages)
            _messages.Add(message);
        return true;
    }
}

/// <summary>An email service that never finishes, to prove a request does not wait on it.</summary>
public sealed class HangingEmailService : IEmailService
{
    public Task SendAsync(string to, IEmailContent content) => new TaskCompletionSource().Task;
}

public static class TestHostExtensions
{
    /// <summary>Swaps the real queue and email sender for the recording queue and a hanging sender.</summary>
    public static Action<IServiceCollection> WithRecordedEmail(RecordingEmailQueue queue) =>
        services =>
        {
            services.AddSingleton<IEmailQueue>(queue);
            services.AddScoped<IEmailService, HangingEmailService>();
        };

    public static string Message(this IOperationResult result) =>
        ((Ok<string>)result.ToResult()).Value!;

    public static User NewUser(Guid congregationId, string email = "ama@echo.test") =>
        new()
        {
            Id = Guid.NewGuid(),
            FirstName = "Ama",
            LastName = "Mensah",
            Name = "Ama Mensah",
            EmailAddress = email,
            PasswordHash = "x",
            Role = UserRole.Admin,
            CongregationId = congregationId,
        };
}
