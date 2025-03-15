using System.IO;
using Microsoft.Extensions.Configuration;

namespace Billy.Infrastructure.Configs
{
    public static class Configuration
    {
        public static IConfiguration Read()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
