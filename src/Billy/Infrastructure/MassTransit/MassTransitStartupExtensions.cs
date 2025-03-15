using System;
using System.Security.Authentication;
using Amazon.Runtime;
using Autofac;
using Billy.Infrastructure.Configs;
using Billy.Infrastructure.CorrelationId;
using Billy.Receipts.Contracts;
using Billy.Receipts.ReceiptRecognition.Consumers;
using Billy.Users.ReceiptsRecognition;
using GreenPipes;
using GreenPipes.Configurators;
using MassTransit;
using MassTransit.Context;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.PrometheusIntegration;
using Microsoft.Extensions.DependencyInjection;

namespace Billy.Infrastructure.MassTransit
{
    public static class MassTransitStartupExtensions
    {
        public static void SetupMassTransit(
            this IServiceCollection services, 
            AWSAccessConfiguration awsAccessConfiguration,
            QueueConfig queueConfig)
        {
            LogContext.ConfigureCurrentLogContext();

            services.AddMassTransit(configurator =>
            {
                configurator.AddConsumers(
                    typeof(ExtractPrintedTextFromReceiptHandler), 
                    typeof(RecognizePrintedTextFromReceiptHandler),
                    typeof(SaveRecognizedReceiptHandler),
                    typeof(StorePrintedTextRecognitionResultHandler),
                    typeof(StoreRecognitionAlgorithmResultHandler),
                    typeof(ReceiptRecognitionCompletedConsumer));

                if (queueConfig.QueueProvider == QueueProvider.InMemory)
                    ConfigureInMemoryQueue(configurator);

                if (queueConfig.QueueProvider == QueueProvider.SQS)
                    ConfigureSQSQueue(awsAccessConfiguration, queueConfig, configurator);
                
                if (queueConfig.QueueProvider == QueueProvider.RabbitMQ)
                    ConfigureRabbit(queueConfig.RabbitMqConfig, configurator);
            });

            services.AddMassTransitHostedService();
        }

        private static void ConfigureSQSQueue(
            AWSAccessConfiguration awsAccessConfiguration,
            QueueConfig queueConfig, 
            IServiceCollectionBusConfigurator configurator)
        {
            configurator.UsingAmazonSqs((context, cfg) =>
            {
                cfg.Host(awsAccessConfiguration.DefaultRegion, h =>
                {
                    h.Scope(queueConfig.SqsConfig.Scope);

                    h.EnableScopedTopics();
                });

                cfg.ReceiveEndpoint("receipts-to-printed-text-recognition-queue", e =>
                {
                    // disable the default topic binding
                    e.ConfigureConsumeTopology = true;
                    e.ConfigureConsumer<ExtractPrintedTextFromReceiptHandler>(context);
                });
                cfg.ReceiveEndpoint("receipts-to-recognize-queue", e =>
                {
                    // disable the default topic binding
                    e.ConfigureConsumeTopology = true;

                    e.ConfigureConsumer<RecognizePrintedTextFromReceiptHandler>(context);
                    e.ConfigureConsumer<StorePrintedTextRecognitionResultHandler>(context);
                });
                cfg.ReceiveEndpoint("recognized-receipts-to-process-queue", e =>
                {
                    // disable the default topic binding
                    e.ConfigureConsumeTopology = true;
                    e.ConfigureConsumer<SaveRecognizedReceiptHandler>(context);
                    e.ConfigureConsumer<StoreRecognitionAlgorithmResultHandler>(context);
                });
                cfg.ReceiveEndpoint("completed-receipt-recognitions-queue", e =>
                {
                    // disable the default topic binding
                    e.ConfigureConsumeTopology = true;
                    e.ConfigureConsumer<ReceiptRecognitionCompletedConsumer>(context);
                });
            });
        }

        private static void ConfigureInMemoryQueue(IServiceCollectionBusConfigurator configurator)
        {
            configurator.UsingInMemory((context, cfg) =>
            {
                cfg.ReceiveEndpoint("receipts-to-printed-text-recognition-queue",
                    e => { e.ConfigureConsumer<ExtractPrintedTextFromReceiptHandler>(context); });

                cfg.ReceiveEndpoint("receipts-to-recognize-queue", e =>
                {
                    e.ConfigureConsumer<RecognizePrintedTextFromReceiptHandler>(context);
                    e.ConfigureConsumer<StorePrintedTextRecognitionResultHandler>(context);
                });

                cfg.ReceiveEndpoint("recognized-receipts-to-process-queue", e =>
                {
                    e.ConfigureConsumer<SaveRecognizedReceiptHandler>(context);
                    e.ConfigureConsumer<StoreRecognitionAlgorithmResultHandler>(context);
                });

                cfg.ReceiveEndpoint("completed-receipt-recognitions-queue",
                    e => { e.ConfigureConsumer<ReceiptRecognitionCompletedConsumer>(context); });
            });
        }

        private static void ConfigureRabbit(RabbitMqConfig rabbitMqConfig, IServiceCollectionBusConfigurator configurator)
        {
            configurator.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.UsePrometheusMetrics();
                        
                        cfg.SetQueueArgument("x-expires", null);
                        cfg.AutoDelete = false;
                        cfg.Durable = true;
                        cfg.PrefetchCount = 1;
                        cfg.UseConcurrencyLimit(1);
                        
                        cfg.UseMessageRetry(r => r.None());
    
                        cfg.Host(new Uri(rabbitMqConfig.HostUrl), h => {
    
                            h.Heartbeat(5);
                            h.RequestedConnectionTimeout(TimeSpan.FromHours(1));
    
                            if (rabbitMqConfig.Ssl)
                            {
                                h.UseSsl(ssl =>
                                    {
                                        ssl.Protocol = SslProtocols.Tls11 | SslProtocols.Tls12;
                                        ssl.CertificateValidationCallback = (sender, certificate, chain, errors) => true;
                                    }
                                );
                            }
    
                            h.Username(rabbitMqConfig.User);
                            h.Password(rabbitMqConfig.Password);
                        });
                        
                        cfg.ReceiveEndpoint("receipts-to-printed-text-recognition-queue", e =>
                        {
                            e.ConfigureConsumer<ExtractPrintedTextFromReceiptHandler>(context);
                        });
    
                        cfg.ReceiveEndpoint("receipts-to-recognize-queue", e =>
                        {
                            e.ConfigureConsumer<RecognizePrintedTextFromReceiptHandler>(context);
                            e.ConfigureConsumer<StorePrintedTextRecognitionResultHandler>(context);
                        });
    
                        cfg.ReceiveEndpoint("recognized-receipts-to-process-queue", e =>
                        {
                            e.ConfigureConsumer<SaveRecognizedReceiptHandler>(context);
                            e.ConfigureConsumer<StoreRecognitionAlgorithmResultHandler>(context);
                        });
    
                        cfg.ReceiveEndpoint("completed-receipt-recognitions-queue", e =>
                        {
                            e.ConfigureConsumer<ReceiptRecognitionCompletedConsumer>(context);
                        });
                    });
        }
    }


}
