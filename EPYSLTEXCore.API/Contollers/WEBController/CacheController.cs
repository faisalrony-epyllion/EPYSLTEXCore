using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;

public class CacheController : Controller
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CacheController(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    [ResponseCache(Duration = 2592000, Location = ResponseCacheLocation.Client, VaryByQueryKeys = new string[] { })]
    public IActionResult CacheEj2MinJS()
    {
        // Combine the root path with the relative path to the file
        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "App_Themes", "lib", "syncfusion", "ej2.min.js");

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("File not found.");
        }

        var contentType = "application/javascript"; // Correct MIME type
        return PhysicalFile(filePath, contentType);
    }
}
