using kryptografia.Algorithms;
using Xunit;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System;

public class SteganographyDecoderTests
{
    private readonly SteganographyEncoder _encoder = new();
    private readonly SteganographyDecoder _decoder = new();

    private Stream CreateTestImage(int width, int height)
    {
        var image = new Image<Rgba32>(width, height);
        using var tempStream = new MemoryStream();
        image.SaveAsPng(tempStream);
        tempStream.Seek(0, SeekOrigin.Begin);
        return new MemoryStream(tempStream.ToArray());
    }

    [Fact]
    public async Task ExtractMessageAsync_ShouldReturnEmbeddedMessage()
    {
        string message = "This is hidden";
        using var originalImageStream = CreateTestImage(50, 50);

        var embeddedImage = await _encoder.EmbedMessageAsync(originalImageStream, message);
        using var embeddedStream = new MemoryStream(embeddedImage);

        var result = await _decoder.ExtractMessageAsync(embeddedStream);

        Assert.Equal(message, result);
    }

    [Fact]
    public async Task ExtractMessageAsync_InvalidData_ShouldThrow()
    {
        using var imageStream = CreateTestImage(20, 20); 

        await Assert.ThrowsAsync<InvalidOperationException>(() => _decoder.ExtractMessageAsync(imageStream));
    }
}
