using GeoKernel.NET.WinForms;

namespace GeoKernel.CrsByAuthority.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        authorityComboBox.Text = "EPSG";
        authoritySridNumericUpDown.Value = 32635;
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
        var databasePath = Path.Combine(FindRepositoryRoot(), "assets", "spatial_ref_sys.sqlite");
        var authority = authorityComboBox.Text.Trim().ToUpperInvariant();
        var authoritySrid = (int)authoritySridNumericUpDown.Value;
        var record = GeoKernelCrsDatabase.FindByAuthority(databasePath, authority, authoritySrid);

        if (!record.Found)
        {
            summaryTextBox.Text = string.IsNullOrWhiteSpace(record.Error)
                ? $"{authority}:{authoritySrid} not found"
                : "Lookup failed";
            detailsTextBox.Text = string.IsNullOrWhiteSpace(record.Error)
                ? $"No CRS record found for authority {authority}:{authoritySrid}."
                : record.Error;
            statusLabel.Text = summaryTextBox.Text;
            return;
        }

        summaryTextBox.Text = $"{record.AuthName}:{record.AuthSrid} / SRID:{record.Srid}";
        detailsTextBox.Text = RecordDetails(record, databasePath, authority, authoritySrid);
        statusLabel.Text = $"Loaded CRS record {record.AuthName}:{record.AuthSrid}";
    }

    private static string RecordDetails(GeoKernelCrsDatabaseRecord record, string databasePath, string authority, int authoritySrid)
    {
        return string.Join(
            Environment.NewLine,
            $"CrsDatabase::findByAuthority(\"{authority}\", {authoritySrid})",
            "",
            "Database",
            databasePath,
            "",
            "Record",
            $"SRID: {record.Srid}",
            $"Authority: {record.AuthName}",
            $"Authority SRID: {record.AuthSrid}",
            "",
            "Usage",
            "GeoKernelCrsDatabase.FindByAuthority(databasePath, authority, authoritySrid)",
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

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "assets", "spatial_ref_sys.sqlite")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return AppContext.BaseDirectory;
    }
}
