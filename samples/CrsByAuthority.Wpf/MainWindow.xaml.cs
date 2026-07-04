using System.IO;
using System.Windows;
using System.Windows.Input;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.CrsByAuthority.Wpf;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Lookup();
    }

    private void FindButton_Click(object sender, RoutedEventArgs e)
    {
        Lookup();
    }

    private void AuthorityComboBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        e.Handled = true;
        Lookup();
    }

    private void AuthoritySridTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        e.Handled = true;
        Lookup();
    }

    private void Lookup()
    {
        var authority = authorityComboBox.Text.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(authority))
        {
            summaryTextBox.Text = "Invalid authority";
            detailsTextBox.Text = "Enter an authority name, for example EPSG.";
            statusTextBlock.Text = "Invalid authority";
            return;
        }

        if (!int.TryParse(authoritySridTextBox.Text, out var authoritySrid) || authoritySrid <= 0)
        {
            summaryTextBox.Text = "Invalid authority code";
            detailsTextBox.Text = "Enter a positive numeric authority code, for example 32635.";
            statusTextBlock.Text = "Invalid authority code";
            return;
        }

        var databasePath = Path.Combine(FindRepositoryRoot(), "assets", "spatial_ref_sys.sqlite");
        var record = GeoKernelCrsDatabase.FindByAuthority(databasePath, authority, authoritySrid);

        if (!record.Found)
        {
            summaryTextBox.Text = string.IsNullOrWhiteSpace(record.Error)
                ? $"{authority}:{authoritySrid} not found"
                : "Lookup failed";
            detailsTextBox.Text = string.IsNullOrWhiteSpace(record.Error)
                ? $"No CRS record found for authority {authority}:{authoritySrid}."
                : record.Error;
            statusTextBlock.Text = summaryTextBox.Text;
            return;
        }

        summaryTextBox.Text = $"{record.AuthName}:{record.AuthSrid} / SRID:{record.Srid}";
        detailsTextBox.Text = RecordDetails(record, databasePath, authority, authoritySrid);
        statusTextBlock.Text = $"Loaded CRS record {record.AuthName}:{record.AuthSrid}";
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
