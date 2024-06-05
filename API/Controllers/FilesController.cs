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
}
