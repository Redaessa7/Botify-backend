using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;

namespace Botify.Infrastructure.CrossCutting.BackgroundServices.Email;
public class EmailQueue : IEmailQueue
{
    private readonly Channel<(string Email, string Otp)> _queue = Channel.CreateUnbounded<(string, string)>();

    public void Enqueue(string email, string otp) =>
        _queue.Writer.TryWrite((email, otp));

    public async ValueTask<(string Email, string Otp)> DequeueAsync(CancellationToken ct) =>
        await _queue.Reader.ReadAsync(ct);
}
