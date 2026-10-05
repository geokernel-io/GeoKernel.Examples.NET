using System.ComponentModel;
using System.Drawing.Drawing2D;
using GeoKernel.NET.WinForms;

namespace GeoKernel.ElevationProfile.Winforms;

internal sealed class ProfileChart : Control
{
    private ElevationProfile3D _profile = new();

    public ProfileChart()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
        ResizeRedraw = true;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ElevationProfile3D Profile
    {
        set { _profile = value; Invalidate(); }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.DrawString("Elevation (m; vertical datum unverified)", Font, Brushes.Black, 8, 6);
        var heights = _profile.Samples.Take(_profile.Processed)
            .Where(s => s.Height.HasValue).Select(s => s.Height!.Value).ToArray();
        if (heights.Length == 0 || _profile.Horizontal <= 0)
        {
            g.DrawString(_profile.PointCount < 2 ? "Click two or more terrain points." :
                "No elevation samples available yet.", Font, Brushes.Gray, 60, 80);
            return;
        }
        var plot = new RectangleF(65, 30, Math.Max(1, Width - 95), Math.Max(1, Height - 80));
        double min = heights.Min(), max = heights.Max();
        double padding = Math.Max(.5, (max - min) * .1);
        min -= padding;
        max += padding;
        using var grid = new Pen(Color.Gainsboro);
        using var curve = new Pen(Color.SteelBlue, 2);
        for (int i = 0; i <= 4; i++)
        {
            float x = plot.Left + plot.Width * i / 4;
            float y = plot.Bottom - plot.Height * i / 4;
            g.DrawLine(grid, plot.Left, y, plot.Right, y);
            g.DrawString((min + (max - min) * i / 4).ToString("F1"), Font, Brushes.Black, 3, y - 7);
            g.DrawString((_profile.Horizontal * i / 4).ToString("F0"), Font, Brushes.Black, x - 15, plot.Bottom + 4);
        }
        g.DrawString("Horizontal distance (m)", Font, Brushes.Black, plot.Left, Height - 22);
        PointF? previous = null;
        foreach (var sample in _profile.Samples.Take(_profile.Processed))
        {
            if (sample.Height is not double height) { previous = null; continue; }
            var point = new PointF(plot.Left + (float)(sample.Distance / _profile.Horizontal) * plot.Width,
                plot.Bottom - (float)((height - min) / (max - min)) * plot.Height);
            if (previous is PointF from) g.DrawLine(curve, from, point);
            else g.FillEllipse(Brushes.SteelBlue, point.X - 2, point.Y - 2, 4, 4);
            previous = point;
        }
    }
}
