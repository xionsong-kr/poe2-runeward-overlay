using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using Tesseract;

namespace POE2RuneWardOverlay.Services;

public partial class OcrService : IDisposable
{
    private readonly TesseractEngine _engine;

    public OcrService(string tessDataPath)
    {
        _engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
        _engine.SetVariable("tessedit_char_whitelist", "0123456789/");
    }

    public (int Current, int Max)? ReadWard(Bitmap bitmap)
    {
        using var scaled = Scale(bitmap, 2);
        using var ms = new System.IO.MemoryStream();
        scaled.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        var bytes = ms.ToArray();
        using var pix = Pix.LoadFromMemory(bytes);
        using var page = _engine.Process(pix, PageSegMode.SingleLine);
        var text = page.GetText();
        return ParseWardText(text);
    }

    private static Bitmap Scale(Bitmap source, int factor)
    {
        var result = new Bitmap(source.Width * factor, source.Height * factor);
        using var g = Graphics.FromImage(result);
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.DrawImage(source, 0, 0, source.Width * factor, source.Height * factor);
        return result;
    }

    public static (int Current, int Max)? ParseWardText(string text)
    {
        var match = WardPattern().Match(text);
        if (!match.Success) return null;
        if (!int.TryParse(match.Groups[1].Value, out var current)) return null;
        if (!int.TryParse(match.Groups[2].Value, out var max) || max <= 0) return null;
        return (current, max);
    }

    [GeneratedRegex(@"(\d+)\s*/\s*(\d+)")]
    private static partial Regex WardPattern();

    public void Dispose() => _engine.Dispose();
}
