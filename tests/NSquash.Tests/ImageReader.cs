using NSquash;

namespace NSquash.Tests;

public class ImageReader
{
    [Theory]
    [InlineData("testdata/rootfs.img")]
    public async Task ReadImageAsync(string path)
    {
        using var stream = File.OpenRead(path);
        await FileSystem.ImportFromStream(stream);
    }
}