using Billy.Receipts.Storage.Minio;
using Billy.Receipts.Storage.S3;

namespace Billy.Receipts.Infrastructure.Configs
{
    public class StorageConfig
    {
        public S3StorageConfig S3StorageConfig { get; set; }
        
        public MinioStorageConfig MinioStorageConfig { get; set; }
        
        public StorageProvider StorageProvider { get; set; } = StorageProvider.S3;
    }
    
    public enum StorageProvider
    {
        Minio = 1,
        S3 = 2
    }
}