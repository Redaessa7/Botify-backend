namespace Botify.Application.CrossCutting.Interfaces;


public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendOtpAsync(string to, string otp);
}