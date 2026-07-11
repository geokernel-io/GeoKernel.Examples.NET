using System.Diagnostics;
using GeoKernel.NET.WinForms;

namespace GeoKernel.LayerLoadCancel.Winforms;

public sealed partial class MainForm : Form
{
    private bool _cancelRequested;
    private bool _isLoading;
    private bool _isPumpingMessages;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {        
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        SetProgress(0);
    }

    private void loadButton_Click(object sender, EventArgs e)
    {
        LoadLargeLayer();
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
        if (!_isLoading)
            return;

        _cancelRequested = true;
        cancelButton.Enabled = false;
        statusLabel.Text = "Cancel requested...";
    }

    private void clearButton_Click(object sender, EventArgs e)
    {
        if (_isLoading)
            return;

        _cancelRequested = false;
        geoKernelViewerControl.ClearLayers();
        SetProgress(0);
        loadButton.Enabled = true;
        cancelButton.Enabled = false;
        SetStatus("Layers cleared.");
    }

    private void LoadLargeLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "output_1m_points.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"Layer file could not be found:{Environment.NewLine}{path}",
                "LayerLoadCancel",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        _cancelRequested = false;
        _isLoading = true;
        loadButton.Enabled = false;
        cancelButton.Enabled = true;
        clearButton.Enabled = false;
        SetProgress(0);
        SetStatus("Layer load started...");

        UseWaitCursor = true;
        var stopwatch = Stopwatch.StartNew();
        try
        {
            geoKernelViewerControl.ClearLayers();
            var loaded = geoKernelViewerControl.AddLayerFile(
                path,
                CreateLoadOptions(),
                new Progress<GeoKernelLayerLoadProgress>(progress =>
                {
                    if (progress.Progress.HasValue)
                        SetProgress(progress.Progress.Value);
                    if (!string.IsNullOrWhiteSpace(progress.Status))
                        SetStatus(progress.Status);
                }),
                isCancellationRequested: () =>
                {
                    PumpMessagesForCancel();
                    return _cancelRequested;
                },
                spatialIndexState: new Progress<GeoKernelSpatialIndexPreparationState>(state =>
                    SetStatus(SpatialIndexStateText(state))));

            stopwatch.Stop();

            if (_cancelRequested)
            {
                SetProgress(0);
                SetStatus($"Layer load cancelled after {stopwatch.ElapsedMilliseconds} ms.");
                return;
            }

            if (!loaded)
            {
                SetProgress(0);
                SetStatus("Layer load failed.");
                MessageBox.Show(
                    this,
                    $"Layer could not be loaded:{Environment.NewLine}{path}",
                    "LayerLoadCancel",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var layer = geoKernelViewerControl.GetLayerInfo(0);
            if (layer is not null)
                geoKernelViewerControl.SetLayerName(layer.Index, "One Million Points");

            geoKernelViewerControl.FullExtent();
            SetProgress(100);
            SetStatus($"Layer loaded in {stopwatch.ElapsedMilliseconds} ms.");
        }
        finally
        {
            _isLoading = false;
            UseWaitCursor = false;
            loadButton.Enabled = true;
            cancelButton.Enabled = false;
            clearButton.Enabled = true;
        }
    }

    private static GeoKernelLayerLoadOptions CreateLoadOptions()
    {
        return new GeoKernelLayerLoadOptions
        {
            UseSpatialIndex = true,
            SpatialIndexType = GeoKernelSpatialIndexType.RTree,
            BuildFeatureSource = true,
            ApplyDefaultStyle = true,
            DefaultStyle = new GeoKernelLayerStyle
            {
                FillColor = "#D8E5E1",
                FillOpacity = 210,
                LineColor = "#607D78",
                LineWidth = 0.9,
                PointColor = "#2D82B7",
                PointSize = 3.5
            }
        };
    }

    private void SetProgress(int value)
    {
        progressBar.Value = Math.Clamp(value, 0, 100);
    }

    private void SetStatus(string text)
    {
        statusLabel.Text = text;
    }

    private void PumpMessagesForCancel()
    {
        if (_isPumpingMessages)
            return;

        _isPumpingMessages = true;
        try
        {
            Application.DoEvents();
        }
        finally
        {
            _isPumpingMessages = false;
        }
    }

    private static string SpatialIndexStateText(GeoKernelSpatialIndexPreparationState state)
    {
        return state switch
        {
            GeoKernelSpatialIndexPreparationState.Loading => "Spatial index is loading...",
            GeoKernelSpatialIndexPreparationState.BuildingLocator => "Feature locators are preparing...",
            GeoKernelSpatialIndexPreparationState.BuildingIndex => "Spatial index is building...",
            GeoKernelSpatialIndexPreparationState.Ready => "Spatial index is ready.",
            GeoKernelSpatialIndexPreparationState.Cancelled => "Load cancelled while preparing spatial index.",
            GeoKernelSpatialIndexPreparationState.Failed => "Spatial index failed.",
            _ => "Spatial index idle."
        };
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "assets", "data")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return AppContext.BaseDirectory;
    }
}
