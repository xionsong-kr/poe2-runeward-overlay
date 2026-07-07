using System.IO;
using System.Text.Json;

namespace POE2RuneWardOverlay.Models;

public class AppSettings
{
    public int WarningThresholdPercent { get; set; } = 30;
    public int MaxWardValue { get; set; } = 0;
    public int CaptureX { get; set; } = 20;
    public int CaptureY { get; set; } = 832;
    public int CaptureWidth { get; set; } = 220;
    public int CaptureHeight { get; set; } = 35;
    public double OverlayOpacity { get; set; } = 0.7;
    public double OverlayLeft { get; set; } = -1;
    public double OverlayTop { get; set; } = -1;
    public double OverlayScale { get; set; } = 1.0;
    public string OverlayLabelText { get; set; } = "룬수호";
    public bool ShowLabel { get; set; } = true;
    public string NormalBarColor { get; set; } = "#4696DC";
    public string DangerBarColor { get; set; } = "#DC3737";
    public string OverflowBarColor { get; set; } = "#FFCC44";

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public void Save(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(path, JsonSerializer.Serialize(this, JsonOptions));
    }

    public static AppSettings Load(string path)
    {
        if (!File.Exists(path)) return new AppSettings();
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
    }

    public static string DefaultPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "POE2RuneWardOverlay",
            "settings.json");
}
