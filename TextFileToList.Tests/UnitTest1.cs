using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.IO;
using Xunit;

namespace TextFileToList.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void Main_ShouldHandleFileNotFound()
        {
            // Arrange
            string nonExistentFile = "nonexistent.txt";
            string originalFileName = "example.txt";
            string originalFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, originalFileName);

            // Temporarily rename the file to simulate file not found
            if (File.Exists(originalFilePath))
            {
                File.Move(originalFilePath, originalFilePath + ".bak");
            }

            // Act
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                Program.Main(new string[] { });

                // Assert
                string result = sw.ToString();
                Assert.Contains("File not found", result);
            }

            // Restore the original file
            if (File.Exists(originalFilePath + ".bak"))
            {
                File.Move(originalFilePath + ".bak", originalFilePath);
            }
        }
    }

    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {

        }
    }
}