using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Timers;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using System.Threading;

namespace TextFileToList
{
    public static class FileProcessor
    {
        public static string GetFilePath(string fileName)
        {
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(directory, fileName);
        }

        public static void ProcessFile(string fileName)
        {
            string filePath = GetFilePath(fileName);
            if (File.Exists(filePath))
            {
                List<string> lines = new List<string>(File.ReadAllLines(filePath));
                Console.WriteLine("File content as a list of strings:");
                foreach (string line in lines)
                {
                    Console.WriteLine(line);
                }
            }
            else
            {
                Console.WriteLine("File not found: " + filePath);
            }
        }

        public static void CompressFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file does not exist.", filePath);
            }

            string zipFilePath = filePath + ".zip";

            using (FileStream originalFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (FileStream zipFileStream = new FileStream(zipFilePath, FileMode.Create))
            using (var zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
            {
                var entry = zipArchive.CreateEntry(Path.GetFileName(filePath));

                using (var entryStream = entry.Open())
                {
                    byte[] buffer = new byte[8192];
                    long totalBytes = originalFileStream.Length;
                    long bytesProcessed = 0;

                    System.Timers.Timer timer = new System.Timers.Timer(1000); // 1 second interval
                    timer.Elapsed += (sender, e) =>
                    {
                        double percentage = (double)bytesProcessed / totalBytes * 100;
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write($"Compression progress: {percentage:F2}%");
                    };
                    timer.Start();

                    int bytesRead;
                    while ((bytesRead = originalFileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        entryStream.Write(buffer, 0, bytesRead);
                        bytesProcessed += bytesRead;
                    }

                    timer.Stop();
                }
            }

            Console.WriteLine("\nCompression completed.");
        }

        public static void SimpleCompressFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file does not exist.", filePath);
            }

            string zipFilePath = filePath + ".zip";

            using (FileStream originalFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (FileStream zipFileStream = new FileStream(zipFilePath, FileMode.Create))
            using (var zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
            {
                var entry = zipArchive.CreateEntry(Path.GetFileName(filePath));

                using (var entryStream = entry.Open())
                {
                    originalFileStream.CopyTo(entryStream);
                }
            }

            Console.WriteLine("File successfully compressed to: " + zipFilePath);
        }

        public static async Task UploadFile(string filePath, string endpointUri)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file does not exist.", filePath);
            }

            if (string.IsNullOrWhiteSpace(endpointUri))
            {
                throw new ArgumentException("Endpoint URI cannot be null or empty.", nameof(endpointUri));
            }

            using (HttpClient client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            using (var multipartContent = new MultipartFormDataContent())
            {
                // Add the file content
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    multipartContent.Add(fileContent, "file", Path.GetFileName(filePath));
                }

                // Add the JSON object
                var jsonObject = new { filePath };
                var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(jsonObject), System.Text.Encoding.UTF8, "application/json");
                multipartContent.Add(jsonContent, "metadata");

                Console.WriteLine($"Uploading file '{filePath}' to '{endpointUri}' with HTTP compression...");

                HttpResponseMessage response = await client.PostAsync(endpointUri, multipartContent);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("File uploaded successfully.");
                }
                else
                {
                    Console.WriteLine($"File upload failed. Status code: {response.StatusCode}");
                }
            }
        }

        private static readonly SemaphoreSlim uploadSemaphore = new SemaphoreSlim(1, 1);

        public static async Task UploadFileWithLock(string filePath, string endpointUri)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file does not exist.", filePath);
            }

            if (string.IsNullOrWhiteSpace(endpointUri))
            {
                throw new ArgumentException("Endpoint URI cannot be null or empty.", nameof(endpointUri));
            }

            await uploadSemaphore.WaitAsync();
            try
            {
                using (HttpClient client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
                using (var multipartContent = new MultipartFormDataContent())
                {
                    // Add the file content
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        var fileContent = new StreamContent(fileStream);
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        multipartContent.Add(fileContent, "file", Path.GetFileName(filePath));
                    }

                    // Add the JSON object
                    var jsonObject = new { filePath };
                    var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(jsonObject), System.Text.Encoding.UTF8, "application/json");
                    multipartContent.Add(jsonContent, "metadata");

                    Console.WriteLine($"Uploading file '{filePath}' to '{endpointUri}' with HTTP compression inside a semaphore lock...");

                    HttpResponseMessage response = await client.PostAsync(endpointUri, multipartContent);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("File uploaded successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"File upload failed. Status code: {response.StatusCode}");
                    }
                }
            }
            finally
            {
                uploadSemaphore.Release();
            }
        }

        public static List<string> GetFoldersFromPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            string? directoryPath = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directoryPath))
            {
                throw new ArgumentException("The provided path does not contain any folders.", nameof(filePath));
            }

            return directoryPath.Split(Path.DirectorySeparatorChar).ToList();
        }

        public static int? GetTier(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new ArgumentException("Folder path cannot be null or empty.", nameof(folderPath));
            }

            string fullPath = Path.GetFullPath(folderPath);
            string[] subfolders = fullPath.Split(Path.DirectorySeparatorChar);

            foreach (string subfolder in subfolders)
            {
                if (int.TryParse(subfolder, out int tier))
                {
                    return tier;
                }
            }

            return null; // Return null if no subfolder can be casted to an integer
        }

        public static async Task<bool> VerifyFileUpload(string baseUri, string relativeFilePath)
        {
            if (string.IsNullOrWhiteSpace(baseUri))
            {
                throw new ArgumentException("Base URI cannot be null or empty.", nameof(baseUri));
            }

            if (string.IsNullOrWhiteSpace(relativeFilePath))
            {
                throw new ArgumentException("Relative file path cannot be null or empty.", nameof(relativeFilePath));
            }

            string requestUri = $"{baseUri.TrimEnd('/')}/api/FileUpload/IsFilePresent?relativeFilePath={Uri.EscapeDataString(relativeFilePath)}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
