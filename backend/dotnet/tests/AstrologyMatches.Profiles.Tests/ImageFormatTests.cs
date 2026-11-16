using System.Text;
using AstrologyMatches.Profiles.Domain;

namespace AstrologyMatches.Profiles.Tests;

public class ImageFormatTests
{
    [Fact]
    public void DetectContentType_JpegHeader_ReturnsJpeg()
    {
        byte[] header = { 0xFF, 0xD8, 0xFF, 0xE0, 0, 0, 0, 0, 0, 0, 0, 0 };

        Assert.Equal(ImageFormat.Jpeg, ImageFormat.DetectContentType(header));
    }

    [Fact]
    public void DetectContentType_PngHeader_ReturnsPng()
    {
        byte[] header = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0 };

        Assert.Equal(ImageFormat.Png, ImageFormat.DetectContentType(header));
    }

    [Fact]
    public void DetectContentType_WebpHeader_ReturnsWebp()
    {
        byte[] header = Encoding.ASCII.GetBytes("RIFF1234WEBP");

        Assert.Equal(ImageFormat.Webp, ImageFormat.DetectContentType(header));
    }

    [Fact]
    public void DetectContentType_TextFile_ReturnsNull()
    {
        byte[] header = Encoding.ASCII.GetBytes("<html>hello!");

        Assert.Null(ImageFormat.DetectContentType(header));
    }

    [Fact]
    public void DetectContentType_TooShortFile_ReturnsNull()
    {
        byte[] header = { 0xFF, 0xD8 };

        Assert.Null(ImageFormat.DetectContentType(header));
    }
}
