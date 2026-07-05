using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net.Mime;

namespace Silnith.CDB.Service.Controllers;

[ApiController]
[Route("CDB")]
public class CdbController : ControllerBase
{
    private readonly ILogger<CdbController> logger;

    private readonly ICDB dataStore;

    public CdbController(ILogger<CdbController> logger,
        ICDB dataStore)
    {
        this.logger = logger;
        this.dataStore = dataStore;
    }

    [HttpGet("{**fileNameAndPath}")]
    public IActionResult Get(string fileNameAndPath)
    {
        const string Message = $"{nameof(Get)}({{FileNameAndPath}})";
        using var _ = logger.BeginScope(Message, fileNameAndPath);

        using MemoryStream memoryStream = new();
        if (dataStore.TryReadFile(fileNameAndPath, stream =>
        {
            stream.CopyTo(memoryStream);
        }))
        {
            byte[] content = memoryStream.ToArray();
            logger.LogDebug("File found.  {Size}", content.LongLength);

            string filename = Path.GetFileName(fileNameAndPath);
            // application/gml+xml
            // application/gltf-buffer
            // application/geo+json
            // image/tiff; application=geotiff
            string contentType = Path.GetExtension(fileNameAndPath).ToLowerInvariant() switch
            {
                ".bmp" => "image/bmp",
                ".dbf" => "application/vnd.dbf",
                //".dbt" => "application/vnd.dbt",
                ".flt" => "model/flt",
                ".gif" => MediaTypeNames.Image.Gif,
                ".gpkg" => "application/geopackage+sqlite3",
                ".glb" => "model/gltf-binary",
                ".gltf" => "model/gltf+json",
                ".jpg" or ".jpeg" => MediaTypeNames.Image.Jpeg,
                ".jp2" or ".jpg2" => "image/jp2",
                ".jsn" or ".json" => MediaTypeNames.Application.Json,
                ".png" => "image/png",
                ".rgb" or ".rgba" => "image/sgi",
                ".shp" => "application/vnd.shp",
                ".shx" => "application/vnd.shp.shx",
                ".tif" or ".tiff" => MediaTypeNames.Image.Tiff,
                ".xml" or ".xsd" => MediaTypeNames.Application.Xml,
                ".zip" => MediaTypeNames.Application.Zip,
                _ => MediaTypeNames.Application.Octet,
            };
            return File(content, contentType, filename);
        }
        else
        {
            logger.LogDebug("File not found.");

            return NotFound();
        }
    }
}
