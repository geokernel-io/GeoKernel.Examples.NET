using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GeoKernel.ProjectSaveLoad.Wpf;

internal sealed class LayerColorDialog : Window
{
    private readonly Slider _red;
    private readonly Slider _green;
    private readonly Slider _blue;
    private readonly Border _preview = new() { Height = 48, Margin = new Thickness(0, 12, 0, 8) };
    private readonly TextBlock _hex = new();

    public Color SelectedColor => Color.FromRgb((byte)_red.Value, (byte)_green.Value, (byte)_blue.Value);

    public LayerColorDialog(Color color)
    {
        Title = "Layer color";
        Width = 350;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ShowInTaskbar = false;
        var panel = new StackPanel { Margin = new Thickness(16) };
        _red = Channel(panel, "Red", color.R);
        _green = Channel(panel, "Green", color.G);
        _blue = Channel(panel, "Blue", color.B);
        panel.Children.Add(_preview);
        panel.Children.Add(_hex);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var ok = new Button { Content = "OK", IsDefault = true, Width = 80, Margin = new Thickness(4, 12, 0, 0) };
        var cancel = new Button { Content = "Cancel", IsCancel = true, Width = 80, Margin = new Thickness(4, 12, 0, 0) };
        ok.Click += (_, _) => DialogResult = true;
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        panel.Children.Add(buttons);
        Content = panel;
        _red.ValueChanged += (_, _) => UpdatePreview();
        _green.ValueChanged += (_, _) => UpdatePreview();
        _blue.ValueChanged += (_, _) => UpdatePreview();
        UpdatePreview();
    }

    private static Slider Channel(Panel panel, string name, byte value)
    {
        panel.Children.Add(new TextBlock { Text = name, Margin = new Thickness(0, 6, 0, 4) });
        var slider = new Slider { Minimum = 0, Maximum = 255, Value = value, TickFrequency = 1, IsSnapToTickEnabled = true };
        panel.Children.Add(slider);
        return slider;
    }

    private void UpdatePreview()
    {
        var color = SelectedColor;
        _preview.Background = new SolidColorBrush(color);
        _hex.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2} — RGB {color.R}, {color.G}, {color.B}";
    }
}
