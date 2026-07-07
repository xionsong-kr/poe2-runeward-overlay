using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using POE2RuneWardOverlay.Models;
using POE2RuneWardOverlay.ViewModels;

namespace POE2RuneWardOverlay;

public partial class MainWindow : Window
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_LAYERED = 0x00080000;

    [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr hwnd, int index);
    [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr hwnd, int index, int style);

    private const double BarMaxWidth = 144.0;
    private const double OverflowMaxWidth = 45.0;
    private bool _isMoveMode;
    private DoubleAnimation? _blinkAnim;
    private readonly AppSettings _settings;

    public MainWindow(AppSettings settings)
    {
        _settings = settings;
        InitializeComponent();
        SourceInitialized += (_, _) => SetClickThrough(true);
    }

    public void ApplyScale(double scale)
    {
        OverlayScale.ScaleX = scale;
        OverlayScale.ScaleY = scale;
    }

    private static Color ParseColor(string hex, Color fallback)
    {
        try { return (Color)ColorConverter.ConvertFromString(hex); }
        catch { return fallback; }
    }

    public void ToggleMoveMode()
    {
        _isMoveMode = !_isMoveMode;
        SetClickThrough(!_isMoveMode);
        RootBorder.BorderBrush = _isMoveMode
            ? new SolidColorBrush(Color.FromRgb(255, 200, 0))
            : null;
        RootBorder.BorderThickness = _isMoveMode ? new Thickness(2) : new Thickness(0);
        if (_isMoveMode)
        {
            LabelText.Visibility = Visibility.Visible;
            LabelText.Text = "이동 중 (Ctrl+Shift+M)";
        }
        else
        {
            ApplyLabelSettings();
        }
    }

    public void ApplyLabelSettings()
    {
        LabelText.Visibility = _settings.ShowLabel ? Visibility.Visible : Visibility.Collapsed;
        LabelText.Text = _settings.OverlayLabelText;
    }

    private void SetClickThrough(bool enabled)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        var style = GetWindowLong(hwnd, GWL_EXSTYLE);
        if (enabled)
            SetWindowLong(hwnd, GWL_EXSTYLE, style | WS_EX_TRANSPARENT | WS_EX_LAYERED);
        else
            SetWindowLong(hwnd, GWL_EXSTYLE, style & ~WS_EX_TRANSPARENT);
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_isMoveMode) DragMove();
    }

    public void ApplyViewModel(OverlayViewModel vm)
    {
        ValueText.Text = vm.DisplayText;
        MainBar.Width = vm.BarFillRatio * BarMaxWidth;

        var normalColor  = ParseColor(_settings.NormalBarColor,   Color.FromRgb(70, 150, 220));
        var dangerColor  = ParseColor(_settings.DangerBarColor,   Color.FromRgb(220, 55, 55));
        var overflowColor = ParseColor(_settings.OverflowBarColor, Color.FromRgb(255, 204, 68));

        switch (vm.State)
        {
            case WardState.Normal:
                MainBar.Fill = new SolidColorBrush(normalColor);
                OverflowBar.Width = 0;
                ValueText.Foreground = Brushes.White;
                StopBlink();
                break;

            case WardState.Overflow:
                MainBar.Fill = new SolidColorBrush(normalColor);
                OverflowBar.Width = Math.Min(vm.OverflowRatio, 0.5) * OverflowMaxWidth * 2;
                OverflowBar.Fill = new SolidColorBrush(overflowColor);
                ValueText.Foreground = new SolidColorBrush(overflowColor);
                StopBlink();
                break;

            case WardState.Danger:
                MainBar.Fill = new SolidColorBrush(dangerColor);
                OverflowBar.Width = 0;
                ValueText.Foreground = new SolidColorBrush(dangerColor);
                StartBlink();
                break;
        }
    }

    private void StartBlink()
    {
        if (_blinkAnim != null) return;
        _blinkAnim = new DoubleAnimation(1.0, 0.3, TimeSpan.FromMilliseconds(500))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever
        };
        MainBar.BeginAnimation(OpacityProperty, _blinkAnim);
    }

    private void StopBlink()
    {
        if (_blinkAnim == null) return;
        MainBar.BeginAnimation(OpacityProperty, null);
        MainBar.Opacity = 1;
        _blinkAnim = null;
    }
}
