﻿using System;
using System.Threading.Tasks;
using Billy.Infrastructure.Configs;
using Billy.Infrastructure.WebApp;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Billy
{
    public class Program
    {
        private static readonly ILogger Logger = Log.ForContext<Program>();

        public static void Main(string[] args)
        {
            try
            {
                var config = Configuration.Read();

                ConfigureLogger(config);

                ApiHost.Run(config);
            }
            catch (Exception exception)
            {
                Logger.Fatal(exception, "Billy error on startup");
                throw;
            }
            finally
            {
                Logger.Information("Stopping Billy");
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureLogger(IConfiguration config)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();
        }

    }
}
