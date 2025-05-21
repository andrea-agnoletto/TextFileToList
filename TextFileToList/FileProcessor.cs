using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Timers;

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

                    Timer timer = new Timer(1000); // 1 second interval
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
    }
}
