using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Core.Workers
{
    public class EmailBackgroundWorker : BackgroundService
    {
        private readonly IEmailQueue _emailQueue;
        private readonly IConfiguration _config;
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
