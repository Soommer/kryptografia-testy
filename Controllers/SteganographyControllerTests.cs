using kryptografia.Controllers;
using kryptografia.Algorithms;
using kryptografia.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class SteganographyControllerTests
{
    private readonly Mock<ISteganography> _stegMock = new();
    private readonly Mock<ISteganographyDecoder> _decoderMock = new();
    private readonly Mock<ILogger<SteganographyController>> _loggerMock = new();

    private SteganographyController CreateController() => new(
        _stegMock.Object,
        _decoderMock.Object,
        _loggerMock.Object
    );

    private IFormFile CreateFormFileFromText(string content, string fileName = "image.png")
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "image", fileName);
    }
    private static void WithHttpContext(ControllerBase c)
    {
        c.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task Embed_WithValidRequest_ReturnsPngFile()
    {
        var controller = CreateController();
        WithHttpContext(controller); 

        var request = new SteganographyEmbedRequest
        {
            Message = "hidden",
            Image = CreateFormFileFromText("fake-image-data")
        };

        _stegMock
            .Setup(e => e.EmbedMessageAsync(It.IsAny<Stream>(), "hidden"))
            .ReturnsAsync(Encoding.UTF8.GetBytes("image-output"));

        var result = await controller.Embed(request);

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("image/png", fileResult.ContentType);
        Assert.Equal("steganography.png", fileResult.FileDownloadName);
        Assert.NotEmpty(fileResult.FileContents);
    }


    [Fact]
    public async Task Embed_WithMissingImageOrMessage_ReturnsBadRequest()
    {
        var controller = CreateController();
        var request = new SteganographyEmbedRequest { Message = null, Image = null };

        var result = await controller.Embed(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var obj = JObject.FromObject(badRequest.Value!);
        Assert.Equal("Image and message are required.", (string)obj["error"]!);
    }

    [Fact]
    public async Task Extract_WithValidImage_ReturnsCipherTextInEnvelope()
    {
        var controller = CreateController();

        var formFile = CreateFormFileFromText("image-data");
        var request = new SteganographyExtractRequest { Image = formFile };

        _decoderMock
            .Setup(d => d.ExtractMessageAsync(It.IsAny<Stream>()))
            .ReturnsAsync("extracted");

        var result = await controller.Extract(request);

        var ok = Assert.IsType<OkObjectResult>(result);
        var obj = JObject.FromObject(ok.Value!);

        string cipher =
            (string?)obj["cipherText"] ??
            (string?)obj["CipherText"];

        Assert.Equal("extracted", cipher);

        var metrics = obj["metrics"] ?? obj["Metrics"];
        Assert.NotNull(metrics);
    }


    [Fact]
    public async Task Extract_WithMissingImage_ReturnsBadRequest()
    {
        var controller = CreateController();
        var request = new SteganographyExtractRequest { Image = null };

        var result = await controller.Extract(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var obj = JObject.FromObject(badRequest.Value!);
        Assert.Equal("Image is required.", (string)obj["error"]!);
    }

    [Fact]
    public async Task EmbedImage_WithHiddenLargerThanHost_ReturnsBadRequest()
    {
        var controller = CreateController();
        var host = CreateFormFileFromText(new string('A', 100), "host.png");      // 100B
        var hidden = CreateFormFileFromText(new string('B', 200), "hidden.png");  // 200B

        var request = new SteganographyImageEmbedRequest
        {
            HostImage = host,
            HiddenImage = hidden
        };

        var result = await controller.EmbedImage(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var obj = JObject.FromObject(badRequest.Value!);
        Assert.Equal("Hidden image must not be larger than host image.", (string)obj["error"]!);
    }

    [Fact]
    public async Task EmbedImage_WithValidImages_ReturnsPngFile()
    {
        var controller = CreateController();
        WithHttpContext(controller); 

        var host = CreateFormFileFromText(new string('A', 200), "host.png");
        var hidden = CreateFormFileFromText(new string('B', 100), "hidden.png");

        _stegMock
            .Setup(s => s.EmbedImageAsync(It.IsAny<Stream>(), It.IsAny<Stream>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes("png-binary"));

        var result = await controller.EmbedImage(new SteganographyImageEmbedRequest
        {
            HostImage = host,
            HiddenImage = hidden
        });

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("image/png", file.ContentType);
        Assert.Equal("hidden-image.png", file.FileDownloadName);
        Assert.NotEmpty(file.FileContents);
    }


    [Fact]
    public async Task ExtractImage_WithMissingImage_ReturnsBadRequest()
    {
        var controller = CreateController();
        var result = await controller.ExtractImage(new SteganographyImageExtractRequest { Image = null });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var obj = JObject.FromObject(badRequest.Value!);
        Assert.Equal("Image is required.", (string)obj["error"]!);
    }
}
