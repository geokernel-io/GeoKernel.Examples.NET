using GeoKernel.NET.WinForms;

namespace GeoKernel.UtmZones.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        authorityComboBox.Items.AddRange(["Northern hemisphere", "Southern hemisphere"]);
        authorityComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        authorityComboBox.SelectedIndex = 0;
        authoritySridNumericUpDown.Minimum = 1;
        authoritySridNumericUpDown.Maximum = 60;
        authoritySridNumericUpDown.Value = 35;
        Lookup();
    }

    private void findButton_Click(object sender, EventArgs e)
    {
        Lookup();
    }

    private void authoritySridNumericUpDown_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
            return;

        e.SuppressKeyPress = true;
        Lookup();
    }

    private void authorityComboBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
            return;

        e.SuppressKeyPress = true;
        Lookup();
    }

    private void Lookup()
    {
        var zone = (int)authoritySridNumericUpDown.Value;
        var north = authorityComboBox.SelectedIndex == 0;
        var epsg = (north ? 32600 : 32700) + zone;
        var record = GeoKernelCoordinateSystemFactory.FromEpsg(epsg);

        if (!record.Found)
        {
            summaryTextBox.Text = string.IsNullOrWhiteSpace(record.Error)
                ? $"EPSG:{epsg} not found"
                : "Lookup failed";
            detailsTextBox.Text = string.IsNullOrWhiteSpace(record.Error)
                ? $"No CRS record found for UTM zone {zone}."
                : record.Error;
            statusLabel.Text = summaryTextBox.Text;
            return;
        }

        summaryTextBox.Text = $"{record.AuthName}:{record.AuthSrid} / SRID:{record.Srid}";
        detailsTextBox.Text = RecordDetails(record, zone, north);
        statusLabel.Text = $"Loaded CRS record {record.AuthName}:{record.AuthSrid}";
    }

    private static string RecordDetails(GeoKernelCrsDatabaseRecord record, int zone, bool north)
    {
        return string.Join(
            Environment.NewLine,
            $"WGS 84 / UTM zone {zone}{(north ? "N" : "S")}",
            "",
            "Record",
            $"SRID: {record.Srid}",
            $"Authority: {record.AuthName}",
            $"Authority SRID: {record.AuthSrid}",
            "",
            "Usage",
            $"GeoKernelCoordinateSystemFactory.FromEpsg({record.AuthSrid})",
            "",
            "WKT / srtext",
            Preview(record.SrText),
            "",
            "PROJ.4 / proj4text",
            string.IsNullOrWhiteSpace(record.Proj4Text) ? "(empty)" : record.Proj4Text);
    }

    private static string Preview(string text)
    {
        const int maxLength = 2200;
        var trimmed = text.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength] + Environment.NewLine + "...";
    }

}
