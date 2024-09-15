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
        private int _riavvii = 0;

        public async Task CheckEthernet()
        {
            NetworkInterface[] nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();

            var adapter = nics.FirstOrDefault(x => x.Name == _settings.AdapterName);

            if (adapter != null)
            {
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
                    _logger.LogWarning("Cavo di rete disconnesso...");
                }
                else if (adapter.Speed < 1000000000 || !await ExecutePing())
                {
                    _riavvii++;
                    var tempoPassato = DateTime.Now - _startTime;
                    _logger.LogWarning($"Errore scheda di rete. Tempo passato: {tempoPassato}. Riavvio scheda di rete per la {_riavvii} volta.");

                    DisableAdapter(_settings.AdapterName);
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    EnableAdapter(_settings.AdapterName);
                    await Task.Delay(TimeSpan.FromSeconds(5));

                    if (!await ExecutePing())
                        throw new Exception("Errore nel riavviare la scheda di rete! Esito del ping negativo.");

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

        async Task<bool> ExecutePing(int occurrence = 0)
        {
            occurrence++;

            if(occurrence > _settings.MaxPingOccurrences)
            {
                _logger.LogError($"Impossibile raggiungere l'host {_settings.PingAddress} dopo {_settings.MaxPingOccurrences} tentativi.");
                return false;
            }

            var ping = new Ping();

            var pingReply = ping.Send(_settings.PingAddress);

            if(pingReply.Status != IPStatus.Success)
            {
                _logger.LogWarning($"Ping verso l'host {_settings.PingAddress} non riuscito con esito {pingReply.Status}.");
                await Task.Delay(TimeSpan.FromSeconds(1));
                // Ritento
                return await ExecutePing(occurrence);
            }

            _logger.LogTrace($"Ping positivo.");

            return true;
        }
    }
}
