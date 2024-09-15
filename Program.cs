using EthernetChecker.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Slitu.Helper.Log.ConsoleProvider;
using Slitu.Helper.Log.FileProvider;
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace EthernetChecker
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //bool isWinPlantform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            var builder = Host.CreateDefaultBuilder(args);

            builder.UseWindowsService(opt =>
            {
                opt.ServiceName = "EthernetChecker";
            });

            builder.ConfigureServices((context, services) =>
            {
                Settings? settings = context.Configuration.GetSection("Settings").Get<Settings>();
                if(settings != null)
                    services.AddSingleton(settings);

                services.AddSingleton<Service>();
                services.AddHostedService<Worker>();
            });

            builder.ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                logging.AddProvider(new ConsoleProvider());
                logging.AddProvider(new FileProvider(context.Configuration));
#pragma warning disable CA1416 // Convalida compatibilità della piattaforma
                logging.AddEventLog();
#pragma warning restore CA1416 // Convalida compatibilità della piattaforma
            });

            IHost host = builder.Build();
            await host.RunAsync();
        }        
    }
}
