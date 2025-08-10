using kryptografia.Algorithms;
using Xunit;
using System.Threading.Tasks;
using System;

public class Sha256HashTests
{
    private readonly Sha256Hash _hash = new();

    [Fact]
    public async Task EncryptAsync_SameInputSameHash()
    {
        string input = "password123";

        var hash1 = await _hash.EncryptAsync(input);
        var hash2 = await _hash.EncryptAsync(input);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public async Task EncryptAsync_DifferentInputDifferentHash()
    {
        string input1 = "password123";
        string input2 = "password124";

        var hash1 = await _hash.EncryptAsync(input1);
        var hash2 = await _hash.EncryptAsync(input2);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public async Task DecryptAsync_ShouldThrowNotSupportedException()
    {
        await Assert.ThrowsAsync<NotSupportedException>(() => _hash.DecryptAsync("somehash"));
    }
}
