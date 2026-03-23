using Core.DTOs;
using Core.Interfaces;
using System.Threading.Channels;

namespace Core.Services
{
    public class EmailQueue : IEmailQueue
    {
        private readonly Channel<EmailMessage> _queue = Channel.CreateUnbounded<EmailMessage>();
        public async Task<EmailMessage> DequeueAsync(CancellationToken cancellationToken) => await _queue.Reader.ReadAsync(cancellationToken);
        public void QueueEmail(EmailMessage message) => _queue.Writer.TryWrite(message);
    }
}
