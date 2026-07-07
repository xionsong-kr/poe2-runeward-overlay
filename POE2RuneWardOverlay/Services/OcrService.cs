using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Tesseract;

namespace POE2RuneWardOverlay.Services;

public partial class OcrService : IDisposable
{
    private readonly TesseractEngine _engine;

    public OcrService(string tessDataPath)
    {
        _engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
        _engine.SetVariable("tessedit_char_whitelist", "0123456789/,");
    }

    public static Action<string>? Logger { get; set; }

    public (int Current, int Max)? ReadWard(Bitmap bitmap)
    {
        using var processed = Preprocess(bitmap);
        using var ms = new System.IO.MemoryStream();
        processed.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        var bytes = ms.ToArray();
        using var pix = Pix.LoadFromMemory(bytes);
        using var page = _engine.Process(pix, PageSegMode.SingleLine);
        var text = page.GetText().Trim();
        return ParseWardText(text);
    }

    // 2x NearestNeighbor 스케일 후 흰색 픽셀만 남기고 나머지는 흰 배경으로 변환.
    // 수호 수치는 항상 흰색이므로 다른 색상의 UI 요소가 겹쳐도 제거됨.
    private static Bitmap Preprocess(Bitmap source)
    {
        int w = source.Width * 2;
        int h = source.Height * 2;
        var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(result))
        {
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(source, 0, 0, w, h);
        }

        var bits = result.LockBits(new Rectangle(0, 0, w, h),
            ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        int byteCount = Math.Abs(bits.Stride) * h;
        var pixels = new byte[byteCount];
        Marshal.Copy(bits.Scan0, pixels, 0, byteCount);

        for (int i = 0; i < byteCount; i += 4)
        {
            // BGRA 순서
            byte b = pixels[i], g2 = pixels[i + 1], r = pixels[i + 2];
            bool isWhite = r > 120 && g2 > 120 && b > 120;
            // 흰 글자 → 검정(0), 나머지 → 흰 배경(255): Tesseract는 흰 배경에 검정 글자 선호
            byte val = isWhite ? (byte)0 : (byte)255;
            pixels[i] = pixels[i + 1] = pixels[i + 2] = val;
            pixels[i + 3] = 255;
        }

        Marshal.Copy(pixels, 0, bits.Scan0, byteCount);
        result.UnlockBits(bits);
        return result;
    }

    public static (int Current, int Max)? ParseWardText(string text)
    {
        var match = WardPattern().Match(text);
        if (!match.Success) return null;
        var currentStr = match.Groups[1].Value.Replace(",", "");
        var maxStr = match.Groups[2].Value.Replace(",", "");
        if (!int.TryParse(currentStr, out var current)) return null;
        if (!int.TryParse(maxStr, out var max) || max <= 0) return null;
        return (current, max);
    }

    [GeneratedRegex(@"([\d,]+)\s*/\s*([\d,]+)")]
    private static partial Regex WardPattern();

    public void Dispose() => _engine.Dispose();
}
