using System;
using Microsoft.AspNetCore.Http;

namespace Billy.Receipts.Contracts
{
    public static class Commands
    {
        public static class Receipt
        {
            public static class V1
            {
                public class RecognizeReceipt
                {
                    public string GlobalUserIdentifier { get; set; }
                    public IFormFile Image { get; set; }
                    
                    public int? ImageWidth { get; set; }
                        
                    public int? ImageHeight { get; set; }
                    public override string ToString() => $"{GetType().FullName}";
                }
                
                public class RecognizeReceiptResponse 
                {
                    public Guid ReceiptId { get; set; }
                }

            }
        }
    }
}
