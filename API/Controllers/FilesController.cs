using API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace API;

public class FilesController : BaseApiController
{
  private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider;

  public FilesController(FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
  {
    _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider ?? throw new ArgumentNullException(nameof(fileExtensionContentTypeProvider));
  }

  [HttpGet("{fileId}")]
  public ActionResult GetFile(string fileId)
  {
    // look up the actual file, depending on the fileId
    // demo code
    var pathToFile = "getting-started-with-rest-slides.pdf";

    // check whether the file exists
    if (!System.IO.File.Exists(pathToFile))
    {
      return NotFound();
    }

    if (!_fileExtensionContentTypeProvider.TryGetContentType(pathToFile, out var contentType))
    {
      contentType = "application/octet-stream";
    }

    // return the file
    var bytes = System.IO.File.ReadAllBytes(pathToFile);
    return File(bytes, contentType, Path.GetFileName(pathToFile));

  }

  [HttpPost]
  public async Task<ActionResult> UploadFile(IFormFile file)
  {
    // Validate the input. Put a limit on file size to avoid large uploads attacks.
    // Only accept .pdf files (check content type)
    System.Console.WriteLine(file.Length);
    System.Console.WriteLine(10 * 1024 * 1024);
    if (file.Length == 0 || file.Length > 10 * 1024 * 1048 || file.ContentType != "application/pdf")
    {
      return BadRequest("No file or an invalid one has been inputted");
    }

    // Create the file path. Avoid using file. Filename , as an attacker can provide a
    // malicious one, including full paths or relative paths.
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), $"uploaded_file_{Guid.NewGuid()}.pdf");

    using (var stream = new FileStream(filePath, FileMode.Create))
    {
      await file.CopyToAsync(stream);
    }

    return Ok("Your file has been uploaded successfully");
  }
}
