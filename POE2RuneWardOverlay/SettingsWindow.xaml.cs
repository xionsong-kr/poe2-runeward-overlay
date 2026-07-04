using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using POE2RuneWardOverlay.Models;

namespace POE2RuneWardOverlay;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;
    private readonly Action _onSave;

    public SettingsWindow(AppSettings settings, Action onSave)
    {
        InitializeComponent();
        _settings = settings;
        _onSave = onSave;
        LoadValues();
    }

    private void LoadValues()
    {
        ThresholdSlider.Value = _settings.WarningThresholdPercent;
        ThresholdLabel.Text = $"{_settings.WarningThresholdPercent}%";
        MaxWard.Text = _settings.MaxWardValue.ToString();
        CaptureX.Text = _settings.CaptureX.ToString();
        CaptureY.Text = _settings.CaptureY.ToString();
        CaptureW.Text = _settings.CaptureWidth.ToString();
        CaptureH.Text = _settings.CaptureHeight.ToString();
    }

    private void OnThresholdChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ThresholdLabel is null) return;
        ThresholdLabel.Text = $"{(int)ThresholdSlider.Value}%";
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        _settings.WarningThresholdPercent = (int)ThresholdSlider.Value;
        if (int.TryParse(MaxWard.Text, out var mw)) _settings.MaxWardValue = mw;
        if (int.TryParse(CaptureX.Text, out var x)) _settings.CaptureX = x;
        if (int.TryParse(CaptureY.Text, out var y)) _settings.CaptureY = y;
        if (int.TryParse(CaptureW.Text, out var w)) _settings.CaptureWidth = w;
        if (int.TryParse(CaptureH.Text, out var h)) _settings.CaptureHeight = h;
        _onSave();
        Close();
    }

    private void OnPreview(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(CaptureX.Text, out var x)) x = _settings.CaptureX;
        if (!int.TryParse(CaptureY.Text, out var y)) y = _settings.CaptureY;
        if (!int.TryParse(CaptureW.Text, out var w)) w = _settings.CaptureWidth;
        if (!int.TryParse(CaptureH.Text, out var h)) h = _settings.CaptureHeight;

        var preview = new Window
        {
            Left = x, Top = y, Width = w, Height = h,
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = Brushes.Transparent,
            Topmost = true,
            ShowInTaskbar = false,
            IsHitTestVisible = false,
            Content = new System.Windows.Controls.Border
            {
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(2),
                Background = new SolidColorBrush(Color.FromArgb(40, 255, 0, 0))
            }
        };
        preview.Show();

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        timer.Tick += (_, _) => { timer.Stop(); preview.Close(); };
        timer.Start();
    }

    private void OnCancel(object sender, RoutedEventArgs e) => Close();
}
