namespace Echo.Auth.Models;

public class VerificationEmailContent(string recipientName, string verificationLink)
{
    public string Subject => "Verify your Echo account";

    public string HtmlBody => $"""
                               <html>
                                 <body style="font-family: Arial, sans-serif; color: #333;">
                                   <h2>Welcome to Echo, {recipientName}</h2>
                                   <p>Thanks for signing up. Please verify your email address to activate your account.</p>
                                   <p>
                                     <a href="{verificationLink}"
                                        style="display:inline-block; padding:5px 12px; background-color:#2563eb; color:#fff; text-decoration:none; border-radius:6px;">
                                       Verify Email
                                     </a>
                                   </p>
                                   <p>Or copy and paste this link into your browser:</p>
                                   <p>{verificationLink}</p>
                                   <p>This link will expire in 24 hours. If you didn't create this account, you can safely ignore this email.</p>
                                 </body>
                               </html>
                               """;
}
