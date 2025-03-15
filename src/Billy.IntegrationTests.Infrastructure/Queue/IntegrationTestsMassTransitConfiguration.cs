using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;
using Billy.Infrastructure.Configs;
using MassTransit;

namespace Billy.IntegrationTests.Infrastructure.Queue
{
    public class IntegrationTestsMassTransitConfiguration
    {
        public static IBusControl ConfigureBus(RabbitMqConfig config)
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.SetQueueArgument("x-expires", null);
                cfg.AutoDelete = false;
                cfg.Durable = true;

                cfg.Host(new Uri(config.HostUrl), h => {

                    if (config.Ssl)
                    {
                        h.UseSsl(ssl =>
                            {
                                ssl.Protocol = SslProtocols.Tls11 | SslProtocols.Tls12;
                                ssl.CertificateValidationCallback = (sender, certificate, chain, errors) => true;
                            }
                        );
                    }

                    h.Username(config.User);
                    h.Password(config.Password);
                });
            });
        }
    }
}
