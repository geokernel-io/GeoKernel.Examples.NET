using System.ComponentModel;
using GeoKernel.NET.WinForms;

namespace GeoKernel.ViewshedAnalysis.Winforms;

internal sealed class ViewshedMap : Control
{
    private Viewshed3D _result = new();
    public ViewshedMap() { DoubleBuffered = true; ResizeRedraw = true; BackColor = Color.White; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Viewshed3D Result { set { _result = value; Invalidate(); } }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        if (_result.Width == 0)
        {
            g.DrawString("Select an observer on terrain.\nThen calculate visibility.", Font, Brushes.Black, 25, 100);
            return;
        }
        float side = Math.Max(1, Math.Min(Width - 50, Height - 55));
        float left = (Width - side) / 2, top = 25, cell = side / _result.Width;
        using var green = new SolidBrush(ColorTranslator.FromHtml("#239b56"));
        using var red = new SolidBrush(ColorTranslator.FromHtml("#d35445"));
        using var grey = new SolidBrush(ColorTranslator.FromHtml("#858585"));
        using var pending = new SolidBrush(ColorTranslator.FromHtml("#dce3eb"));
        for (int i = 0; i < _result.Cells.Count; i++)
        {
            Brush? brush = _result.Cells[i] switch
            {
                ViewshedCell.Clear => green, ViewshedCell.Blocked => red,
                ViewshedCell.Unknown => grey, ViewshedCell.Pending => pending, _ => null
            };
            if (brush != null) g.FillRectangle(brush, left + i % _result.Width * cell,
                top + i / _result.Width * cell, cell, cell);
        }
        g.DrawString("N ↑", Font, Brushes.Black, Width / 2 - 12, 2);
        g.DrawString("W", Font, Brushes.Black, 2, top + side / 2);
        g.DrawString("E", Font, Brushes.Black, Width - 18, top + side / 2);
        g.DrawString($"S ↓    Radius: {_result.Radius:F0} m", Font, Brushes.Black, left, top + side + 5);
        float x = left + side / 2, y = top + side / 2;
        g.DrawLine(Pens.Black, x - 6, y, x + 6, y);
        g.DrawLine(Pens.Black, x, y - 6, x, y + 6);
        g.DrawEllipse(Pens.White, x - 4, y - 4, 8, 8);
    }
}
