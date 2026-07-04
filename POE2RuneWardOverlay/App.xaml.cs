using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using POE2RuneWardOverlay.Models;
using POE2RuneWardOverlay.Services;
using POE2RuneWardOverlay.ViewModels;

namespace POE2RuneWardOverlay;

public partial class App : Application
{
    private MainWindow? _overlay;
    private SettingsWindow? _settingsWindow;
    private AppSettings _settings = new();
    private OcrService? _ocr;
    private readonly ScreenCaptureService _capture = new();
    private OverlayViewModel? _viewModel;
    private DispatcherTimer? _timer;
    private TaskbarIcon? _trayIcon;
    private int _lastKnownMax = 0;
    private int _lastAccepted = -1;
    private int _pendingCurrent = -1;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _trayIcon = (TaskbarIcon)FindResource("TrayIcon");

        _settings = AppSettings.Load(AppSettings.DefaultPath);
        _ocr = new OcrService("tessdata");
        _viewModel = new OverlayViewModel(_settings);

        _overlay = new MainWindow();
        PositionOverlay();
        _overlay.Show();

        // Ctrl+Shift+M 전역 단축키 (게임 포커스 중에도 작동)
        var helper = new WindowInteropHelper(_overlay);
        helper.EnsureHandle();
        _overlayHandle = helper.Handle;
        var hwndSource = HwndSource.FromHwnd(_overlayHandle);
        hwndSource?.AddHook(WndProc);
        RegisterHotKey(_overlayHandle, HotkeyId, MOD_CONTROL | MOD_SHIFT, VK_M);

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _timer.Tick += OnTick;
        _timer.Start();

        _settingsWindow = new SettingsWindow(_settings, SaveSettings);
        _settingsWindow.Show();
    }

    private void PositionOverlay()
    {
        if (_settings.OverlayLeft < 0)
        {
            _overlay!.Left = (SystemParameters.PrimaryScreenWidth - 350) / 2;
            _overlay.Top = SystemParameters.PrimaryScreenHeight / 2 + 100;
        }
        else
        {
            _overlay!.Left = _settings.OverlayLeft;
            _overlay.Top = _settings.OverlayTop;
        }
    }

    private void OnTick(object? sender, EventArgs e)
    {
        try
        {
            using var bitmap = _capture.Capture(
                _settings.CaptureX, _settings.CaptureY,
                _settings.CaptureWidth, _settings.CaptureHeight);

            var result = _ocr!.ReadWard(bitmap);
            if (result is null) return;

            var (current, max) = result.Value;

            // max는 장비/레벨업 외엔 안 바뀌므로 캐시 유지
            if (max > 0) _lastKnownMax = max;
            int effectiveMax = _lastKnownMax > 0 ? _lastKnownMax : max;

            // 5자리 이상(10000+)은 OCR 오인식
            if (current >= 10000 || max >= 10000) return;

            // 설정된 최대치가 있으면 ×1.5 초과는 오인식으로 무시
            if (_settings.MaxWardValue > 0 &&
                current > (int)Math.Ceiling(_settings.MaxWardValue * 1.5)) return;

            // 자릿수가 2자리 이상 짧으면 partial read로 간주하고 무시
            if (effectiveMax > 0 &&
                current.ToString().Length < effectiveMax.ToString().Length - 1) return;

            // 직전 값 대비 25% 초과 급변 → 다음 틱에서 재확인 후 반영
            if (_lastAccepted >= 0 &&
                Math.Abs(current - _lastAccepted) > effectiveMax * 0.40)
            {
                if (current != _pendingCurrent)
                {
                    _pendingCurrent = current;
                    return; // 이번 틱은 보류
                }
                // 2틱 연속 같은 값이면 실제 변화로 인정
            }
            _pendingCurrent = -1;
            _lastAccepted = current;

            _viewModel!.Update(current, effectiveMax);
            _overlay!.ApplyViewModel(_viewModel);
        }
        catch { /* OCR 실패 시 마지막 표시값 유지 */ }
    }

    [DllImport("user32.dll")] static extern bool RegisterHotKey(IntPtr hWnd, int id, uint mods, uint vk);
    [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int HotkeyId = 9000;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint VK_M = 0x4D;
    private IntPtr _overlayHandle;

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == 0x0312 && wParam.ToInt32() == HotkeyId) // WM_HOTKEY
        {
            _overlay?.ToggleMoveMode();
            handled = true;
        }
        return IntPtr.Zero;
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        if (_settingsWindow is { IsVisible: true })
        {
            _settingsWindow.Activate();
            return;
        }
        _settingsWindow = new SettingsWindow(_settings, SaveSettings);
        _settingsWindow.Show();
    }

    private void SaveSettings()
    {
        _settings.OverlayLeft = _overlay!.Left;
        _settings.OverlayTop = _overlay.Top;
        _settings.Save(AppSettings.DefaultPath);
    }

    private void OnExitClick(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _timer?.Stop();
        if (_overlayHandle != IntPtr.Zero) UnregisterHotKey(_overlayHandle, HotkeyId);
        SaveSettings();
        _ocr?.Dispose();
        _trayIcon?.Dispose();
        base.OnExit(e);
    }
}
