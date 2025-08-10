using kryptografia.Algorithms;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System;

public class SteganographyEncoderTests
{
    private readonly SteganographyEncoder _encoder = new();

    private Stream CreateTestImage(int width, int height)
    {
        var image = new Image<Rgba32>(width, height);
        using var tempStream = new MemoryStream();
        image.SaveAsPng(tempStream);
        tempStream.Seek(0, SeekOrigin.Begin);
        return new MemoryStream(tempStream.ToArray());
    }

    [Fact]
    public async Task EmbedMessageAsync_ShouldEmbedWithoutError()
    {
        string message = "Test message";
        using var imageStream = CreateTestImage(50, 50); 

        var result = await _encoder.EmbedMessageAsync(imageStream, message);

        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    [Fact]
    public async Task EmbedMessageAsync_WithTooShortImage_ShouldThrow()
    {
        string message = new string('A', 5000); 
        using var imageStream = CreateTestImage(10, 10);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _encoder.EmbedMessageAsync(imageStream, message));
    }

    [Fact]
    public async Task EmbedMessageAsync_OutputShouldBeImage()
    {
        string message = "hidden";
        using var imageStream = CreateTestImage(40, 40);

        var embeddedBytes = await _encoder.EmbedMessageAsync(imageStream, message);

        using var image = SixLabors.ImageSharp.Image.Load(embeddedBytes);
        Assert.Equal(40, image.Width);
        Assert.Equal(40, image.Height);
    }
}
