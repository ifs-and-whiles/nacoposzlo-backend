namespace Billy.Api.Mobile.Infrastructure.Configs
{
    public class RequestRetryPolicyConfig
    {
        public int RetryCount { get; set; }
        public int MilisecondsBetweenRetries { get; set; }
    }
}
