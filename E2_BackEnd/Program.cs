using Dapr.Client;
using Dapr.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace E2_BackEnd
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseUrls("http://*:5000");
                })
                .ConfigureAppConfiguration(config =>
                {
                    var daprClient = new DaprClientBuilder().Build();
                    var secretsDescriptors = new List<DaprSecretDescriptor> { new DaprSecretDescriptor("RabbitMQConnectStr") };
                    config.AddDaprSecretStore("secrets01", secretsDescriptors, daprClient);
                });
    }
}
