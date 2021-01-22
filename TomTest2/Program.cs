using System;
using System.Threading;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MTTEst
{
    using MassTransit.Monitoring.Health;


    class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder()
                .ConfigureLogging(l =>
               {
                   l.ClearProviders();
                   l.AddConsole();
               })
                .ConfigureServices(s =>
               {
                   s.AddMassTransit(c =>
                   {
                       c.UsingRabbitMq((cx, rx) =>
                       {
                            rx.Host("rabbitmq://localhost", hc =>
                            {
                                hc.Username("cricket");
                                hc.Password("cricket");
                            });

                            rx.ReceiveEndpoint("test-receive-endpoint", re =>
                            {
                                re.PrefetchCount = 1;
                                re.UseConcurrencyLimit(1);
                                re.Handler<TestMessage>(async context =>
                                {
                                    Console.WriteLine("Got it");
                                    await Task.Delay(TimeSpan.FromSeconds(5));
                                });
                            });

                       });

                   });
                   s.AddMassTransitHostedService();
               }).RunConsoleAsync();
        }
    }

    class TestMessage
    {

    }

}
