using System.Globalization;
using System.Windows;
using System.Windows.Media;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.ViewshedAnalysis.Wpf;

public sealed class ViewshedMap : FrameworkElement
{
    private Viewshed3D _result = new();
    private static readonly Brush Green = MakeBrush("#239b56"), Red = MakeBrush("#d35445"),
        Grey = MakeBrush("#858585"), Pending = MakeBrush("#dce3eb");

    public ViewshedMap()
    {
        ClipToBounds = true;
        SizeChanged += (_, _) => InvalidateVisual();
    }

    public Viewshed3D Result { set { _result = value; InvalidateVisual(); } }

    private static Brush MakeBrush(string color)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        brush.Freeze();
        return brush;
    }

    protected override void OnRender(DrawingContext drawing)
    {
        base.OnRender(drawing);
        drawing.DrawRectangle(Brushes.White, null, new Rect(RenderSize));
        void Label(string text, double x, double y)
        {
            drawing.DrawText(new FormattedText(text, CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, new Typeface("Segoe UI"), 12, Brushes.Black,
                VisualTreeHelper.GetDpi(this).PixelsPerDip), new Point(x, y));
        }
        if (_result.Width == 0)
        {
            Label("Select an observer on terrain.\nThen calculate visibility.", 25, 100);
            return;
        }
        double side = Math.Max(1, Math.Min(ActualWidth - 50, ActualHeight - 55));
        double left = (ActualWidth - side) / 2, top = 25, cell = side / _result.Width;
        for (int i = 0; i < _result.Cells.Count; i++)
        {
            Brush? brush = _result.Cells[i] switch
            {
                ViewshedCell.Clear => Green, ViewshedCell.Blocked => Red,
                ViewshedCell.Unknown => Grey, ViewshedCell.Pending => Pending, _ => null
            };
            if (brush != null) drawing.DrawRectangle(brush, null,
                new Rect(left + i % _result.Width * cell, top + i / _result.Width * cell, cell, cell));
        }
        Label("N ↑", ActualWidth / 2 - 12, 2);
        Label("W", 2, top + side / 2);
        Label("E", ActualWidth - 18, top + side / 2);
        Label($"S ↓    Radius: {_result.Radius:F0} m", left, top + side + 5);
        double x = left + side / 2, y = top + side / 2;
        var pen = new Pen(Brushes.Black, 2);
        drawing.DrawLine(pen, new Point(x - 6, y), new Point(x + 6, y));
        drawing.DrawLine(pen, new Point(x, y - 6), new Point(x, y + 6));
        drawing.DrawEllipse(null, new Pen(Brushes.White, 1), new Point(x, y), 4, 4);
    }
}
