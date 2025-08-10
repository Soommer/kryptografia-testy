using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using kryptografia.Controllers;
using kryptografia.Services;
using kryptografia.Models;
using System.Threading.Tasks;
using System;

public class EncryptionControllerTests
{
    private readonly Mock<IEncryptionService> _serviceMock = new();
    private readonly Mock<ILogger<EncryptionController>> _loggerMock = new();

    private EncryptionController CreateController() => new(_serviceMock.Object, _loggerMock.Object);

    [Fact]
    public async Task Encrypt_ReturnsEncryptedResult()
    {
        var controller = CreateController();
        var request = new EncryptionRequest
        {
            PlainText = "hello",
            Algorithm = "caesar",
            Key = "3"
        };

        _serviceMock
            .Setup(s => s.EncryptAsync("hello", "caesar", "3"))
            .ReturnsAsync("khoor");

        var result = await controller.Encrypt(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<EncryptionResponse>(okResult.Value);
        Assert.Equal("khoor", response.CipherText);
    }

    [Fact]
    public async Task Decrypt_ReturnsDecryptedResult()
    {
        var controller = CreateController();
        var request = new EncryptionRequest
        {
            PlainText = "khoor",
            Algorithm = "caesar",
            Key = "3"
        };

        _serviceMock
            .Setup(s => s.DecryptAsync("khoor", "caesar", "3"))
            .ReturnsAsync("hello");

        var result = await controller.Decrypt(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<EncryptionResponse>(okResult.Value);
        Assert.Equal("hello", response.CipherText);
    }

    [Fact]
    public async Task Encrypt_WhenExceptionThrown_ReturnsBadRequest()
    {
        var controller = CreateController();
        var request = new EncryptionRequest { PlainText = "", Algorithm = "caesar", Key = "3" };

        _serviceMock
            .Setup(s => s.EncryptAsync("", "caesar", "3"))
            .ThrowsAsync(new ArgumentException("Invalid input"));

        var result = await controller.Encrypt(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("Invalid input", badRequest.Value.ToString());
    }

    [Fact]
    public async Task Encrypt_WhenMessageTooLong_ReturnsBadRequest()
    {
        var controller = CreateController();
        var longText = new string('A', 10001);

        var request = new EncryptionRequest
        {
            PlainText = longText,
            Algorithm = "caesar",
            Key = "3"
        };

        var result = await controller.Encrypt(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("maximum allowed length", badRequest.Value.ToString());
    }

    [Fact]
    public async Task Decrypt_WhenMessageTooLong_ReturnsBadRequest()
    {
        var controller = CreateController();
        var longText = new string('B', 10001);

        var request = new EncryptionRequest
        {
            PlainText = longText,
            Algorithm = "caesar",
            Key = "3"
        };

        var result = await controller.Decrypt(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("maximum allowed length", badRequest.Value.ToString());
    }

    [Fact]
    public async Task GenerateRsaKeyPair_ReturnsKeys()
    {
        var controller = CreateController();
        var result = await controller.GenerateRsaKeyPair();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var keyPair = Assert.IsType<RsaKeyPairResponse>(okResult.Value);

        Assert.Contains("<RSAKeyValue>", keyPair.PublicKeyXml);
        Assert.Contains("<RSAKeyValue>", keyPair.PrivateKeyXml);
    }
}
