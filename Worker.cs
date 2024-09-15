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
        ILogger<Worker> _logger = logger;
        Service _service = service;
        Settings _settings = settings;

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
                    await Task.Delay(_settings.RecurringSeconds * 1000);
                }
            }
        }
    }
}
