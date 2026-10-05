using System.Globalization;
using System.Windows;
using System.Windows.Media;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.ElevationProfile.Wpf;

public sealed class ProfileChart : FrameworkElement
{
    private ElevationProfile3D _profile = new();

    public ProfileChart()
    {
        ClipToBounds = true;
        SizeChanged += (_, _) => InvalidateVisual();
    }

    public ElevationProfile3D Profile
    {
        set { _profile = value; InvalidateVisual(); }
    }

    protected override void OnRender(DrawingContext drawing)
    {
        base.OnRender(drawing);
        drawing.DrawRectangle(Brushes.White, null, new Rect(RenderSize));
        void Label(string text, double x, double y)
        {
            var formatted = new FormattedText(text, CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, new Typeface("Segoe UI"), 12, Brushes.Black,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
            drawing.DrawText(formatted, new Point(x, y));
        }
        Label("Elevation (m; vertical datum unverified)", 8, 6);
        var heights = _profile.Samples.Take(_profile.Processed)
            .Where(s => s.Height.HasValue).Select(s => s.Height!.Value).ToArray();
        if (heights.Length == 0 || _profile.Horizontal <= 0)
        {
            Label(_profile.PointCount < 2 ? "Click two or more terrain points." :
                "No elevation samples available yet.", 60, 80);
            return;
        }
        var plot = new Rect(65, 30, Math.Max(1, ActualWidth - 95), Math.Max(1, ActualHeight - 80));
        double min = heights.Min(), max = heights.Max();
        double padding = Math.Max(.5, (max - min) * .1);
        min -= padding;
        max += padding;
        var grid = new Pen(Brushes.Gainsboro, 1);
        var curve = new Pen(Brushes.SteelBlue, 2);
        for (int i = 0; i <= 4; i++)
        {
            double x = plot.Left + plot.Width * i / 4;
            double y = plot.Bottom - plot.Height * i / 4;
            drawing.DrawLine(grid, new Point(plot.Left, y), new Point(plot.Right, y));
            Label((min + (max - min) * i / 4).ToString("F1"), 3, y - 7);
            Label((_profile.Horizontal * i / 4).ToString("F0"), x - 15, plot.Bottom + 4);
        }
        Label("Horizontal distance (m)", plot.Left, ActualHeight - 22);
        Point? previous = null;
        foreach (var sample in _profile.Samples.Take(_profile.Processed))
        {
            if (sample.Height is not double height) { previous = null; continue; }
            var point = new Point(plot.Left + sample.Distance / _profile.Horizontal * plot.Width,
                plot.Bottom - (height - min) / (max - min) * plot.Height);
            if (previous is Point from) drawing.DrawLine(curve, from, point);
            else drawing.DrawEllipse(Brushes.SteelBlue, null, point, 2, 2);
            previous = point;
        }
    }
}
