using System.Globalization;
using GeoKernel.NET.WinForms;

namespace GeoKernel.Classification.Winforms;

public sealed partial class MainForm : Form
{
    private int _layerIndex = -1;
    public MainForm() => InitializeComponent();

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        try
        {
            geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
            geoKernelViewerControl.AddOpenStreetMapLayer();
            FillControls();
            controlsPanel.Enabled = false;

            var path = await SampleData.EnsureFileAsync("california.zip", "california", "california.shp", "California", this,
                new ControlProgress<SampleDataProgress>(this, SetProgress));
            downloadProgressBar.Visible = false;
            if (string.IsNullOrEmpty(path))
                return;

            statusLabel.Text = "Loading California counties...";
            if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = BaseStyle() }))
                throw new InvalidOperationException("California layer could not be loaded.");

            var layer = geoKernelViewerControl.GetLayerInfo(0)
                ?? throw new InvalidOperationException("Loaded California layer could not be inspected.");
            _layerIndex = layer.Index;
            geoKernelViewerControl.SetLayerName(_layerIndex, "California counties - classification");
            PopulateFields();
            controlsPanel.Enabled = true;
            ApplyClassification();
            geoKernelViewerControl.ZoomToLayer(_layerIndex);
        }
        catch (Exception ex)
        {
            downloadProgressBar.Visible = false;
            statusLabel.Text = "Classification could not be initialized.";
            MessageBox.Show(this, ex.Message, "Classification", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void FillControls()
    {
        rendererComboBox.Items.AddRange(["Categorized", "Graduated"]); rendererComboBox.SelectedIndex = 1;
        methodComboBox.DataSource = Enum.GetValues<GeoKernelClassificationMethod>(); methodComboBox.SelectedItem = GeoKernelClassificationMethod.NaturalBreaks;
        rampComboBox.Items.AddRange(geoKernelViewerControl.GetColorRampNames().Cast<object>().ToArray()); rampComboBox.SelectedItem = GeoKernelColorRampNames.GreenBlue;
        rampModeComboBox.DataSource = Enum.GetValues<GeoKernelColorRampMode>(); targetComboBox.DataSource = Enum.GetValues<GeoKernelSymbolStyleTarget>();
    }

    private void PopulateFields()
    {
        var selectedField = fieldComboBox.SelectedItem?.ToString();
        fieldComboBox.Items.Clear();

        var numericOnly = rendererComboBox.SelectedIndex == 1;
        foreach (var definition in geoKernelViewerControl.GetLayerAttributeDefinitions(_layerIndex))
        {
            var name = definition.Name.Trim();
            if (name.Length == 0)
                continue;

            var typeName = definition.Type.ToString();
            var numeric = typeName is "Integer" or "Double";
            if (!numericOnly || numeric)
                fieldComboBox.Items.Add(name);
        }

        if (fieldComboBox.Items.Count == 0)
            throw new InvalidOperationException("No compatible attribute fields were found in the California layer schema.");

        var preferredField = numericOnly ? "POPULATION" : "STATEFP";
        var selected = fieldComboBox.Items.Cast<object>().FirstOrDefault(x => string.Equals(x.ToString(), selectedField, StringComparison.OrdinalIgnoreCase));
        var preferred = fieldComboBox.Items.Cast<object>().FirstOrDefault(x => string.Equals(x.ToString(), preferredField, StringComparison.OrdinalIgnoreCase));
        fieldComboBox.SelectedItem = selected ?? preferred ?? fieldComboBox.Items[0];
    }

    private void applyButton_Click(object sender, EventArgs e) => ApplyClassification();
    private void clearButton_Click(object sender, EventArgs e) { if (_layerIndex >= 0 && geoKernelViewerControl.ClearLayerSymbolRenderer(_layerIndex)) { geoKernelViewerControl.SetLayerStyle(_layerIndex, BaseStyle()); geoKernelViewerControl.RefreshLayers(); statusLabel.Text = "Renderer cleared."; } }
    private void fullExtentButton_Click(object sender, EventArgs e) => geoKernelViewerControl.FullExtent();
    private void rendererComboBox_SelectedIndexChanged(object sender, EventArgs e) { if (_layerIndex >= 0) PopulateFields(); SyncControls(); }
    private void methodComboBox_SelectedIndexChanged(object sender, EventArgs e) => SyncControls();

    private void SyncControls()
    {
        var graduated = rendererComboBox.SelectedIndex == 1; methodComboBox.Enabled = graduated; classCountNumeric.Enabled = graduated; intervalNumeric.Enabled = graduated; manualBreaksTextBox.Enabled = graduated && methodComboBox.SelectedItem is GeoKernelClassificationMethod.Manual; rampModeComboBox.Enabled = graduated;
    }

    private void ApplyClassification()
    {
        if (_layerIndex < 0 || fieldComboBox.SelectedItem is null) return;
        var field = fieldComboBox.SelectedItem.ToString()!; var ramp = rampComboBox.SelectedItem?.ToString() ?? GeoKernelColorRampNames.GreenBlue;
        var target = targetComboBox.SelectedItem is GeoKernelSymbolStyleTarget t ? t : GeoKernelSymbolStyleTarget.Color;
        bool ok;
        if (rendererComboBox.SelectedIndex == 0)
            ok = geoKernelViewerControl.ApplyLayerCategorizedRenderer(_layerIndex, field, ramp, reverseColorRamp: reverseCheckBox.Checked, styleTarget: target);
        else
        {
            var method = methodComboBox.SelectedItem is GeoKernelClassificationMethod m ? m : GeoKernelClassificationMethod.NaturalBreaks;
            var breaks = manualBreaksTextBox.Text.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => double.TryParse(x.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : double.NaN).Where(double.IsFinite).ToArray();
            ok = geoKernelViewerControl.ApplyLayerGraduatedRenderer(_layerIndex, field, method, (int)classCountNumeric.Value, ramp, (double)intervalNumeric.Value, breaks, (GeoKernelColorRampMode)rampModeComboBox.SelectedItem!, reverseCheckBox.Checked, styleTarget: target);
        }
        if (ok) { geoKernelViewerControl.RefreshLayers(); statusLabel.Text = $"{rendererComboBox.Text} renderer applied: {field}"; }
        else { statusLabel.Text = $"Renderer could not be created for field '{field}'."; }
    }

    private void SetProgress(SampleDataProgress p) { statusLabel.Text = p.Message; downloadProgressBar.Visible = true; downloadProgressBar.Style = p.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee; if (p.Percentage.HasValue) downloadProgressBar.Value = p.Percentage.Value; }
    private static GeoKernelLayerStyle BaseStyle() => new() { FillColor = "#DCE8E4", FillOpacity = 225, LineColor = "#536B68", LineWidth = 0.8 };
}
