using Core.DTOs.Message;

namespace Core.Interfaces
{
    public interface IEmailQueue
    {
        void QueueEmail(EmailMessage message);
        Task<EmailMessage> DequeueAsync(CancellationToken cancellationToken);
    }
}
