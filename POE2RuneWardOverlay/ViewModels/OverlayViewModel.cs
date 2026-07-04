using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using POE2RuneWardOverlay.Models;

namespace POE2RuneWardOverlay.ViewModels;

public enum WardState { Normal, Overflow, Danger }

public class OverlayViewModel : INotifyPropertyChanged
{
    private readonly AppSettings _settings;
    private double _barFillRatio;
    private double _overflowRatio;
    private WardState _state;
    private string _displayText = "---";

    public OverlayViewModel(AppSettings settings) => _settings = settings;

    public double BarFillRatio { get => _barFillRatio; private set => Set(ref _barFillRatio, value); }
    public double OverflowRatio { get => _overflowRatio; private set => Set(ref _overflowRatio, value); }
    public WardState State { get => _state; private set => Set(ref _state, value); }
    public string DisplayText { get => _displayText; private set => Set(ref _displayText, value); }

    public void Update(int current, int max)
    {
        if (max <= 0) return;

        DisplayText = $"{current}";
        var ratio = (double)current / max;

        if (current > max)
        {
            BarFillRatio = 1.0;
            OverflowRatio = (double)(current - max) / max;
            State = WardState.Overflow;
        }
        else
        {
            BarFillRatio = ratio;
            OverflowRatio = 0.0;
            State = ratio < _settings.WarningThresholdPercent / 100.0
                ? WardState.Danger
                : WardState.Normal;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
