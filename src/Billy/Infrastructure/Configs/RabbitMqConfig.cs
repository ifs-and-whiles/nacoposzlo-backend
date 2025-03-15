using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Billy.Infrastructure.Configs
{
    public class RabbitMqConfig
    {
        public string HostUrl { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public bool Ssl { get; set; }

    }
}
