namespace Botify.Infrastructure.CrossCutting.BackgroundServices.Email;
public interface IEmailQueue
{
    void Enqueue(string email, string otp);
    ValueTask<(string Email, string Otp)> DequeueAsync(CancellationToken ct);
}

