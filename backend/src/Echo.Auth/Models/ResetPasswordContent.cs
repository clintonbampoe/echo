using Echo.Application.Models;

namespace Echo.Auth.Models;

public class ResetPasswordContent(string recipientName, string resetLink) : IEmailContent
{
    public string Subject => "Reset your Echo password";

    public string HtmlBody => $"""
                               <html>
                                 <body style="font-family: Arial, sans-serif; color: #333;">
                                   <h2>Hi {recipientName},</h2>
                                   <p>We received a request to reset your Echo password. Click below to choose a new one.</p>
                                   <p>
                                     <a href="{resetLink}"
                                        style="display:inline-block; padding:5px 12px; background-color:#2563eb; color:#fff; text-decoration:none; border-radius:6px;">
                                       Reset Password
                                     </a>
                                   </p>
                                   <p>This link will expire in 1 hour. If you didn't request this, you can safely ignore this email — your password won't be changed.</p>
                                 </body>
                               </html>
                               """;
}
