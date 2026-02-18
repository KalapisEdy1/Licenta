using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

public class EmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Datele de la Mailtrap (le iei din panoul lor)
        var client = new SmtpClient("sandbox.smtp.mailtrap.io", 2525)
        {
            Credentials = new NetworkCredential("75214fb34ada4c", "d554b99e7aac52"),
            EnableSsl = true
        };

        var mailMessage = new MailMessage("kalapisedy2@gmail.com", email, subject, htmlMessage)
        {
            IsBodyHtml = true
        };

        return client.SendMailAsync(mailMessage);
    }
}
