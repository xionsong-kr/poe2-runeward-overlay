using POE2RuneWardOverlay.Models;
using POE2RuneWardOverlay.ViewModels;
using Xunit;

namespace POE2RuneWardOverlay.Tests;

public class OverlayViewModelTests
{
    private readonly AppSettings _settings = new() { WarningThresholdPercent = 30 };

    [Fact]
    public void Update_NormalState_BarFillRatioCorrect()
    {
        var vm = new OverlayViewModel(_settings);
        vm.Update(600, 1200);

        Assert.Equal(0.5, vm.BarFillRatio);
        Assert.Equal(0.0, vm.OverflowRatio);
        Assert.Equal(WardState.Normal, vm.State);
    }

    [Fact]
    public void Update_OverflowState_OverflowRatioCorrect()
    {
        var vm = new OverlayViewModel(_settings);
        vm.Update(1800, 1200); // 1.5x

        Assert.Equal(1.0, vm.BarFillRatio);
        Assert.Equal(0.5, vm.OverflowRatio, precision: 5);
        Assert.Equal(WardState.Overflow, vm.State);
    }

    [Fact]
    public void Update_DangerState_BelowThreshold()
    {
        var vm = new OverlayViewModel(_settings);
        vm.Update(300, 1200); // 25% < 30%

        Assert.Equal(WardState.Danger, vm.State);
    }

    [Fact]
    public void Update_ExactThreshold_IsNormal()
    {
        var vm = new OverlayViewModel(_settings);
        vm.Update(360, 1200); // 정확히 30%

        Assert.Equal(WardState.Normal, vm.State);
    }

    [Fact]
    public void DisplayText_ShowsCurrentSlashMax()
    {
        var vm = new OverlayViewModel(_settings);
        vm.Update(847, 1200);

        Assert.Equal("847/1200", vm.DisplayText);
    }
}
