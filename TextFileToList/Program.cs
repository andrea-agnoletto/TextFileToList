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

            // Refactored to use the new FileProcessor class
            FileProcessor.ProcessFile("example.txt");
        }
    } 
}
