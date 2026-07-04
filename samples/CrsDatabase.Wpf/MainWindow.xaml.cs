using System.IO;
using System.Windows;
using System.Windows.Input;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.CrsDatabase.Wpf;

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

    private void SridTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        e.Handled = true;
        Lookup();
    }

    private void Lookup()
    {
        if (!int.TryParse(sridTextBox.Text, out var srid) || srid <= 0)
        {
            summaryTextBox.Text = "Invalid SRID";
            detailsTextBox.Text = "Enter a positive numeric SRID, for example 4326 or 3857.";
            statusTextBlock.Text = "Invalid SRID";
            return;
        }

        var databasePath = Path.Combine(FindRepositoryRoot(), "assets", "spatial_ref_sys.sqlite");
        var record = GeoKernelCrsDatabase.FindBySrid(databasePath, srid);

        if (!record.Found)
        {
            summaryTextBox.Text = string.IsNullOrWhiteSpace(record.Error)
                ? $"EPSG:{srid} not found"
                : "Lookup failed";
            detailsTextBox.Text = string.IsNullOrWhiteSpace(record.Error)
                ? $"No CRS record found for SRID {srid}."
                : record.Error;
            statusTextBlock.Text = summaryTextBox.Text;
            return;
        }

        summaryTextBox.Text = $"{record.AuthName}:{record.AuthSrid} / SRID:{record.Srid}";
        detailsTextBox.Text = RecordDetails(record, databasePath);
        statusTextBlock.Text = $"Loaded CRS record {record.AuthName}:{record.AuthSrid}";
    }

    private static string RecordDetails(GeoKernelCrsDatabaseRecord record, string databasePath)
    {
        return string.Join(
            Environment.NewLine,
            $"CrsDatabase::findBySrid({record.Srid})",
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
            "GeoKernelCrsDatabase.FindBySrid(databasePath, srid)",
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
