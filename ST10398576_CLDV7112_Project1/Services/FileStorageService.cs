using Azure.Storage.Files.Shares;
using ST10398576_CLDV7112_Project1.Models;
using System.Text;

namespace ST10398576_CLDV7112_Project1.Services
{
    public interface IFileStorageService
    {
        Task WriteLogAsync(LogEntry entry);
        Task<List<string>> ListLogFileNamesAsync();
        Task<string> ReadLogAsync(string fileName);
    }

    public class FileStorageService : IFileStorageService
    {
        private readonly ShareDirectoryClient? _rootDirectory;
        private readonly bool _initialized;

        public FileStorageService(IConfiguration config)
        {
            try
            {
                string? connectionString = config.GetValue<string>("AzureStorage:ConnectionString");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    _initialized = false;
                    return;
                }

                string shareName = "logfiles";

                var shareClient = new ShareClient(connectionString, shareName);
                shareClient.CreateIfNotExists();

                _rootDirectory = shareClient.GetRootDirectoryClient();
                _initialized = true;
            }
            catch
            {
                _initialized = false;
            }
        }

        public async Task WriteLogAsync(LogEntry entry)
        {
            if (!_initialized || _rootDirectory == null) return;

            string fileName = $"{entry.Action}_{entry.Timestamp:yyyyMMdd_HHmmssfff}.txt";
            var fileClient = _rootDirectory.GetFileClient(fileName);

            byte[] content = Encoding.UTF8.GetBytes(entry.ToString());
            using var stream = new MemoryStream(content);

            await fileClient.CreateAsync(content.Length);
            await fileClient.UploadRangeAsync(
                new Azure.HttpRange(0, content.Length),
                stream);
        }

        public async Task<List<string>> ListLogFileNamesAsync()
        {
            var names = new List<string>();
            if (!_initialized || _rootDirectory == null) return names;

            await foreach (var item in _rootDirectory.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    names.Add(item.Name);
                }
            }
            return names;
        }

        public async Task<string> ReadLogAsync(string fileName)
        {
            if (!_initialized || _rootDirectory == null) return string.Empty;

            var fileClient = _rootDirectory.GetFileClient(fileName);
            var download = await fileClient.DownloadAsync();

            using var reader = new StreamReader(download.Value.Content);
            return await reader.ReadToEndAsync();
        }
    }
}