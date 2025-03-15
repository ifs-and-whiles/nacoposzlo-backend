using System.IO;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;

namespace Billy.PolishReceiptRecognitionAlgorithm.Tests.Infrastructure
{
    class ReceiptJsonReader
    {
        public static T Read<T>(string jsonFilePath)
        {
            var embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());

            using var reader = embeddedProvider.GetFileInfo(jsonFilePath).CreateReadStream();
            using var streamReader = new StreamReader(reader);

            var content = streamReader.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
