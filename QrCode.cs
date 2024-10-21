using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using QRCoder;

namespace UtilFunctions;

public class QrCode(ILogger<QrCode> logger)
{
    /// <summary>
    /// 生成二维码
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Function("QrCode")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        StringValues content, format;

        content = req.Query[nameof(content)];
        if (StringValues.IsNullOrEmpty(content))
            return new NotFoundResult();

        format = req.Query[nameof(format)];
        if (StringValues.IsNullOrEmpty(format))
            format = "html";

        logger.LogInformation("Received generate qrcode with: {content}.", content!);

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(content!, QRCodeGenerator.ECCLevel.Q);
        using var svgQrCode = new SvgQRCode(data);

        var svg = svgQrCode.GetGraphic(12).Replace("\n", string.Empty);

        logger.LogInformation("Response generated, length {len}.", svg.Length);

        return new ContentResult()
        {
            Content = svg,
            ContentType = "image/svg+xml"
        };
    }
}