using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using QRCoder;

namespace UtilFunctions;

public class QrCode(ILogger<QrCode> logger)
{
    [Function("QrCode")]
    public HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var content = req.Query["content"];
        if (StringValues.IsNullOrEmpty(content))
            return new(HttpStatusCode.NotFound);

        logger.LogInformation("Received generate qrcode with: {content}.", content!);

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(content!, QRCodeGenerator.ECCLevel.Q);
        using var svg = new SvgQRCode(data);

        var svgStr = svg.GetGraphic(24)
            .Replace("\n", string.Empty);

        logger.LogInformation("Response generated: SVG {svg}.", svgStr);

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>QrCode</title></head><body>{svgStr}</body></html>")
            {
                Headers =
                {
                    ContentType = new("text/html")
                }
            }
        };

        return response;
    }
}