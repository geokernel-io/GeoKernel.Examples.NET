using System.IO;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;
namespace GeoKernel.XyzLocalCache.Wpf;
public sealed partial class MainWindow
{
 const string Url="https://tile.openstreetmap.org/{z}/{x}/{y}.png"; static readonly GeoKernelExtent DefaultExtent=new(-1400000,4100000,4200000,7800000);
 public MainWindow()=>InitializeComponent();
 private void Window_Loaded(object s,RoutedEventArgs e){cachePathTextBox.Text=Path.Combine(AppContext.BaseDirectory,"XyzLocalCacheData","osm");cacheCheckBox.IsChecked=true;viewerControl.ActiveTool=GeoKernelViewerTool.Pan;ApplyCache();}
 private void FullExtent_Click(object s,RoutedEventArgs e)=>viewerControl.ViewExtent=DefaultExtent; private void Apply_Click(object s,RoutedEventArgs e)=>ApplyCache(); private void Refresh_Click(object s,RoutedEventArgs e)=>UpdateDetails();
 private void Browse_Click(object s,RoutedEventArgs e){var d=new Microsoft.Win32.OpenFolderDialog{Title="Select XYZ cache directory",InitialDirectory=CacheDirectory()};if(d.ShowDialog(this)==true)cachePathTextBox.Text=d.FolderName;}
 private void Clear_Click(object s,RoutedEventArgs e){var p=CacheDirectory();if(MessageBox.Show(this,$"Clear all cached tiles under:\n{p}","XyzLocalCache",MessageBoxButton.YesNo,MessageBoxImage.Question)!=MessageBoxResult.Yes)return;if(Directory.Exists(p))Directory.Delete(p,true);UpdateDetails();statusText.Text="Cache directory cleared.";}
 private void ApplyCache(){try{var p=CacheDirectory();Directory.CreateDirectory(p);cachePathTextBox.Text=p;viewerControl.ClearLayers();var i=viewerControl.AddXyzLayer("OSM with Local Cache",Url,0,19,256,"OpenStreetMap contributors",cacheCheckBox.IsChecked==true,p);if(i<0)throw new InvalidOperationException("XYZ layer could not be loaded.");viewerControl.ViewExtent=DefaultExtent;UpdateDetails();statusText.Text=cacheCheckBox.IsChecked==true?"XYZ layer loaded with local disk cache.":"XYZ layer loaded with local cache disabled.";}catch(Exception ex){MessageBox.Show(this,ex.Message,"XyzLocalCache",MessageBoxButton.OK,MessageBoxImage.Error);}}
 private string CacheDirectory()=>Path.GetFullPath(string.IsNullOrWhiteSpace(cachePathTextBox.Text)?Path.Combine(AppContext.BaseDirectory,"XyzLocalCacheData","osm"):cachePathTextBox.Text.Trim());
 private void UpdateDetails(){var p=CacheDirectory();var f=Directory.Exists(p)?Directory.EnumerateFiles(p,"*.tile",SearchOption.AllDirectories).ToArray():[];var b=f.Sum(x=>new FileInfo(x).Length);detailsTextBox.Text=string.Join(Environment.NewLine,"XYZ local cache sample","","URL template:",Url,"",$"Local cache: {(cacheCheckBox.IsChecked==true?"enabled":"disabled")}","Configured cache directory:",p,"","Cache contents:",$"Tile files: {f.Length}",$"Size: {FormatBytes(b)}","","SDK flow:","viewerControl.AddXyzLayer(..., localCacheEnabled, cacheDirectory)","","Pan or zoom the map to request tiles. Cached tiles are reused on later runs.");}
 static string FormatBytes(long v)=>v>=1048576?$"{v/1048576d:F2} MB":v>=1024?$"{v/1024d:F1} KB":$"{v} bytes";
}
