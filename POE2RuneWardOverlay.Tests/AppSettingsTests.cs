using System.IO;
using POE2RuneWardOverlay.Models;
using Xunit;

namespace POE2RuneWardOverlay.Tests;

public class AppSettingsTests
{
    [Fact]
    public void SaveAndLoad_RoundTrip_PreservesValues()
    {
        var path = Path.GetTempFileName();
        var settings = new AppSettings
        {
            WarningThresholdPercent = 25,
            CaptureX = 10,
            CaptureY = 950,
            CaptureWidth = 200,
            CaptureHeight = 80,
            OverlayOpacity = 0.75,
            OverlayLeft = 800.0,
            OverlayTop = 640.0
        };

        settings.Save(path);
        var loaded = AppSettings.Load(path);

        Assert.Equal(25, loaded.WarningThresholdPercent);
        Assert.Equal(10, loaded.CaptureX);
        Assert.Equal(950, loaded.CaptureY);
        Assert.Equal(200, loaded.CaptureWidth);
        Assert.Equal(80, loaded.CaptureHeight);
        Assert.Equal(0.75, loaded.OverlayOpacity);
        Assert.Equal(800.0, loaded.OverlayLeft);
        Assert.Equal(640.0, loaded.OverlayTop);
    }

    [Fact]
    public void Load_MissingFile_ReturnsDefaults()
    {
        var settings = AppSettings.Load("nonexistent_xyz_file.json");

        Assert.Equal(30, settings.WarningThresholdPercent);
        Assert.Equal(0.7, settings.OverlayOpacity);
    }
}
