using kryptografia.Algorithms;
using Xunit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System;

public class RsaEncryptionTests
{
	private readonly RsaEncryption _rsa = new();

	private (string PublicKey, string PrivateKey) GenerateKeyPair()
	{
		using var rsa = RSA.Create(2048);
		return (
			rsa.ToXmlString(false), 
			rsa.ToXmlString(true)  
		);
	}

	[Fact]
	public async Task EncryptAndDecrypt_ShouldReturnOriginalText()
	{
		string original = "RSA test message";
		var keys = GenerateKeyPair();

		var encrypted = await _rsa.EncryptAsync(original, keys.PublicKey);
		var decrypted = await _rsa.DecryptAsync(encrypted, keys.PrivateKey);

		Assert.Equal(original, decrypted);
	}

	[Fact]
	public async Task Encrypt_WithMissingPublicKey_ShouldThrow()
	{
		await Assert.ThrowsAsync<ArgumentException>(() => _rsa.EncryptAsync("test", null));
	}

	[Fact]
	public async Task Decrypt_WithMissingPrivateKey_ShouldThrow()
	{
		await Assert.ThrowsAsync<ArgumentException>(() => _rsa.DecryptAsync("encrypted", ""));
	}

	[Fact]
	public async Task Decrypt_WithWrongPrivateKey_ShouldThrow()
	{
		string original = "Sensitive RSA data";
		var keys1 = GenerateKeyPair();
		var keys2 = GenerateKeyPair(); 

		var encrypted = await _rsa.EncryptAsync(original, keys1.PublicKey);

		await Assert.ThrowsAsync<CryptographicException>(() => _rsa.DecryptAsync(encrypted, keys2.PrivateKey));
	}
}
