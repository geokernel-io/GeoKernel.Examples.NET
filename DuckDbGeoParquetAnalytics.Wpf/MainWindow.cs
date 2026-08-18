using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.DuckDbGeoParquetAnalytics.Wpf;

public sealed class MainWindow : Window
{
    private readonly GeoKernelViewerControl viewer = new();
    private readonly ComboBox classBox = new() { IsEditable = true, ItemsSource = new[] { "apartments", "residential", "house" }, Text = "apartments" };
    private readonly TextBox limitBox = new() { Text = "25000" };
    private readonly Button runButton = new() { Content = "Run measured comparison", Height = 30 };
    private readonly TextBox report = new() { IsReadOnly = true, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    private readonly TextBlock status = new();
    private string parquetPath = string.Empty;

    public MainWindow()
    {
        Title="DuckDbGeoParquetAnalytics"; Width=1220; Height=790;
        Icon=System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Images/GeoKernelAppIcon.ico"));
        var root=new DockPanel(); var toolbar=new StackPanel{Orientation=Orientation.Horizontal,Height=34,Background=Brushes.WhiteSmoke};
        foreach(var item in new (string,Action)[]{("Zoom In",()=>viewer.ZoomIn()),("Zoom Out",()=>viewer.ZoomOut()),("Full Extent",viewer.FullExtent),("Pan",()=>viewer.ActiveTool=GeoKernelViewerTool.Pan)})
        {var button=new Button{Content=item.Item1,Margin=new Thickness(2)};button.Click+=(_,_)=>item.Item2();toolbar.Children.Add(button);}
        DockPanel.SetDock(toolbar,Dock.Top);root.Children.Add(toolbar);
        var footer=new Border{Child=status,Padding=new Thickness(4),Background=Brushes.WhiteSmoke};DockPanel.SetDock(footer,Dock.Bottom);root.Children.Add(footer);
        var right=new StackPanel{Width=390,Margin=new Thickness(10)};right.Children.Add(new TextBlock{Text="DuckDB GeoParquet analytics",FontWeight=FontWeights.Bold,FontSize=14,Margin=new Thickness(0,0,0,8)});
        right.Children.Add(new TextBlock{Text="Building class"});right.Children.Add(classBox);right.Children.Add(new TextBlock{Text="Maximum results"});right.Children.Add(limitBox);
        right.Children.Add(new TextBlock{Text="Spatial filter: Central Stockholm BBOX",Margin=new Thickness(0,6,0,6)});right.Children.Add(runButton);
        report.Height=570;report.Margin=new Thickness(0,8,0,0);right.Children.Add(report);DockPanel.SetDock(right,Dock.Right);root.Children.Add(right);root.Children.Add(viewer);Content=root;
        viewer.ActiveTool=GeoKernelViewerTool.Pan;viewer.MapBackgroundColor=System.Drawing.Color.FromArgb(244,246,245);
        report.Text="Press Run measured comparison.\n\nThe baseline transfers every row and filters in the application. The optimized path pushes predicate, BBOX, projection and limit into DuckDB.";
        runButton.IsEnabled=false;status.Text="Loading sample data...";runButton.Click+=async(_,_)=>await RunAsync();Loaded+=LoadSample;
    }

    private void LoadSample(object sender,RoutedEventArgs e)
    {
        try
        {
            parquetPath=SampleData.EnsureWpfSampleFile(
                new Uri("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/stockholm_data.zip"),
                "stockholm_data.zip",".",System.IO.Path.Combine("stockholm_data","stockholm_buildings.parquet"),this);
            if(string.IsNullOrWhiteSpace(parquetPath))
            {
                status.Text="Sample data was not loaded.";
                return;
            }
            runButton.IsEnabled=true;status.Text=$"Ready: {System.IO.Path.GetFileName(parquetPath)}";
        }
        catch(Exception ex)
        {
            status.Text="Sample data could not be loaded.";
            MessageBox.Show(this,ex.Message,Title,MessageBoxButton.OK,MessageBoxImage.Error);
        }
    }
    private async Task RunAsync()
    {
        runButton.IsEnabled=false;report.Text="Running full transfer and DuckDB pushdown paths...";status.Text="Benchmark running in background...";
        try {
            var limit=long.TryParse(limitBox.Text,out var parsed)?Math.Clamp(parsed,1,100000):25000;
            var buildingClass=classBox.Text.Trim();
            var result=await Task.Run(()=>AnalyticsEngine.Run(parquetPath,buildingClass,limit));var timer=Stopwatch.StartNew();viewer.ClearLayers();
            var rings=new List<IReadOnlyList<GeoKernelPoint>>();foreach(var wkb in result.Geometries)foreach(var ring in viewer.ReadWkbPolygon(wkb,IsMultiPolygon(wkb)))rings.Add(ring);
            viewer.AddPolygonLayer("DuckDB pushdown result",rings,new GeoKernelLayerStyle{FillColor="#65B8E8",LineColor="#176B9C",LineWidth=0.8});viewer.FullExtent();timer.Stop();
            report.Text=AnalyticsEngine.Report(result,timer.ElapsedMilliseconds);status.Text="Comparison completed.";
        } catch(Exception ex){report.Text=$"Comparison failed:\n{ex.Message}";status.Text="Comparison failed.";MessageBox.Show(this,ex.Message,Title,MessageBoxButton.OK,MessageBoxImage.Error);}
        finally{runButton.IsEnabled=true;}
    }

    private static bool IsMultiPolygon(byte[] wkb)
    {
        if(wkb.Length<5)throw new InvalidDataException("WKB header is incomplete.");
        var type=wkb[0] switch
        {
            1=>BinaryPrimitives.ReadUInt32LittleEndian(wkb.AsSpan(1,4)),
            0=>BinaryPrimitives.ReadUInt32BigEndian(wkb.AsSpan(1,4)),
            _=>throw new InvalidDataException("WKB byte order is invalid.")
        };
        var baseType=(int)((type&0x0FFFFFFF)%1000);
        return baseType switch
        {
            3=>false,
            6=>true,
            _=>throw new InvalidDataException($"Unsupported building WKB type: {baseType}.")
        };
    }
}
