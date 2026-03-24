using Core.DTOs;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MimeKit;
using MailKit.Net.Smtp;

namespace Core.Workers
{
    public class EmailBackgroundWorker : BackgroundService
    {
        private readonly IEmailQueue _emailQueue;
        private readonly IConfiguration _config;

        public EmailBackgroundWorker(IEmailQueue emailQueue, IConfiguration config)
        {
            _emailQueue = emailQueue;
            _config = config;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var message = await _emailQueue.DequeueAsync(stoppingToken);
                    await SendEmailAsync(message);

                } catch (Exception e)
                {
                    Console.WriteLine($"{e.Message}");
                }
            }
        }
        private async Task SendEmailAsync(EmailMessage msg)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_config["EmailSettings:SenderName"], _config["EmailSettings:SenderEmail"]));
            email.To.Add(MailboxAddress.Parse(msg.To));
            email.Subject = msg.Subject;
            email.Body = new TextPart("html") { Text = msg.Body };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _config["EmailSettings:SmtpServer"],
                int.Parse(_config["EmailSettings:SmtpPort"]),
                MailKit.Security.SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(_config["EmailSettings:Username"], _config["EmailSettings:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
