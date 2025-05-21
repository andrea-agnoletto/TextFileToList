using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;

namespace TextFileToList
{
    public static class ExcelProcessor
    {
        public static void ConvertToExcel(List<string> data, string filePath)
        {
            if (data == null || data.Count == 0)
            {
                throw new ArgumentException("The data list cannot be null or empty.", nameof(data));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("The file path cannot be null or empty.", nameof(filePath));
            }

            // Ensure the directory exists
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create and save the Excel file
            ExcelPackage.License.SetNonCommercialPersonal("Andrea"); 
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                for (int i = 0; i < data.Count; i++)
                {
                    worksheet.Cells[i + 1, 1].Value = data[i];
                }

                package.SaveAs(new FileInfo(filePath));
            }
        }
    }
}
