using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minio;
using Minio.DataModel;
using Newtonsoft.Json;

namespace Billy.Receipts.Storage.Minio
{
    public interface IMinioStorage
    {
        Task<List<MinioDirectory>> ListDirectoriesFor(string relativePath);
        Task<List<MinioFile>> ListFilesFor(MinioDirectory minioDirectory);
        Task<List<MinioFile>> ListFilesFor(string minioDirectoryPath);
        Task<List<MinioFile>> ListRecursiveFilesFor(MinioDirectory rootMinioDirectory);
        Task<List<MinioFile>> ListRecursiveFilesFor(string rootMinioDirectoryPath);
        Task<List<MinioFile>> ListRecursiveFilesFor(MinioDirectory rootMinioDirectory, string searchPattern);
        Task<List<MinioFile>> ListRecursiveFilesFor(string rootMinioDirectoryPath, string searchPattern);
        Task<byte[]> ReadBinaryFile(string filePath);
        Task<Stream> ReadStreamFile(string filePath);
        Task<string> ReadStringFile(string filePath);
        Task<string> ReadStringFile(MinioFile file);
        Task<TObject> ReadJsonFile<TObject>(MinioFile file);
        Task<TObject> ReadJsonFile<TObject>(string filePath);
        Task<List<TObject>> ReadJsonFiles<TObject>(List<string> paths);
        Task<List<TObject>> ReadJsonFiles<TObject>(List<MinioFile> files);
        Task Save(byte[] fileContent, string relativePath, string fileName, 
            string contentType);

        Task Save(string fileContent, string bucketName, string fileName,
            string contentType);

        Task Save(string fileContent, string fullPath, string contentType = "application/octet-stream");

        Task Save(byte[] fileContent, string fullPath, string contentType = "application/octet-stream");

        Task Save(Stream fileStream, string fullPath, string contentType = "application/octet-stream");

        Task<string> Download(string minioFileKey, string targetPath);

        Task ClearBucket();

        Task RemoveBucket();

        Task<bool> BucketExists();

        Task<bool> FileExists(string fullPath);

        string Endpoint { get; }

        string BucketName { get; }

    }
    public class MinioStorage : IMinioStorage
    {
        private readonly MinioStorageConfig _config;
        private readonly MinioClient _minioClient;

        public MinioStorage(MinioStorageConfig config)
        {
            _config = config;
            _minioClient = new MinioClient(_config.Endpoint, _config.AccessKey, _config.SecretKey);
            if (_config.Ssl)
            {
                _minioClient = _minioClient.WithSSL();
            }

            
        }

        public async Task<bool> FileExists(string fullPath)
        {
            try
            {
                var objectStat = await _minioClient
                    .StatObjectAsync(BucketName, fullPath)
                    .ConfigureAwait(false);
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        public string Endpoint => _config.Endpoint;

        public string BucketName => _config.BucketName;

        public async Task<List<MinioDirectory>> ListDirectoriesFor(string relativePath) =>
            await ListDirectories(relativePath);
        
        public async Task<List<MinioFile>> ListFilesFor(MinioDirectory minioDirectory) =>
            await ListFilesInDirectory(minioDirectory.FullPath);

        public async Task<List<MinioFile>> ListFilesFor(string minioDirectoryPath) =>
            await ListFilesInDirectory(minioDirectoryPath);

        public async Task<List<MinioFile>> ListRecursiveFilesFor(MinioDirectory rootMinioDirectory) =>
            await ListFilesInDirectory(rootMinioDirectory.FullPath, recursive: true);

        public async Task<List<MinioFile>> ListRecursiveFilesFor(string rootMinioDirectoryPath) =>
            await ListFilesInDirectory(rootMinioDirectoryPath, recursive: true);

        public async Task<List<MinioFile>> ListRecursiveFilesFor(MinioDirectory rootMinioDirectory, string searchPattern) =>
            await ListFilesInDirectory(rootMinioDirectory.FullPath, searchPattern, recursive: true);

        public async Task<List<MinioFile>> ListRecursiveFilesFor(string rootMinioDirectoryPath, string searchPattern) =>
            await ListFilesInDirectory(rootMinioDirectoryPath, searchPattern, recursive: true);

        public async Task<byte[]> ReadBinaryFile(string filePath) =>
            await ReadBytes(filePath);

        public async Task<Stream> ReadStreamFile(string filePath) =>
            await ReadStream(filePath);
        
        public async Task<string> ReadStringFile(string filePath)
        {
            var fileContent = await ReadBytes(filePath);
            return Encoding.Default.GetString(fileContent);
        }


        public async Task<string> ReadStringFile(MinioFile file)
        {
            var fileContent = await ReadBytes(file.FullPath);
            return Encoding.Default.GetString(fileContent);
        }
      

        public async Task<TObject> ReadJsonFile<TObject>(MinioFile file)
        {
            var fileContent = await ReadBytes(file.FullPath);
            return JsonConvert.DeserializeObject<TObject>(Encoding.Default.GetString(fileContent));
        }

        public async Task<TObject> ReadJsonFile<TObject>(string filePath)
        {
            var fileContent = await ReadBytes(filePath);
            return JsonConvert.DeserializeObject<TObject>(Encoding.Default.GetString(fileContent));
        }

        public async Task<List<TObject>> ReadJsonFiles<TObject>(params string[] paths)
        {
            return await ReadJsonFiles<TObject>(paths.ToList());
        }

        public async Task<List<TObject>> ReadJsonFiles<TObject>(List<string> paths)
        {
            var jsonFiles = new List<TObject>();
            foreach (var filePath in paths)
            {
                var fileContent = await ReadBytes(filePath);
                jsonFiles.Add(JsonConvert.DeserializeObject<TObject>(Encoding.Default.GetString(fileContent)));
            }
            return jsonFiles;
        }

        public async Task<List<TObject>> ReadJsonFiles<TObject>(List<MinioFile> files)
        {
            var jsonFiles = new List<TObject>();
            foreach (var file in files)
            {
                var fileContent = await ReadBytes(file.FullPath);
                jsonFiles.Add(JsonConvert.DeserializeObject<TObject>(Encoding.Default.GetString(fileContent)));
            }
            return jsonFiles;
        }

        public async Task Save(byte[] fileContent, string fullPath, string contentType = "application/octet-stream")
        {
            await CreateBucketIfDoesNotExist(_minioClient, _config.BucketName);

            using var fileStream = new MemoryStream(fileContent);

            await _minioClient.PutObjectAsync(_config.BucketName, fullPath, fileStream, fileStream.Length, contentType);
        }

        public async Task Save(Stream fileStream, string fullPath, string contentType = "application/octet-stream")
        {
            await CreateBucketIfDoesNotExist(_minioClient, _config.BucketName);

            await _minioClient.PutObjectAsync(_config.BucketName, fullPath, fileStream, fileStream.Length, contentType);
        }

        public async Task Save(byte[] fileContent, string relativePath, string fileName, string contentType = "application/octet-stream")
        {
            var fullPath = $"{relativePath}/{fileName}";

            await Save(fileContent, fullPath, contentType);
        }

        public async Task Save(string fileContent, string relativePath, string fileName, string contentType = "application/octet-stream")
        {
            await Save(Encoding.ASCII.GetBytes(fileContent), relativePath, fileName, contentType);
        }
        public async Task Save(string fileContent, string fullPath, string contentType = "application/octet-stream")
        {
            await Save(Encoding.ASCII.GetBytes(fileContent), fullPath, contentType);
        }

        public async Task RemoveBucket()
        {
            await ClearBucket();
            await _minioClient.RemoveBucketAsync(_config.BucketName);
        }

        public async Task ClearBucket()
        {
            var bucketExists = await _minioClient.BucketExistsAsync(_config.BucketName);
            if (bucketExists)
            {
                var existingElementsInsideBucket = await ListObjects(recursive: true);
                await (await _minioClient
                        .RemoveObjectAsync(_config.BucketName, existingElementsInsideBucket.Select(x => x.Key).ToList()))
                    .ToAsyncEnumerable()
                    .ToListAsync();
            }
        }

        public async Task<bool> BucketExists() => await _minioClient.BucketExistsAsync(_config.BucketName);

        private async Task<byte[]> ReadBytes(string minioFileKey)
        {
            byte[] fileContent = null;
            await _minioClient.GetObjectAsync(_config.BucketName, minioFileKey, (stream) =>
            {
                using var memoryStream = new MemoryStream();

                stream.CopyTo(memoryStream);

                fileContent = memoryStream.ToArray();

            });

            if (!fileContent.Any())
                throw new InvalidOperationException($"File content has not been downloaded for {minioFileKey}");

            return fileContent;
        }

        private async Task<Stream> ReadStream(string minioFileKey)
        {
            var fileStream = new MemoryStream();
            
            await _minioClient.GetObjectAsync(_config.BucketName, minioFileKey, (stream) =>
            {
                stream.CopyTo(fileStream);
               
            });

            return fileStream;
        }

        private async Task<List<Item>> ListObjects(string relativePath = null, bool recursive = false)
        {
            return await _minioClient
                    .ListObjectsAsync(_config.BucketName, relativePath, recursive)
                    .ToAsyncEnumerable()
                    .ToListAsync();
        }
        
        private async Task<List<MinioDirectory>> ListDirectories(string relativePath, bool recursive = false)
        {
            return (await ListObjects(relativePath, recursive))
                .OnlyDirectories()
                .ToMinioDirectory()
                .ToList();
        }

        private async Task<List<MinioFile>> ListFilesInDirectory(string directoryPath, bool recursive = false)
        {
            return (await ListObjects(directoryPath, recursive))
                .OnlyFiles()
                .ToMinioFile()
                .ToList();
        }

        private async Task<List<MinioFile>> ListFilesInDirectory(string directoryPath, string fileName, bool recursive = false)
        {
            return (await ListObjects(directoryPath, recursive))
                .OnlyFiles()
                .WithName(fileName)
                .ToMinioFile()
                .ToList();
        }


        private async Task CreateBucketIfDoesNotExist(global::Minio.MinioClient minioClient, string bucketName)
        {
            var bucketExists = await minioClient.BucketExistsAsync(bucketName);
            if (!bucketExists)
                await minioClient.MakeBucketAsync(bucketName);
        }

        public async Task<string> Download(string minioFileKey, string targetPath)
        {
            var fileName = Path.GetFileName(minioFileKey);
            var filePath = Path.Combine(targetPath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await _minioClient.GetObjectAsync(_config.BucketName, minioFileKey, (stream) => stream.CopyTo(fileStream));
            }

            return filePath;
        }
    }
    

    public class MinioDirectory
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
    }

    public class MinioFile
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public string Extension { get; set; }
    }
    internal static class MinioExtensions
    {
        public static IEnumerable<MinioDirectory> ToMinioDirectory(this IEnumerable<Item> directories)
        {
            foreach (var directory in directories)
            {
                yield return new MinioDirectory()
                {
                    FullPath = directory.Key,
                    Name = new DirectoryInfo(directory.Key).Name,
                };
            }
        }

        public static IEnumerable<MinioFile> ToMinioFile(this IEnumerable<Item> files)
        {
            foreach (var file in files)
            {
                yield return new MinioFile()
                {
                    FullPath = file.Key,
                    Name = Path.GetFileNameWithoutExtension(file.Key),
                    Extension = Path.GetExtension(file.Key)
                };
            }
        }

        public static IEnumerable<Item> OnlyDirectories(this IEnumerable<Item> items)
        {
            return items.Where(x => x.IsDir);
        }

        public static IEnumerable<Item> OnlyFiles(this IEnumerable<Item> items)
        {
            return items.Where(x => !x.IsDir);
        }

        public static IEnumerable<Item> WithName(this IEnumerable<Item> items, string searchPattern)
        {
            return items.Where(x=> x.Key.Contains(searchPattern));
        }
    }
}
