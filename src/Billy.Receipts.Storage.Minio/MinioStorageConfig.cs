namespace Billy.Receipts.Storage.Minio
{
    public class MinioStorageConfig
    {       
        public bool Ssl { get; set; }
        
        public string Endpoint { get; set; }
        
        public string BucketName { get; set; }
        
        public string AccessKey { get; set; }
        
        public string SecretKey { get; set; }
        
    }
}