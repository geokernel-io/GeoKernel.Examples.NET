using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.Classification.Wpf;

public partial class MainWindow
{
    private int _layerIndex = -1;
    private bool _initializing = true;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
            viewerControl.AddOpenStreetMapLayer();
            FillControls();

            var path = SampleData.EnsureKnownWpfSampleFile("california.shp", this);
            if (string.IsNullOrWhiteSpace(path))
                return;

            statusText.Text = "Loading California counties...";
            if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = BaseStyle()
                }))
                throw new InvalidOperationException("California layer could not be loaded.");

            var layer = viewerControl.GetLayerInfo(0)
                ?? throw new InvalidOperationException("Loaded California layer could not be inspected.");
            _layerIndex = layer.Index;
            viewerControl.SetLayerName(_layerIndex, "California counties - classification");
            PopulateFields();
            controlsPanel.IsEnabled = true;
            _initializing = false;
            SyncControls();
            ApplyClassification();
            viewerControl.ZoomToLayer(_layerIndex);
        }
        catch (Exception ex)
        {
            statusText.Text = "Classification could not be initialized.";
            MessageBox.Show(this, ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void FillControls()
    {
        rendererComboBox.ItemsSource = new[] { "Categorized", "Graduated" };
        rendererComboBox.SelectedIndex = 1;
        methodComboBox.ItemsSource = Enum.GetValues<GeoKernelClassificationMethod>();
        methodComboBox.SelectedItem = GeoKernelClassificationMethod.NaturalBreaks;
        rampComboBox.ItemsSource = viewerControl.GetColorRampNames();
        rampComboBox.SelectedItem = GeoKernelColorRampNames.GreenBlue;
        rampModeComboBox.ItemsSource = Enum.GetValues<GeoKernelColorRampMode>();
        rampModeComboBox.SelectedItem = GeoKernelColorRampMode.Continuous;
        targetComboBox.ItemsSource = Enum.GetValues<GeoKernelSymbolStyleTarget>();
        targetComboBox.SelectedItem = GeoKernelSymbolStyleTarget.Color;
    }

    private void PopulateFields()
    {
        var selectedField = fieldComboBox.SelectedItem?.ToString();
        fieldComboBox.Items.Clear();
        var numericOnly = rendererComboBox.SelectedIndex == 1;

        foreach (var definition in viewerControl.GetLayerAttributeDefinitions(_layerIndex))
        {
            var name = definition.Name.Trim();
            var typeName = definition.Type.ToString();
            var numeric = typeName is "Integer" or "Double";
            if (name.Length > 0 && (!numericOnly || numeric))
                fieldComboBox.Items.Add(name);
        }

        if (fieldComboBox.Items.Count == 0)
            throw new InvalidOperationException("No compatible attribute fields were found in the California layer schema.");

        var preferredName = numericOnly ? "POPULATION" : "STATEFP";
        var selected = fieldComboBox.Items.Cast<object>().FirstOrDefault(item =>
            string.Equals(item.ToString(), selectedField, StringComparison.OrdinalIgnoreCase));
        var preferred = fieldComboBox.Items.Cast<object>().FirstOrDefault(item =>
            string.Equals(item.ToString(), preferredName, StringComparison.OrdinalIgnoreCase));
        fieldComboBox.SelectedItem = selected ?? preferred ?? fieldComboBox.Items[0];
    }

    private void Renderer_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_layerIndex >= 0)
            PopulateFields();
        SyncControls();
    }

    private void Method_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (methodComboBox.SelectedItem is GeoKernelClassificationMethod.StandardDeviation or
            GeoKernelClassificationMethod.StandardDeviationWithCentral &&
            ParseDouble(intervalTextBox.Text, 100000.0) > 10.0)
            intervalTextBox.Text = "1.0";
        SyncControls();
    }

    private void SyncControls()
    {
        if (_initializing && methodComboBox.SelectedItem is null)
            return;

        var graduated = rendererComboBox.SelectedIndex == 1;
        var method = methodComboBox.SelectedItem is GeoKernelClassificationMethod value
            ? value
            : GeoKernelClassificationMethod.NaturalBreaks;
        var usesClassCount = !graduated || method is not (GeoKernelClassificationMethod.Manual or
            GeoKernelClassificationMethod.DefinedInterval or GeoKernelClassificationMethod.Quartile or
            GeoKernelClassificationMethod.StandardDeviation or GeoKernelClassificationMethod.StandardDeviationWithCentral);
        var usesInterval = graduated && method is GeoKernelClassificationMethod.DefinedInterval or
            GeoKernelClassificationMethod.StandardDeviation or GeoKernelClassificationMethod.StandardDeviationWithCentral;
        var usesManualBreaks = graduated && method == GeoKernelClassificationMethod.Manual;

        methodComboBox.IsEnabled = graduated;
        classCountLabel.Text = graduated ? "Classes" : "Categories";
        classCountLabel.IsEnabled = classCountTextBox.IsEnabled = usesClassCount;
        intervalLabel.Text = method is GeoKernelClassificationMethod.StandardDeviation or
            GeoKernelClassificationMethod.StandardDeviationWithCentral ? "Std dev step" : "Interval";
        intervalLabel.IsEnabled = intervalTextBox.IsEnabled = usesInterval;
        manualBreaksLabel.IsEnabled = manualBreaksTextBox.IsEnabled = usesManualBreaks;
        rampModeComboBox.IsEnabled = graduated;
    }

    private void Apply_Click(object sender, RoutedEventArgs e) => ApplyClassification();

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        if (_layerIndex < 0 || !viewerControl.ClearLayerSymbolRenderer(_layerIndex))
            return;
        viewerControl.SetLayerStyle(_layerIndex, BaseStyle());
        viewerControl.RefreshLayers();
        statusText.Text = "Renderer cleared.";
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e) => viewerControl.FullExtent();

    private void ApplyClassification()
    {
        if (_layerIndex < 0 || fieldComboBox.SelectedItem is null)
            return;

        var field = fieldComboBox.SelectedItem.ToString()!;
        var ramp = rampComboBox.SelectedItem?.ToString() ?? GeoKernelColorRampNames.GreenBlue;
        var target = targetComboBox.SelectedItem is GeoKernelSymbolStyleTarget selectedTarget
            ? selectedTarget
            : GeoKernelSymbolStyleTarget.Color;
        bool applied;

        if (rendererComboBox.SelectedIndex == 0)
        {
            applied = viewerControl.ApplyLayerCategorizedRenderer(
                _layerIndex, field, ramp, reverseColorRamp: reverseCheckBox.IsChecked == true, styleTarget: target);
        }
        else
        {
            var method = methodComboBox.SelectedItem is GeoKernelClassificationMethod selectedMethod
                ? selectedMethod
                : GeoKernelClassificationMethod.NaturalBreaks;
            var manualBreaks = ParseManualBreaks();
            if (method == GeoKernelClassificationMethod.Manual && manualBreaks.Length < 2)
            {
                MessageBox.Show(this, "Manual mode needs at least two numeric break values.", Title,
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            applied = viewerControl.ApplyLayerGraduatedRenderer(
                _layerIndex,
                field,
                method,
                Math.Clamp(ParseInt(classCountTextBox.Text, 15), 2, 64),
                ramp,
                Math.Max(0.0001, ParseDouble(intervalTextBox.Text, 100000.0)),
                manualBreaks,
                rampModeComboBox.SelectedItem is GeoKernelColorRampMode mode ? mode : GeoKernelColorRampMode.Continuous,
                reverseCheckBox.IsChecked == true,
                styleTarget: target);
        }

        if (applied)
        {
            viewerControl.RefreshLayers();
            statusText.Text = $"{rendererComboBox.SelectedItem} renderer applied: {field}";
        }
        else
        {
            statusText.Text = $"Renderer could not be created for field '{field}'.";
        }
    }

    private double[] ParseManualBreaks() => manualBreaksTextBox.Text
        .Split([',', ';', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries)
        .Select(value => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
            ? number
            : double.NaN)
        .Where(double.IsFinite)
        .OrderBy(value => value)
        .ToArray();

    private static int ParseInt(string text, int fallback) =>
        int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : fallback;

    private static double ParseDouble(string text, double fallback) =>
        double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : fallback;

    private static GeoKernelLayerStyle BaseStyle() => new()
    {
        FillColor = "#DCE8E4",
        FillOpacity = 225,
        LineColor = "#536B68",
        LineWidth = 0.8
    };
}
