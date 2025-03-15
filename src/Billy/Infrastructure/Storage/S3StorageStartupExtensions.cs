using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.Textract;
using Billy.Infrastructure.Configs;
using Microsoft.Extensions.DependencyInjection;

namespace Billy.Infrastructure.Storage
{
    public static class S3StorageStartupExtensions
    {
        public static void SetupAWSAccessConfiguration(
            this IServiceCollection services,
            AWSAccessConfiguration awsAccessConfiguration)
        {
            services.AddDefaultAWSOptions(GetAWSConfiguration());
            
            AWSOptions GetAWSConfiguration()
            {
                if (awsAccessConfiguration.UseInstanceAWSRoleBasedAuthentication)
                    return new AWSOptions()
                    {
                        Credentials = new InstanceProfileAWSCredentials(),
                        Region = RegionEndpoint.GetBySystemName(awsAccessConfiguration.DefaultRegion)
                    };
                else
                    return new AWSOptions()
                    {
                        Credentials = new BasicAWSCredentials(awsAccessConfiguration.AccessKey, awsAccessConfiguration.SecretKey),
                        Region = RegionEndpoint.GetBySystemName(awsAccessConfiguration.DefaultRegion)
                    };
            }
        }
    }
}