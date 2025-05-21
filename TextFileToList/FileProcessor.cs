using System;
using System.Collections.Generic;
using System.IO;

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
    }
}
