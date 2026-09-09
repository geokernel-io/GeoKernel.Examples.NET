using GeoKernel.NET.WinForms;

namespace GeoKernel.CrsDatabase.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        sridNumericUpDown.Value = 4326;
        Lookup();
    }

    private void findButton_Click(object sender, EventArgs e)
    {
        Lookup();
    }

    private void sridNumericUpDown_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
            return;

        e.SuppressKeyPress = true;
        Lookup();
    }

    private void Lookup()
    {
        var srid = (int)sridNumericUpDown.Value;
        var record = GeoKernelCoordinateSystemFactory.FromEpsg(srid);

        if (!record.Found)
        {
            summaryTextBox.Text = string.IsNullOrWhiteSpace(record.Error)
                ? $"EPSG:{srid} not found"
                : "Lookup failed";
            detailsTextBox.Text = string.IsNullOrWhiteSpace(record.Error)
                ? $"No CRS record found for SRID {srid}."
                : record.Error;
            statusLabel.Text = summaryTextBox.Text;
            return;
        }

        summaryTextBox.Text = $"{record.AuthName}:{record.AuthSrid} / SRID:{record.Srid}";
        detailsTextBox.Text = RecordDetails(record);
        statusLabel.Text = $"Loaded CRS record {record.AuthName}:{record.AuthSrid}";
    }

    private static string RecordDetails(GeoKernelCrsDatabaseRecord record)
    {
        return string.Join(
            Environment.NewLine,
            $"CoordinateSystemFactory::fromEpsg({record.Srid})",
            "",
            "Record",
            $"SRID: {record.Srid}",
            $"Authority: {record.AuthName}",
            $"Authority SRID: {record.AuthSrid}",
            "",
            "Usage",
            "GeoKernelCoordinateSystemFactory.FromEpsg(srid)",
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
