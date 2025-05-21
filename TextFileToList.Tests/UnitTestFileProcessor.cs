using System;
using System.IO;
using Xunit;

namespace TextFileToList.Tests
{
    public class UnitTestFileProcessor
    {
        [Fact]
        public void ProcessFile_ShouldPrintFileContent_WhenFileExists()
        {
            // Arrange
            string testFileName = "testfile.txt";
            string testFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, testFileName);
            File.WriteAllText(testFilePath, "Line1\nLine2\nLine3");

            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                // Act
                FileProcessor.ProcessFile(testFileName);

                // Assert
                string result = sw.ToString();
                Assert.Contains("Line1", result);
                Assert.Contains("Line2", result);
                Assert.Contains("Line3", result);
            }

            // Cleanup
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
        }

        [Fact]
        public void ProcessFile_ShouldPrintFileNotFound_WhenFileDoesNotExist()
        {
            // Arrange
            string nonExistentFile = "nonexistent.txt";

            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                // Act
                FileProcessor.ProcessFile(nonExistentFile);

                // Assert
                string result = sw.ToString();
                Assert.Contains("File not found", result);
            }
        }
    }
}
