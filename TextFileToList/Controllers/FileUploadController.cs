using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace TextFileToList.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> UploadFile()
        {
            if (!Request.HasFormContentType || !Request.Form.Files.Any())
            {
                return BadRequest("Invalid request. No file or form data found.");
            }

            var form = Request.Form;

            // Extract the file
            var file = form.Files["file"];
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is missing or empty.");
            }

            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            // Extract the JSON metadata
            var metadata = form["metadata"];
            if (string.IsNullOrWhiteSpace(metadata))
            {
                return BadRequest("Metadata is missing.");
            }

            /// <summary>
            /// Deserialize the metadata JSON and extract the filePath property.
            /// </summary>
            string? filePath = null;
            if (!string.IsNullOrWhiteSpace(metadata))
            {
                // Ensure metadata is not null before deserialization to avoid possible null reference errors
                /// <summary>
                /// Deserialize the metadata JSON and extract the filePath property.
                /// </summary>
                var metadataString = metadata.ToString();
                var metadataObj = System.Text.Json.JsonSerializer.Deserialize<dynamic>(metadataString);
                filePath = metadataObj?.filePath;
            }
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return BadRequest("File path is missing in metadata.");
            }

            // Process the file and metadata as needed
            // For example, save the file to a directory or log the file path

            return Ok(new { Message = "File uploaded successfully.", FilePath = filePath });
        }
    }
}
