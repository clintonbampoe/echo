namespace Echo.Shared.Services.Email;

public sealed record EmailRow(string Label, string Value);

public sealed record EmailStep(string Title, string Body);

public sealed record EmailButton(string Text, string Url);

public sealed record EmailCallout(string Title, string Body);

public sealed record EmailCode(string Label, string Value);

/// <summary>
/// Everything one email says, as plain text. <see cref="EmailLayout"/> turns it into HTML, so no
/// email builds markup itself and every email looks the same. Nothing here is encoded: the layout
/// encodes it all.
/// </summary>
public sealed class EmailPage
{
    /// <summary>Small capitalised label in the header block, for example "Account" or "Security".</summary>
    public required string Category { get; init; }
    public required string Heading { get; init; }

    /// <summary>The grey line mail apps show after the subject in the inbox list.</summary>
    public required string Preheader { get; init; }
    public required IReadOnlyList<string> Paragraphs { get; init; }
    public required EmailButton Button { get; init; }

    /// <summary>Why the person got this email, shown in the footer.</summary>
    public required string Reason { get; init; }

    public IReadOnlyList<EmailRow> Rows { get; init; } = [];
    public IReadOnlyList<EmailStep> Steps { get; init; } = [];
    public EmailCallout? Callout { get; init; }
    public EmailCode? Code { get; init; }

    /// <summary>Small print under the button, for example when a link expires.</summary>
    public string? Note { get; init; }

    /// <summary>Shows the button's link as text, for when the button does not work.</summary>
    public bool ShowLinkFallback { get; init; }
}
