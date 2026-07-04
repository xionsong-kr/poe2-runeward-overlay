using System.Drawing;
using System.Runtime.InteropServices;

namespace POE2RuneWardOverlay.Services;

public class ScreenCaptureService
{
    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(
        IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
        IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    private const int SRCCOPY = 0x00CC0020;

    public Bitmap Capture(int x, int y, int width, int height)
    {
        var bitmap = new Bitmap(width, height);
        using var g = Graphics.FromImage(bitmap);
        var hdcDest = g.GetHdc();
        var desktop = GetDesktopWindow();
        var hdcSrc = GetDC(desktop);
        try
        {
            BitBlt(hdcDest, 0, 0, width, height, hdcSrc, x, y, SRCCOPY);
        }
        finally
        {
            g.ReleaseHdc(hdcDest);
            ReleaseDC(desktop, hdcSrc);
        }
        return bitmap;
    }
}
