using System;
using Autofac;
using ContainerBuilder = Autofac.ContainerBuilder;


namespace Scratch.Autofac
{
    using System.Threading.Tasks;
    using Autofac;
    using MassTransit;


    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.AddMassTransit(x =>
            {

                x.UsingRabbitMq((context, cfg) =>
                {

                    cfg.Host("rabbitmq://cricket-uat-rabbitmq0", hc =>
                    {
                        hc.Username("cricket");
                        hc.Password("cricket");
                        hc.UseCluster(cc =>
                        {
                            cc.Node("cricket-uat-rabbitmq0");
                            cc.Node("cricket-uat-rabbitmq1");
                            cc.Node("cricket-uat-rabbitmq2");
                        });
                    });


                });
            });

            var container = builder.Build();
            var bus = container.Resolve<IBusControl>();
            await bus.StartAsync();
            Console.ReadLine();
        }
    }
}
