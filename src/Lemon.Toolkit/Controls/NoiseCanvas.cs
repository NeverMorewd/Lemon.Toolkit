using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Lemon.Toolkit.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lemon.Toolkit.Controls;

public class NoiseCanvas : Control
{
    // Dependency properties
    public static readonly StyledProperty<int> NoiseCountProperty =
        AvaloniaProperty.Register<NoiseCanvas, int>(nameof(NoiseCount), 100);

    public static readonly StyledProperty<double> NoiseSizeProperty =
        AvaloniaProperty.Register<NoiseCanvas, double>(nameof(NoiseSize), 1.0);

    public static readonly StyledProperty<IBrush> NoiseColorProperty =
        AvaloniaProperty.Register<NoiseCanvas, IBrush>(nameof(NoiseColor), Brushes.White);

    public static readonly StyledProperty<TimeSpan> RefreshIntervalProperty =
        AvaloniaProperty.Register<NoiseCanvas, TimeSpan>(nameof(RefreshInterval), TimeSpan.FromMilliseconds(500));

    private readonly DispatcherTimer _refreshTimer;
    private readonly Random _random = new();
    private List<Point> _noisePoints = [];

    public NoiseCanvas()
    {
        _refreshTimer = new DispatcherTimer { Interval = RefreshInterval };
        _refreshTimer.Tick += (s, _) => RegenerateNoise();
        _refreshTimer.Start();
    }

    public int NoiseCount
    {
        get => GetValue(NoiseCountProperty);
        set => SetValue(NoiseCountProperty, value);
    }

    public double NoiseSize
    {
        get => GetValue(NoiseSizeProperty);
        set => SetValue(NoiseSizeProperty, value);
    }

    public IBrush NoiseColor
    {
        get => GetValue(NoiseColorProperty);
        set => SetValue(NoiseColorProperty, value);
    }

    public TimeSpan RefreshInterval
    {
        get => GetValue(RefreshIntervalProperty);
        set
        {
            SetValue(RefreshIntervalProperty, value);
            _refreshTimer.Interval = value;
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        RegenerateNoise();
    }
    private void RegenerateNoise()
    {
        if (Width <= 0 || Height <= 0)
            return;

        _noisePoints = Enumerable.Range(0, NoiseCount)
            .Select(_ => new Point(_random.NextDouble(0.01, 0.99, 2) * Width, _random.NextDouble(0.01, 0.99, 2) * Height))
            .ToList();

        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (_noisePoints.Count == 0)
            return;
        foreach (var point in _noisePoints)
        {
            context.DrawRectangle(NoiseColor, null, new Rect(point.X, point.Y, NoiseSize, NoiseSize));
        }
    }
}
