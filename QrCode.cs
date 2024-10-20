using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using QRCoder;

namespace UtilFunctions;

public class QrCode(ILogger<QrCode> logger)
{
    [Function("QrCode")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var content = req.Query["content"];
        if (string.IsNullOrEmpty(content))
            return new NotFoundResult();

        logger.LogInformation("Received generate qrcode with: {content}.", content!);

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(content!, QRCodeGenerator.ECCLevel.Q);
        using var svg = new SvgQRCode(data);

        var svgStr = svg.GetGraphic(24)
            .Replace("\n", string.Empty);

        logger.LogInformation("Response generated: SVG length {len}.", svgStr.Length);

        return new ContentResult()
        {
            Content = svgStr,
            ContentType = "image/svg+xml"
        };
    }
}