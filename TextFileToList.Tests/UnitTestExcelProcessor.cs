using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using Xunit;

namespace TextFileToList.Tests
{
    public class UnitTestExcelProcessor
    {
        [Fact]
        public void ConvertToExcel_ShouldCreateExcelFile_WhenDataIsValid()
        {
            // Arrange
            var data = new List<string> { "Row1", "Row2", "Row3" };
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.xlsx");

            // Act
            ExcelProcessor.ConvertToExcel(data, filePath);

            // Assert
            Assert.True(File.Exists(filePath), "Excel file was not created.");

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                Assert.Equal("Row1", worksheet.Cells[1, 1].Text);
                Assert.Equal("Row2", worksheet.Cells[2, 1].Text);
                Assert.Equal("Row3", worksheet.Cells[3, 1].Text);
            }

            // Cleanup
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public void ConvertToExcel_ShouldThrowException_WhenDataIsNull()
        {
            // Arrange
            List<string> data = null;
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.xlsx");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => ExcelProcessor.ConvertToExcel(data, filePath));
        }

        [Fact]
        public void ConvertToExcel_ShouldThrowException_WhenFilePathIsInvalid()
        {
            // Arrange
            var data = new List<string> { "Row1", "Row2" };
            string filePath = ""; // Invalid file path

            // Act & Assert
            Assert.Throws<ArgumentException>(() => ExcelProcessor.ConvertToExcel(data, filePath));
        }
    }
}
