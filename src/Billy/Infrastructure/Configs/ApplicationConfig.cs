namespace Billy.Infrastructure.Configs
{
    public class ApplicationConfig
    {
        public bool RunProjections { get; set; } = true;
        public int Port { get; set; }

        public bool RunRabbitMq { get; set; } = true;
    }
}
