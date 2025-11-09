using Microsoft.AspNetCore.Mvc;

namespace LoginServer.Controllers;

[ApiController]
[Route("api/files")]
public class ApiFileController: ControllerBase
{
    private readonly string _fileDirectory = Path.Combine(Directory.GetCurrentDirectory(), "DownloadFiles");
    [HttpGet("{*fileName}")]
    public IActionResult DownloadFile(string fileName)
    {
        // 计算文件的完整路径
        var filePath = Path.GetFullPath(Path.Combine(_fileDirectory, fileName));

        // 确保请求的文件在 _fileDirectory 目录下，防止目录遍历攻击
        if (!filePath.StartsWith(_fileDirectory) || !System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        var contentType = "application/octet-stream";
        return File(fileBytes, contentType, Path.GetFileName(filePath));
    }
}