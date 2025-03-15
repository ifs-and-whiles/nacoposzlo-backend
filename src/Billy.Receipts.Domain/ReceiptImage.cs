using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Billy.Domain;

namespace Billy.Receipts.Domain
{
    public class ReceiptImage : Value<ReceiptImage>
    {
        public Stream ImageStream { get; }
        public string Extension { get; }
        public ReceiptDimensions ReceiptDimensions { get; }

        private readonly List<string> _allowedImageExtensions = new List<string>(){ ".png", ".jpg", ".jpeg"}; 
        
        public ReceiptImage(
            Stream imageStream, 
            string imageExtension,
            ReceiptDimensions receiptDimensions)
        {
            Validate(imageStream, imageExtension, receiptDimensions);
            ImageStream = imageStream;
            Extension = imageExtension;
            ReceiptDimensions = receiptDimensions;
        }

        private void Validate(Stream imageStream, string imageExtension, ReceiptDimensions receiptDimensions)
        {
            if(imageStream == null)
                throw new InvalidValueException("Receipt image cannot be empty", ErrorCodes.ReceiptImageCanNotBeEmpty);

            if(receiptDimensions == null)
                throw new InvalidValueException("Receipt dimensions cannot be empty", ErrorCodes.ReceiptDimensionsCanNotBeEmpty);
            
            if(!_allowedImageExtensions.Contains(imageExtension))
                throw new InvalidValueException($"Not supported image extension. Extension: {imageExtension}. Allowed extensions: {_allowedImageExtensions.Select(x=> x)}", ErrorCodes.IncorrectReceiptImageExtension);
        }

        public static ReceiptImage From(
            Stream imageStream, 
            string imageExtension,
            int imageWidth,
            int imageHeight) =>
            new ReceiptImage(imageStream, imageExtension, ReceiptDimensions.From(imageWidth, imageHeight));
    }
}
