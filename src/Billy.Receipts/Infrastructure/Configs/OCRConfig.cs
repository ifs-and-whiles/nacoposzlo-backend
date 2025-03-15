using Billy.Receipts.AWS.VisionAPI;

namespace Billy.Receipts.Infrastructure.Configs
{
    public class OCRConfig
    {
        public AWSOcrConfig AWSConfig { get; set; }
        
        public OCRProvider OCRProvider { get; set; }
    }
    
    public enum OCRProvider
    {
        AWS = 1
    }
}