using GeoKernel.NET.Wpf.Controls;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace GeoKernel.UtmZones.Wpf;

public partial class MainWindow
{
    private static readonly Regex DigitsOnly = new("^[0-9]+$");

    public MainWindow() => InitializeComponent();

    private void Window_Loaded(object sender, RoutedEventArgs e) => Lookup();
    private void FindButton_Click(object sender, RoutedEventArgs e) => Lookup();

    private void Input_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        Lookup();
    }

    private void ZoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !DigitsOnly.IsMatch(e.Text);
    }

    private void Lookup()
    {
        if (!int.TryParse(zoneTextBox.Text, out var zone) || zone is < 1 or > 60)
        {
            summaryTextBox.Text = "Invalid UTM zone";
            detailsTextBox.Text = "Enter a UTM zone from 1 through 60.";
            statusText.Text = "Invalid UTM zone.";
            return;
        }

        var north = hemisphereComboBox.SelectedIndex == 0;
        var epsg = (north ? 32600 : 32700) + zone;
        var record = GeoKernelCoordinateSystemFactory.FromEpsg(epsg);

        if (!record.Found)
        {
            summaryTextBox.Text = string.IsNullOrWhiteSpace(record.Error) ? $"EPSG:{epsg} not found" : "Lookup failed";
            detailsTextBox.Text = string.IsNullOrWhiteSpace(record.Error) ? $"No CRS record found for UTM zone {zone}." : record.Error;
            statusText.Text = summaryTextBox.Text;
            return;
        }

        summaryTextBox.Text = $"{record.AuthName}:{record.AuthSrid} / SRID:{record.Srid}";
        detailsTextBox.Text = RecordDetails(record, zone, north);
        statusText.Text = $"Loaded CRS record {record.AuthName}:{record.AuthSrid}";
    }

    private static string RecordDetails(GeoKernelCrsDatabaseRecord record, int zone, bool north) =>
        string.Join(Environment.NewLine,
            $"WGS 84 / UTM zone {zone}{(north ? "N" : "S")}", "", "Record",
            $"SRID: {record.Srid}", $"Authority: {record.AuthName}", $"Authority SRID: {record.AuthSrid}", "", "Usage",
            $"GeoKernelCoordinateSystemFactory.FromEpsg({record.AuthSrid})", "", "WKT / srtext", Preview(record.SrText), "",
            "PROJ.4 / proj4text", string.IsNullOrWhiteSpace(record.Proj4Text) ? "(empty)" : record.Proj4Text);

    private static string Preview(string text)
    {
        const int maxLength = 2200;
        var trimmed = text.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength] + Environment.NewLine + "...";
    }
}
