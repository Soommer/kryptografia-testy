using kryptografia.Algorithms;
using System.Threading.Tasks;
using System;
using Xunit;

public class VigenereCipherTests
{
    private readonly VigenereCipher _cipher = new();

    [Theory]
    [InlineData("ATTACKATDAWN", "LEMON", "LXFOPVEFRNHR")]
    [InlineData("HELLO", "KEY", "RIJVS")]
    [InlineData("hello", "abc", "hfnlp")]
    public async Task EncryptAsync_ShouldEncryptCorrectly(string input, string key, string expected)
    {
        var encrypted = await _cipher.EncryptAsync(input, key);
        Assert.Equal(expected, encrypted);
    }

    [Theory]
    [InlineData("LXFOPVEFRNHR", "LEMON", "ATTACKATDAWN")]
    [InlineData("RIJVS", "KEY", "HELLO")]
    [InlineData("hfnlp", "abc", "hello")]
    public async Task DecryptAsync_ShouldDecryptCorrectly(string input, string key, string expected)
    {
        var decrypted = await _cipher.DecryptAsync(input, key);
        Assert.Equal(expected, decrypted);
    }

    [Fact]
    public async Task EncryptAndDecrypt_ShouldReturnOriginalText()
    {
        string original = "sdawdaw wadwdas awdafre.";
        string key = "SECRET";

        var encrypted = await _cipher.EncryptAsync(original, key);
        var decrypted = await _cipher.DecryptAsync(encrypted, key);

        Assert.Equal(original, decrypted);
    }

    [Fact]
    public async Task Encrypt_WithInvalidKey_ShouldThrow()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _cipher.EncryptAsync("Test", ""));
    }

    [Fact]
    public async Task Decrypt_WithInvalidKey_ShouldThrow()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _cipher.DecryptAsync("Encrypted", null));
    }
}
