using System.Net;
using System.Net.Mail;
using Botify.Application.CrossCutting.Interfaces;
using Microsoft.Extensions.Options;

namespace Botify.Infrastructure.CrossCutting.Services.EmailSenders;

public class SmtpEmailSender(IOptions<SmtpSettings> options) : IEmailSender
{
    private readonly SmtpSettings _settings = options.Value;

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient(_settings.Host, _settings.Port);
        client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
        client.EnableSsl = _settings.EnableSsl;

        var mail = new MailMessage
        {
            From = new MailAddress(_settings.From),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mail.To.Add(to);

        await client.SendMailAsync(mail);
    }

    public async Task SendOtpAsync(string to, string otp)
    {
        var subject = "Your OTP Code";
        var body = $@"
            <html>
                <body>
                    <p>Hello,</p>
                    <p>Your one-time password (OTP) is:</p>
                    <h2>{otp}</h2>
                    <p>This code will expire in 10 minutes.</p>
                </body>
            </html>";

        await SendEmailAsync(to, subject, body);
    }
}
