using EthernetChecker.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;

namespace EthernetChecker
{
    internal class Service(ILogger<Service> logger, Settings settings)
    {
        private readonly ILogger<Service> _logger = logger;
        private readonly Settings _settings = settings;
        private DateTime _startTime = DateTime.Now;

        public async Task CheckEthernet()
        {
            IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
            NetworkInterface[] nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();

            var adapter = nics.FirstOrDefault(x => x.Name == _settings.AdapterName);

            if (adapter != null)
            {
                _logger.LogDebug("Interface information for {0}.{1}     ", computerProperties.HostName, computerProperties.DomainName);

                IPInterfaceProperties properties = adapter.GetIPProperties();
                _logger.LogDebug(adapter.Description);
                _logger.LogDebug(string.Empty.PadLeft(adapter.Description.Length, '='));
                _logger.LogDebug("  Interface type .......................... : {0}", adapter.NetworkInterfaceType);
                _logger.LogDebug("  Physical Address ........................ : {0}", adapter.GetPhysicalAddress().ToString());
                _logger.LogDebug("  Is receive only.......................... : {0}", adapter.IsReceiveOnly);
                _logger.LogDebug("  Multicast................................ : {0}", adapter.SupportsMulticast);
                _logger.LogDebug("  Speed................................ : {0}", adapter.Speed);


                if (adapter.Speed == -1)
                {
                    _logger.LogWarning("Cavo disconnesso...");
                }
                else if (adapter.Speed < 1000)
                {
                    var tempoPassato = DateTime.Now - _startTime;
                    _logger.LogWarning($"Tempo passato: {tempoPassato}. Riavvio scheda di rete");

                    DisableAdapter(_settings.AdapterName);
                    await Task.Delay(1000);
                    EnableAdapter(_settings.AdapterName);

                    _startTime = DateTime.Now;
                }
            }
            else
            {
                _logger.LogWarning($"Adapter \"{_settings.AdapterName}\" non identificato");
            }
        }

        static void EnableAdapter(string interfaceName)
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", "interface set interface \"" + interfaceName + "\" enable");
            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();
        }

        static void DisableAdapter(string interfaceName)
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", "interface set interface \"" + interfaceName + "\" disable");
            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();
        }
    }
}
