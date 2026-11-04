using System.Net;
using System.Text;

namespace Echo.Shared.Services.Email;

/// <summary>
/// The one HTML layout every Echo email uses. Tables and inline styles, because mail apps ignore
/// most modern CSS. The only classes are for dark mode and small screens, which need a
/// stylesheet; apps that drop the stylesheet still show the light, full-width version.
/// </summary>
public static class EmailLayout
{
    // The logo files live in the frontend's public/email folder. Emails already sent point at
    // these paths, so do not rename them.
    private const string LogoInkPath = "/email/echo-mark.png";
    private const string LogoPaperPath = "/email/echo-mark-light.png";

    private const string Font =
        "Geist,-apple-system,BlinkMacSystemFont,'Segoe UI',Helvetica,Arial,sans-serif";
    private const string Mono = "ui-monospace,SFMono-Regular,Menlo,Consolas,monospace";

    // Light colours. The dark ones are in Styles, where the classes below switch to them.
    private const string Bg = "#F6F6F7";
    private const string Card = "#FFFFFF";
    private const string Ink = "#0A0A0B";
    private const string Ink2 = "#52525B";
    private const string Ink3 = "#6A6A72";
    private const string Line = "#E4E4E7";
    private const string Line2 = "#CFCFD5";
    private const string Soft = "#F6F6F7";

    private const string Styles = """
        @media (prefers-color-scheme: dark) {
          .c-bg { background-color: #09090A !important; }
          .c-card { background-color: #111113 !important; border-color: #1F1F23 !important; }
          .c-soft { background-color: #17171A !important; }
          .c-ink { color: #EDEDEF !important; }
          .c-ink2 { color: #A1A1AA !important; }
          .c-ink3 { color: #85858D !important; }
          .c-line { border-color: #1F1F23 !important; }
          .c-line2 { border-color: #2E2E34 !important; }
          .c-btn { background-color: #FAFAFA !important; }
          .c-btn-text { color: #09090A !important; }
          .logo-ink { display: none !important; }
          .logo-paper { display: inline-block !important; }
        }
        @media (max-width: 480px) {
          .outer { padding: 16px 12px 28px !important; }
          .px-head { padding: 18px 24px !important; }
          .px-body { padding: 36px 24px 40px !important; }
          .px-foot { padding: 26px 24px 30px !important; }
          .h1 { font-size: 28px !important; }
          .code { font-size: 20px !important; }
          .btn-table { width: 100% !important; }
          .btn-text { display: block !important; }
        }
        """;

    /// <summary>"Hi Ama," or "Hi there," when there is no name.</summary>
    public static string Greeting(string? name) =>
        string.IsNullOrWhiteSpace(name) ? "Hi there," : $"Hi {name.Trim()},";

    public static string Render(EmailPage page)
    {
        var html = new StringBuilder();
        AppendHead(html, page);
        AppendHeader(html, page);

        html.Append("""<tr><td class="px-body" style="padding:48px 44px 52px">""");
        html.Append("""<table role="presentation" width="100%" cellpadding="0" cellspacing="0">""");

        AppendHeading(html, page.Heading);
        foreach (var paragraph in page.Paragraphs)
            AppendParagraph(html, paragraph);
        if (page.Rows.Count > 0)
            AppendRows(html, page.Rows);
        if (page.Steps.Count > 0)
            AppendSteps(html, page.Steps);
        if (page.Section is not null)
            AppendSection(html, page.Section);
        if (page.Callout is not null)
            AppendCallout(html, page.Callout);
        AppendButton(html, page.Button);
        if (page.Code is not null)
            AppendCode(html, page.Code);
        if (page.Note is not null)
            AppendNote(html, page.Note);
        if (page.ShowLinkFallback)
            AppendLinkFallback(html, page.Button.Url);

        html.Append("</table></td></tr>");
        AppendFooter(html, page.Reason);
        html.Append("</table></td></tr></table></body></html>");
        return html.ToString();
    }

    private static string Esc(string text) => WebUtility.HtmlEncode(text);

    private static void AppendHead(StringBuilder html, EmailPage page)
    {
        html.Append($"""
            <!doctype html>
            <html lang="en">
            <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <meta name="color-scheme" content="light dark">
            <meta name="supported-color-schemes" content="light dark">
            <title>{Esc(page.Heading)}</title>
            <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Geist:wght@400;500;600&amp;display=swap">
            <style>
            {Styles}
            </style>
            </head>
            <body class="c-bg" style="margin:0;padding:0;background-color:{Bg};-webkit-text-size-adjust:100%">
            <div style="display:none;max-height:0;max-width:0;overflow:hidden;opacity:0;font-size:1px;line-height:1px;mso-hide:all">{Esc(page.Preheader)}</div>
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" class="c-bg" style="background-color:{Bg}">
            <tr><td align="center" class="outer" style="padding:28px 28px 44px">
            <table role="presentation" width="640" cellpadding="0" cellspacing="0" class="c-card c-line" style="width:100%;max-width:640px;background-color:{Card};border:1px solid {Line};border-radius:24px;overflow:hidden;font-family:{Font}">
            """);
    }

    private static void AppendHeader(StringBuilder html, EmailPage page)
    {
        var site = page.SiteUrl.TrimEnd('/');
        html.Append($"""
            <tr><td class="px-head c-line" style="padding:22px 44px;border-bottom:1px solid {Line}">
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0"><tr>
            <td valign="middle">
            <img src="{Esc(site + LogoInkPath)}" width="24" height="24" alt="" class="logo-ink" style="display:inline-block;vertical-align:middle;border:0">
            <img src="{Esc(site + LogoPaperPath)}" width="24" height="24" alt="" class="logo-paper" style="display:none;vertical-align:middle;border:0">
            <span class="c-ink" style="vertical-align:middle;padding-left:10px;font-size:18px;font-weight:600;letter-spacing:-0.03em;color:{Ink}">Echo</span>
            </td>
            <td align="right" valign="middle" class="c-ink3" style="font-size:13.5px;font-weight:500;color:{Ink3}">{Esc(page.Category)}</td>
            </tr></table>
            </td></tr>
            """);
    }

    private static void AppendHeading(StringBuilder html, string heading) =>
        html.Append($"""
            <tr><td class="h1 c-ink" style="font-size:32px;line-height:1.12;font-weight:600;letter-spacing:-0.035em;color:{Ink}">{Esc(heading)}</td></tr>
            """);

    private static void AppendParagraph(StringBuilder html, string text) =>
        html.Append($"""
            <tr><td class="c-ink2" style="padding-top:18px;font-size:16.5px;line-height:1.65;color:{Ink2}">{Esc(text)}</td></tr>
            """);

    private static void AppendRows(StringBuilder html, IReadOnlyList<EmailRow> rows)
    {
        html.Append($"""
            <tr><td style="padding-top:32px">
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" class="c-line" style="border-top:1px solid {Line}">
            """);
        foreach (var row in rows)
        {
            html.Append($"""
                <tr>
                <td class="c-ink3 c-line" style="padding:15px 20px 15px 0;border-bottom:1px solid {Line};font-size:15.5px;line-height:1.4;color:{Ink3}" valign="top">{Esc(row.Label)}</td>
                <td align="right" class="c-ink c-line" style="padding:15px 0;border-bottom:1px solid {Line};font-size:15.5px;line-height:1.4;font-weight:500;color:{Ink};word-break:break-word" valign="top">{Esc(row.Value)}</td>
                </tr>
                """);
        }
        html.Append("</table></td></tr>");
    }

    private static void AppendSteps(StringBuilder html, IReadOnlyList<EmailStep> steps)
    {
        html.Append("""<tr><td style="padding-top:32px"><table role="presentation" width="100%" cellpadding="0" cellspacing="0">""");
        for (var i = 0; i < steps.Count; i++)
        {
            var gap = i == 0 ? 0 : 22;
            html.Append($"""
                <tr>
                <td width="32" valign="top" style="width:32px;padding-top:{gap}px"><div class="c-ink c-line2" style="width:30px;height:30px;border:1px solid {Line2};border-radius:16px;text-align:center;line-height:30px;font-size:14px;font-weight:600;color:{Ink}">{i + 1}</div></td>
                <td width="16" style="width:16px">&nbsp;</td>
                <td valign="top" style="padding-top:{gap + 4}px">
                <div class="c-ink" style="font-size:16.5px;line-height:1.3;font-weight:600;letter-spacing:-0.01em;color:{Ink}">{Esc(steps[i].Title)}</div>
                <div class="c-ink2" style="padding-top:4px;font-size:15.5px;line-height:1.55;color:{Ink2}">{Esc(steps[i].Body)}</div>
                </td>
                </tr>
                """);
        }
        html.Append("</table></td></tr>");
    }

    private static void AppendSection(StringBuilder html, EmailSection section) =>
        html.Append($"""
            <tr><td style="padding-top:32px">
            <div class="c-ink" style="font-size:18px;line-height:1.3;font-weight:600;letter-spacing:-0.02em;color:{Ink}">{Esc(section.Title)}</div>
            <div class="c-ink2" style="padding-top:8px;font-size:16.5px;line-height:1.65;color:{Ink2}">{Esc(section.Body)}</div>
            </td></tr>
            """);

    private static void AppendCallout(StringBuilder html, EmailCallout callout) =>
        html.Append($"""
            <tr><td style="padding-top:32px">
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0"><tr>
            <td class="c-soft c-line2" style="padding:20px 24px;background-color:{Soft};border:1px solid {Line2};border-radius:16px">
            <div class="c-ink" style="font-size:16.5px;line-height:1.3;font-weight:600;letter-spacing:-0.01em;color:{Ink}">{Esc(callout.Title)}</div>
            <div class="c-ink2" style="padding-top:6px;font-size:15.5px;line-height:1.55;color:{Ink2}">{Esc(callout.Body)}</div>
            </td>
            </tr></table>
            </td></tr>
            """);

    private static void AppendButton(StringBuilder html, EmailButton button) =>
        html.Append($"""
            <tr><td style="padding-top:36px">
            <table role="presentation" cellpadding="0" cellspacing="0" class="btn-table"><tr>
            <td align="center" class="c-btn" style="background-color:{Ink};border-radius:999px">
            <a href="{Esc(button.Url)}" class="btn-text c-btn-text" style="display:inline-block;padding:0 34px;height:52px;line-height:52px;font-size:16px;font-weight:600;letter-spacing:-0.005em;color:#FFFFFF;text-decoration:none;border-radius:999px">{Esc(button.Text)}</a>
            </td>
            </tr></table>
            </td></tr>
            """);

    private static void AppendCode(StringBuilder html, EmailCode code) =>
        html.Append($"""
            <tr><td style="padding-top:32px">
            <div class="c-ink2" style="padding-bottom:12px;font-size:14.5px;line-height:1.5;color:{Ink2}">{Esc(code.Label)}</div>
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0"><tr>
            <td align="center" class="code c-soft c-line c-ink" style="padding:20px 18px;background-color:{Soft};border:1px solid {Line};border-radius:16px;font-family:{Mono};font-size:24px;font-weight:600;letter-spacing:0.12em;color:{Ink}">{Esc(code.Value)}</td>
            </tr></table>
            </td></tr>
            """);

    private static void AppendNote(StringBuilder html, string note) =>
        html.Append($"""
            <tr><td class="c-ink3" style="padding-top:18px;font-size:14px;line-height:1.55;color:{Ink3}">{Esc(note)}</td></tr>
            """);

    private static void AppendLinkFallback(StringBuilder html, string url) =>
        html.Append($"""
            <tr><td style="padding-top:32px">
            <div class="c-ink3" style="padding-bottom:10px;font-size:14px;line-height:1.5;color:{Ink3}">Button not working? Paste this link into your browser.</div>
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0"><tr>
            <td class="c-soft c-line c-ink2" style="padding:14px 16px;background-color:{Soft};border:1px solid {Line};border-radius:16px;font-family:{Mono};font-size:12.5px;line-height:1.6;color:{Ink2};word-break:break-all">{Esc(url)}</td>
            </tr></table>
            </td></tr>
            """);

    private static void AppendFooter(StringBuilder html, string reason) =>
        html.Append($"""
            <tr><td class="px-foot c-soft c-line" style="padding:30px 44px 36px;background-color:{Soft};border-top:1px solid {Line}">
            <div class="c-ink3" style="font-size:13.5px;line-height:1.6;color:{Ink3}">{Esc(reason)}</div>
            <div class="c-ink3" style="padding-top:14px;font-size:13px;font-weight:500;color:{Ink3}">Echo · Your church, answered.</div>
            </td></tr>
            """);
}
