using System.Text.Json;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf;
using GeoKernel.NET.Wpf.Controls;
using Microsoft.Win32;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using Panel = System.Windows.Controls.Panel;
using ProgressBar = System.Windows.Controls.ProgressBar;
using TextBox = System.Windows.Controls.TextBox;
using Orientation = System.Windows.Controls.Orientation;
using Brushes = System.Windows.Media.Brushes;
using MediaColor = System.Windows.Media.Color;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace GeoKernel.LandCoverInference.Wpf;

public sealed class MainWindow : Window
{
    static readonly Uri RasterUrl = new("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/bilbao_s2_rgbnir_2021.zip");
    static readonly Uri ModelUrl = new("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/landcover-bilbao-model.zip");
    static readonly AIClassColor[] WorldCoverColors = [new(10,0,100,0),new(20,255,187,34),new(30,255,255,76),new(40,240,150,255),new(50,250,0,0),new(60,180,180,180),new(70,240,240,240),new(80,0,100,200),new(90,0,150,160),new(95,0,207,117),new(100,250,230,160)];
    readonly GeoKernelViewerControl viewer = new(); readonly TextBox modelPath = new(); readonly TextBox rasterPath = new(); readonly TextBox outputPath = new();
    readonly ComboBox provider = new(); readonly Slider predictionOpacity = new() { Minimum = 0, Maximum = 100, Value = 50, TickFrequency = 5, IsSnapToTickEnabled = true }; readonly TextBlock predictionOpacityText = new() { Text = "50%", Width = 42, TextAlignment = TextAlignment.Right };
    readonly Button run = new() { Content = "Run land-cover inference", Height = 30 }; readonly ProgressBar progress = new() { Minimum = 0, Maximum = 100, Height = 20 };
    readonly TextBlock progressText = new(); readonly TextBox details = new() { IsReadOnly = true, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = ScrollBarVisibility.Auto }; readonly TextBlock status = new(); int predictionLayerIndex = -1;

    public MainWindow()
    {
        Title="LandCoverInference"; Width=1280; Height=820; Icon=System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Images/GeoKernelAppIcon.ico"));
        provider.ItemsSource=new[]{"Auto","CPU","CUDA","DirectML"}; provider.SelectedIndex=0;
        var root=new DockPanel(); var tools=new StackPanel{Orientation=Orientation.Horizontal,Height=36,Background=Brushes.WhiteSmoke}; foreach(var x in new (string,Action)[]{("Zoom In",()=>viewer.ZoomIn()),("Zoom Out",()=>viewer.ZoomOut()),("Full Extent",viewer.FullExtent),("Zoom Rect",()=>viewer.ActiveTool=GeoKernelViewerTool.ZoomBox),("Pan",()=>viewer.ActiveTool=GeoKernelViewerTool.Pan)}){var b=new Button{Content=x.Item1,Margin=new Thickness(2)};b.Click+=(_,_)=>x.Item2();tools.Children.Add(b);} DockPanel.SetDock(tools,Dock.Top);root.Children.Add(tools);
        var footer=new Border{Child=status,Padding=new Thickness(4)};DockPanel.SetDock(footer,Dock.Bottom);root.Children.Add(footer); var right=new DockPanel{Width=420,Margin=new Thickness(10)};DockPanel.SetDock(right,Dock.Right);
        var form=new StackPanel(); form.Children.Add(new TextBlock{Text="Land-cover inference",FontWeight=FontWeights.Bold,FontSize=14});form.Children.Add(new TextBlock{Text="Run a GeoKernel model package on a georeferenced RGBNIR raster.",TextWrapping=TextWrapping.Wrap}); AddPath(form,"Model package",modelPath,BrowseModel);AddPath(form,"Input raster",rasterPath,BrowseRaster);AddPath(form,"Class mask",outputPath,BrowseOutput);form.Children.Add(new TextBlock{Text="Execution provider"});form.Children.Add(provider);form.Children.Add(new TextBlock{Text="Prediction opacity"});var opacityRow=new DockPanel();DockPanel.SetDock(predictionOpacityText,Dock.Right);opacityRow.Children.Add(predictionOpacityText);opacityRow.Children.Add(predictionOpacity);form.Children.Add(opacityRow);form.Children.Add(run);form.Children.Add(progress);form.Children.Add(progressText);form.Children.Add(new TextBlock{Text="Inference diagnostics",Margin=new Thickness(0,7,0,2),FontWeight=FontWeights.Bold}); DockPanel.SetDock(form,Dock.Top);right.Children.Add(form);
        var legend=CreateLegend();DockPanel.SetDock(legend,Dock.Bottom);right.Children.Add(legend);right.Children.Add(details);root.Children.Add(right);root.Children.Add(viewer);Content=root;viewer.ActiveTool=GeoKernelViewerTool.Pan;run.Click+=async(_,_)=>await RunInference();predictionOpacity.ValueChanged+=(_,_)=>ApplyPredictionOpacity();details.Text="Preparing the Bilbao sample...";ContentRendered+=async(_,_)=>await PrepareSamples();
    }
    static FrameworkElement CreateLegend()
    {
        var legend=new StackPanel{Margin=new Thickness(0,8,0,0)};
        legend.Children.Add(new TextBlock{Text="ESA WorldCover classes",FontWeight=FontWeights.Bold});
        var items=new WrapPanel();
        foreach(var item in new (string Name,MediaColor Color)[]{
            ("Tree cover",MediaColor.FromRgb(0,100,0)),("Shrubland",MediaColor.FromRgb(255,187,34)),
            ("Grassland",MediaColor.FromRgb(255,255,76)),("Cropland",MediaColor.FromRgb(240,150,255)),
            ("Built-up",MediaColor.FromRgb(250,0,0)),("Bare / sparse",MediaColor.FromRgb(180,180,180)),
            ("Permanent water",MediaColor.FromRgb(0,100,200))})
        {
            var entry=new StackPanel{Orientation=Orientation.Horizontal,Margin=new Thickness(0,2,12,2)};
            entry.Children.Add(new Border{Width=10,Height=10,Background=new SolidColorBrush(item.Color),Margin=new Thickness(0,3,4,0)});
            entry.Children.Add(new TextBlock{Text=item.Name});
            items.Children.Add(entry);
        }
        legend.Children.Add(items);
        return legend;
    }
    static string Existing(string p)=>Directory.Exists(p)||File.Exists(p)?p:""; void UpdateOutputPath(){if(File.Exists(rasterPath.Text))outputPath.Text=Path.Combine(Path.GetDirectoryName(rasterPath.Text)!,Path.GetFileNameWithoutExtension(rasterPath.Text)+"_landcover_mask.tif");}
    async Task PrepareSamples(){run.IsEnabled=false;try{await Task.Yield();rasterPath.Text=SampleData.EnsureWpfSampleFile(RasterUrl,"bilbao_s2_rgbnir_2021.zip","bilbao_s2_rgbnir_2021","bilbao_s2_rgbnir_2021.tif",this);var manifest=SampleData.EnsureWpfSampleFile(ModelUrl,"landcover-bilbao-model.zip","landcover-bilbao-model","geokernel-model.json",this);modelPath.Text=Path.GetDirectoryName(manifest)!;UpdateOutputPath();OpenBaseRaster();details.Text="Bilbao input raster is open. Run land-cover inference to add the prediction layer.";}catch(Exception ex){details.Text="Sample preparation failed:\n"+ex.Message;MessageBox.Show(this,ex.Message,Title);}finally{run.IsEnabled=true;}}
    void OpenBaseRaster(){predictionLayerIndex=-1;viewer.ClearLayers();if(!viewer.AddLayerFile(rasterPath.Text))throw new InvalidOperationException("The Bilbao input raster could not be opened.");viewer.SetLayerName(0,"Bilbao RGBNIR raster");viewer.RefreshLayers();viewer.FullExtent();status.Text="Map ready.";}
    void ApplyPredictionOpacity(){predictionOpacityText.Text=$"{predictionOpacity.Value:0}%";if(predictionLayerIndex<0)return;viewer.SetLayerOpacity(predictionLayerIndex,predictionOpacity.Value/100d);viewer.RefreshLayers();}
    static void AddPath(Panel panel,string label,TextBox box,RoutedEventHandler browse){panel.Children.Add(new TextBlock{Text=label});var grid=new Grid();grid.ColumnDefinitions.Add(new ColumnDefinition());grid.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(82)});box.Margin=new Thickness(0,0,4,2);Grid.SetColumn(box,0);var b=new Button{Content="Browse...",Margin=new Thickness(0,0,0,2)};b.Click+=browse;Grid.SetColumn(b,1);grid.Children.Add(box);grid.Children.Add(b);panel.Children.Add(grid);}
    void BrowseModel(object s,RoutedEventArgs e){var d=new OpenFolderDialog{InitialDirectory=modelPath.Text};if(d.ShowDialog(this)==true)modelPath.Text=d.FolderName;}
    void BrowseRaster(object s,RoutedEventArgs e){var d=new OpenFileDialog{Filter="GeoTIFF (*.tif;*.tiff)|*.tif;*.tiff",FileName=rasterPath.Text};if(d.ShowDialog(this)==true){rasterPath.Text=d.FileName;UpdateOutputPath();OpenBaseRaster();}}
    void BrowseOutput(object s,RoutedEventArgs e){var d=new SaveFileDialog{Filter="GeoTIFF (*.tif)|*.tif",FileName=outputPath.Text};if(d.ShowDialog(this)==true)outputPath.Text=d.FileName;}
    async Task RunInference(){if(!Directory.Exists(modelPath.Text)||!File.Exists(rasterPath.Text)){MessageBox.Show(this,"Select an existing model package and input raster.",Title);return;}if(string.IsNullOrWhiteSpace(outputPath.Text)){MessageBox.Show(this,"Select a class-mask output path.",Title);return;}run.IsEnabled=false;SetProgress(0,"Opening model package...");details.Text="Validating the model package and preparing tiled inference...";try{var preview=Path.Combine(Path.GetDirectoryName(outputPath.Text)!,Path.GetFileNameWithoutExtension(outputPath.Text)+"_preview.tif");var selected=Enum.Parse<AIExecutionProvider>(provider.SelectedItem!.ToString()!);var reporter=new Progress<AIProgress>(x=>SetProgress(x.Percent,x.Message));var result=await GeoKernelAI.RunRasterInferenceAsync(new AIRasterInferenceRequest{ModelPackagePath=modelPath.Text,RasterPath=rasterPath.Text,OutputPath=outputPath.Text,PreviewOutputPath=preview,ApplyManifestClassCodes=true,ClassPalette=WorldCoverColors,Bands=[1,2,3,4],Provider=selected,OutputMode=AIRasterOutputMode.ClassMask},reporter);SetProgress(95,"Opening color preview...");viewer.RemoveLayerByName("Land-cover prediction");if(!viewer.AddLayerFile(preview))throw new InvalidOperationException("The color preview could not be opened.");predictionLayerIndex=0;viewer.SetLayerName(predictionLayerIndex,"Land-cover prediction");viewer.SetLayerOpacity(predictionLayerIndex,predictionOpacity.Value/100d);viewer.RefreshLayers();details.Text=Diagnostics(result,outputPath.Text,preview);SetProgress(100,"Inference complete");status.Text=$"Land-cover mask created in {result.GetProperty("elapsedMilliseconds").GetInt64()} ms.";}catch(Exception ex){progress.Value=0;progressText.Text="Inference failed";details.Text="Inference failed:\n"+ex.Message;MessageBox.Show(this,ex.Message,Title);}finally{run.IsEnabled=true;}}
    void SetProgress(int value,string text){progress.IsIndeterminate=false;progress.Value=Math.Clamp(value,0,100);progressText.Text=$"{progress.Value}% — {text}";status.Text=text;}
    static string Diagnostics(JsonElement r,string mask,string preview)=>$"GeoKernel AI land-cover inference\n\nProvider: {r.GetProperty("provider").GetString()}\nRaster: {r.GetProperty("width").GetInt32()} x {r.GetProperty("height").GetInt32()}\nTiles: {r.GetProperty("processedTiles").GetInt32()}\nElapsed: {r.GetProperty("elapsedMilliseconds").GetInt64()} ms\n\nClass mask:\n{mask}\n\nColor preview:\n{preview}";
}
