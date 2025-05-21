using System;
using System.Collections.Generic;
using System.IO;

namespace TextFileToList
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            // Function to read a text file and convert it to a list of strings
            string fileName = "example.txt";

            // Write a function called GetFilePath that concats the filePath name with the directory of execution of the assembly
            string GetFilePath(string fileName)
            {
                string directory = AppDomain.CurrentDomain.BaseDirectory;
                return Path.Combine(directory, fileName);
            }

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
