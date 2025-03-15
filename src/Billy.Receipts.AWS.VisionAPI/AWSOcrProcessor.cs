using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Textract;
using Amazon.Textract.Model;
using Billy.CodeReadability;
using Billy.Metrics;
using Billy.Receipts.Domain;
using Newtonsoft.Json;
using Serilog;

namespace Billy.Receipts.AWS.VisionAPI
{
    public class AWSOcrProcessor : IOcr
    {
        private readonly IAmazonTextract _amazonTextract;
        private readonly AWSOcrConfig _awsOcrConfig;
        private ILogger _logger = Log.ForContext<AWSOcrProcessor>();
        
        public OcrProviderName ProviderName { get; } = OcrProviderName.From("AWS Textract");

        public AWSOcrProcessor(IAmazonTextract amazonTextract, AWSOcrConfig awsOcrConfig)
        {
            _amazonTextract = amazonTextract;
            _awsOcrConfig = awsOcrConfig;
        }
        
        public async Task<PrintedTextRecognitionResult> RecognizeImage(
            ReceiptId receiptId,
            ReceiptDimensions receiptDimensions)
        {
            try
            {
                using (new OcrTimer("aws"))
                {
                    var awsOCRResult = await _amazonTextract.DetectDocumentTextAsync(
                        new DetectDocumentTextRequest()
                    {
                        Document = new Document()
                        {
                            S3Object = new S3Object()
                            {
                                Bucket = _awsOcrConfig.ImageStorageBucket,
                                Name = $"{receiptId}/ReceiptImage"
                            }
                        }
                    });
                    
                    //Only for demo
                    var jsonResponse = JsonConvert.SerializeObject(awsOCRResult);
                    
                    var rawDetectedTexts = awsOCRResult
                        .Blocks
                        .Where(line => line.BlockType == BlockType.LINE)
                        .Select(line => new DetectedText(
                            text: line.Text,
                            leftTop: ReceiptPoint.FromNormalized(
                                line.Geometry.Polygon[0].X, 
                                line.Geometry.Polygon[0].Y,
                                receiptDimensions),
                            
                            rightTop: ReceiptPoint.FromNormalized(
                                line.Geometry.Polygon[1].X, 
                                line.Geometry.Polygon[1].Y,
                                receiptDimensions),
                            
                            rightBottom: ReceiptPoint.FromNormalized(
                                line.Geometry.Polygon[2].X,
                                line.Geometry.Polygon[2].Y,
                                receiptDimensions),
                            
                            leftBottom: ReceiptPoint.FromNormalized(
                                line.Geometry.Polygon[3].X,
                                line.Geometry.Polygon[3].Y,
                                receiptDimensions)))
                        .ToArray();

                    return new PrintedTextRecognitionResult(rawDetectedTexts);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "AWS OCR error. Error while recognizing image");
                throw;
            }
        }
    }
}