using kryptografia.Algorithms;
using System.Threading.Tasks;
using System;
using Xunit;

public class CaesarCipherTests
{
	private readonly CaesarCipher _cipher = new();

	[Theory]
	[InlineData("HELLO", "3", "KHOOR")]
	[InlineData("abc", "1", "bcd")]
	[InlineData("XYZ", "2", "ZAB")]
	public async Task EncryptAsync_ShouldEncryptCorrectly(string input, string key, string expected)
	{
		var encrypted = await _cipher.EncryptAsync(input, key);
		Assert.Equal(expected, encrypted);
	}

	[Theory]
	[InlineData("KHOOR", "3", "HELLO")]
	[InlineData("bcd", "1", "abc")]
	[InlineData("ZAB", "2", "XYZ")]
	public async Task DecryptAsync_ShouldDecryptCorrectly(string input, string key, string expected)
	{
		var decrypted = await _cipher.DecryptAsync(input, key);
		Assert.Equal(expected, decrypted);
	}

	[Fact]
	public async Task EncryptAndDecrypt_ShouldReturnOriginalText()
	{
		string original = "dawdsdasf fawfasd afwsfawf.";
		string key = "5";

		var encrypted = await _cipher.EncryptAsync(original, key);
		var decrypted = await _cipher.DecryptAsync(encrypted, key);

		Assert.Equal(original, decrypted);
	}

	[Fact]
	public async Task Encrypt_WithInvalidKey_ShouldThrow()
	{
		await Assert.ThrowsAsync<ArgumentException>(() => _cipher.EncryptAsync("Test", "notanumber"));
	}

	[Fact]
	public async Task Decrypt_WithInvalidKey_ShouldThrow()
	{
		await Assert.ThrowsAsync<ArgumentException>(() => _cipher.DecryptAsync("Test", "!@#"));
	}
}
