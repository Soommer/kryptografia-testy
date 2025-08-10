using kryptografia.Algorithms;
using Xunit;
using System;
using System.Threading.Tasks;

public class AesEncryptionTests
{
    private readonly AesEncryption _aes = new();

    [Fact]
    public async Task EncryptAndDecrypt_ShouldReturnOriginalText()
    {
        string original = "Secret message for AES";
        string key = "mysecurekey123";

        var encrypted = await _aes.EncryptAsync(original, key);
        var decrypted = await _aes.DecryptAsync(encrypted, key);

        Assert.Equal(original, decrypted);
    }

    [Fact]
    public async Task Encrypt_WithEmptyKey_ShouldThrow()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _aes.EncryptAsync("data", ""));
    }

    [Fact]
    public async Task Decrypt_WithEmptyKey_ShouldThrow()
    {
        var encrypted = await _aes.EncryptAsync("test", "validkey");
        await Assert.ThrowsAsync<ArgumentException>(() => _aes.DecryptAsync(encrypted, ""));
    }

    [Fact]
    public async Task Decrypt_WithWrongKey_ShouldThrowOrReturnGarbage()
    {
        string original = "sensitive info";
        string key1 = "correctkey";
        string key2 = "wrongkey";

        var encrypted = await _aes.EncryptAsync(original, key1);

        var ex = await Record.ExceptionAsync(() => _aes.DecryptAsync(encrypted, key2));
        Assert.NotNull(ex);
    }
}
