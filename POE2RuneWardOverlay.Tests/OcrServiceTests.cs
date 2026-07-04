using POE2RuneWardOverlay.Services;
using Xunit;

namespace POE2RuneWardOverlay.Tests;

public class OcrServiceTests
{
    [Theory]
    [InlineData("847/1200", 847, 1200)]
    [InlineData("1800/1200", 1800, 1200)]
    [InlineData("200/1200", 200, 1200)]
    [InlineData("0/800", 0, 800)]
    public void ParseWardText_ValidInput_ReturnsParsedValues(
        string input, int expectedCurrent, int expectedMax)
    {
        var result = OcrService.ParseWardText(input);

        Assert.NotNull(result);
        Assert.Equal(expectedCurrent, result.Value.Current);
        Assert.Equal(expectedMax, result.Value.Max);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("1200")]
    [InlineData("abc/def")]
    public void ParseWardText_InvalidInput_ReturnsNull(string input)
    {
        var result = OcrService.ParseWardText(input);
        Assert.Null(result);
    }

    [Fact]
    public void ParseWardText_ExtraWhitespace_ParsesCorrectly()
    {
        var result = OcrService.ParseWardText("  847 / 1200  ");

        Assert.NotNull(result);
        Assert.Equal(847, result.Value.Current);
        Assert.Equal(1200, result.Value.Max);
    }
}
