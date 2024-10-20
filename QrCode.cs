using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
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
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        var content = req.Query["content"];
        if (string.IsNullOrEmpty(content))
            return req.CreateResponse(HttpStatusCode.NotFound);

        logger.LogInformation("Received generate qrcode with: {content}.", content);

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        using var svg = new SvgQRCode(data);

        var svgStr = svg.GetGraphic(24)
            .Replace("\n", string.Empty);

        logger.LogInformation("Response generated: SVG {svg}.", svgStr);

        var resp = req.CreateResponse(HttpStatusCode.OK);
        resp.Headers.Add("Content-Type", "image/svg+xml");
        resp.WriteString(svgStr);

        return resp;
    }
}