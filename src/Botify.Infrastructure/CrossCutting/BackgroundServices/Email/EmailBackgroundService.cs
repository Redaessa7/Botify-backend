using Botify.Application.CrossCutting.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Botify.Infrastructure.CrossCutting.BackgroundServices.Email;

public class EmailBackgroundService : BackgroundService
{
    private readonly IEmailQueue _queue;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailBackgroundService> _logger;

    public EmailBackgroundService(
        IEmailQueue queue,
        IEmailSender emailSender,
        ILogger<EmailBackgroundService> logger)
    {
        _queue = queue;
        _emailSender = emailSender;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var job = await _queue.DequeueAsync(stoppingToken);

            try
            {
                _logger.LogInformation("Processing email job for {Email}", job.Email);
                await _emailSender.SendOtpAsync(job.Email, job.Otp);
                _logger.LogInformation("Successfully sent OTP email to {Email}", job.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email}", job.Email);
            }
        }

        _logger.LogInformation("Email background service stopping.");
    }
}
