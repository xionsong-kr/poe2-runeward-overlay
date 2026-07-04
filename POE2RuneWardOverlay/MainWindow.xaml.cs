using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
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

    public MainWindow()
    {
        InitializeComponent();
        SourceInitialized += (_, _) => SetClickThrough(true);
    }

    public void ToggleMoveMode()
    {
        _isMoveMode = !_isMoveMode;
        SetClickThrough(!_isMoveMode);
        RootBorder.BorderBrush = _isMoveMode
            ? new SolidColorBrush(Color.FromRgb(255, 200, 0))
            : null;
        RootBorder.BorderThickness = _isMoveMode ? new Thickness(2) : new Thickness(0);
        LabelText.Text = _isMoveMode ? "이동 중 (Ctrl+Shift+M)" : "룬수호";
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

        switch (vm.State)
        {
            case WardState.Normal:
                MainBar.Fill = new SolidColorBrush(Color.FromRgb(70, 150, 220));
                OverflowBar.Width = 0;
                ValueText.Foreground = Brushes.White;
                StopBlink();
                break;

            case WardState.Overflow:
                MainBar.Fill = new SolidColorBrush(Color.FromRgb(70, 150, 220));
                OverflowBar.Width = Math.Min(vm.OverflowRatio, 0.5) * OverflowMaxWidth * 2;
                ValueText.Foreground = new SolidColorBrush(Color.FromRgb(255, 204, 68));
                StopBlink();
                break;

            case WardState.Danger:
                MainBar.Fill = new SolidColorBrush(Color.FromRgb(220, 55, 55));
                OverflowBar.Width = 0;
                ValueText.Foreground = new SolidColorBrush(Color.FromRgb(220, 55, 55));
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
