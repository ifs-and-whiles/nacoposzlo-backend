namespace Billy.Infrastructure.Configs
{
    public class AWSAccessConfiguration
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public bool UseInstanceAWSRoleBasedAuthentication { get; set; }
        public string DefaultRegion { get; set; }
        
    }
}