using System;
using Billy.IntegrationTests.Infrastructure.Utils;
using Newtonsoft.Json;

namespace Billy.IntegrationTests.Infrastructure
{
    public class TestApiCallErrorException : Exception
    {
        public ErrorDetails ErrorDetails { get; }

        public TestApiCallErrorException(ErrorDetails error)
        {
            ErrorDetails = error;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(ErrorDetails);
        }
    }
}
