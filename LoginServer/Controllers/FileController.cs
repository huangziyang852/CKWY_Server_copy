using Microsoft.AspNetCore.Mvc;

namespace LoginServer.Controllers
{
    [Route("files")]
    public class FileController:Controller
    {

        private readonly string _filesPath = Path.Combine(Directory.GetCurrentDirectory(), "DownloadFiles", "apk");

        [HttpGet("apk")]
        public IActionResult Index()
        {
            var files = Directory.GetFiles(_filesPath)
                .Select(path => Path.GetFileName(path))
                .ToList();

            return View(files); // 返回 Razor View
        }

        [HttpGet("download/{fileName}")]
        public IActionResult Download(string fileName)
        {
            var filePath = Path.Combine(_filesPath, fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = "application/octet-stream";
            return PhysicalFile(filePath, contentType, fileName);
        }

    }
}
