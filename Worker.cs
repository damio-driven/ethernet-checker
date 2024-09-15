using EthernetChecker.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EthernetChecker
{
    internal class Worker(ILogger<Worker> logger, Settings settings, Service service) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly Service _service = service;
        private readonly Settings _settings = settings;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _service.CheckEthernet();
                }
                catch(Exception ex)
                {
                    _logger.LogCritical(ex.ToString());
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromSeconds(_settings.RecurringSeconds), stoppingToken);
                }
            }
        }
    }
}
