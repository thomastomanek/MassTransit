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

                       });

                   });
                   s.AddMassTransitHostedService();
                   s.AddHostedService<RequestService>();
               }).RunConsoleAsync();
        }
    }


    class RequestService : IHostedService
    {
        readonly IBus _bus;
        readonly IBusHealth _busHealth;

        public RequestService(IBus bus, IBusHealth busHealth)
        {
            _bus = bus;
            _busHealth = busHealth;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            await Task.Factory.StartNew(async () =>
            {
                for (var i = 0; i < int.MaxValue; i++)
                {
                    try
                    {
                        await WaitForHealthyBus(cancellationToken);
                        await _bus.Publish(new TestMessage());
                    }
                    catch (Exception e)
                    {

                    }

                    Thread.Sleep(100);
                }
            });

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }

        async Task WaitForHealthyBus(CancellationToken cancellationToken)
        {
            HealthResult result;
            do
            {
                result = _busHealth.CheckHealth();

                await Task.Delay(100, cancellationToken);
            } while (result.Status != BusHealthStatus.Healthy);
        }
    }

    class TestMessage
    {

    }

}
