using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace POE2RuneWardOverlay;

public partial class CaptureAreaWindow : Window
{
    private const int ButtonAreaWidth = 88;
    private const int LabelRowHeight = 22;

    public int ResultX => (int)Math.Round(Left);
    public int ResultY => (int)Math.Round(Top + LabelRowHeight);
    public int ResultW => (int)Math.Round(Width - ButtonAreaWidth);
    public int ResultH => (int)Math.Round(Height - LabelRowHeight);

    public CaptureAreaWindow(int x, int y, int w, int h)
    {
        InitializeComponent();
        Left = x;
        Top = Math.Max(0, y - LabelRowHeight);
        Width = w + ButtonAreaWidth;
        Height = h + LabelRowHeight;
        UpdateLabel();
    }

    private void OnDrag(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }

    private void OnResizeDelta(object sender, DragDeltaEventArgs e)
    {
        Width = Math.Max(80 + ButtonAreaWidth, Width + e.HorizontalChange);
        Height = Math.Max(LabelRowHeight + 30, Height + e.VerticalChange);
    }

    private void OnConfirm(object sender, RoutedEventArgs e) => Close();

    protected override void OnLocationChanged(EventArgs e)
    {
        base.OnLocationChanged(e);
        UpdateLabel();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo info)
    {
        base.OnRenderSizeChanged(info);
        UpdateLabel();
    }

    private void UpdateLabel() =>
        CoordLabel.Text = $" X={ResultX}  Y={ResultY}  W={ResultW}  H={ResultH} ";
}
