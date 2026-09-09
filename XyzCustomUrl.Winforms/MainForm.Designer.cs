namespace GeoKernel.XyzCustomUrl.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private FlowLayoutPanel toolbarPanel;
    private Button zoomInButton, zoomOutButton, fullExtentButton, zoomRectButton, panButton, applyButton;
    private Label urlLabel, minLabel, maxLabel;
    private TextBox urlTextBox, detailsTextBox;
    private NumericUpDown minZoomNumeric, maxZoomNumeric;
    private CheckBox localCacheCheckBox;
    private ToolTip toolbarToolTip;
    private SplitContainer splitContainer;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl viewerControl;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;

    protected override void Dispose(bool disposing) { if (disposing) components?.Dispose(); base.Dispose(disposing); }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        layoutPanel = new(); toolbarPanel = new(); zoomInButton = new(); zoomOutButton = new(); fullExtentButton = new();
        zoomRectButton = new(); panButton = new(); applyButton = new(); urlLabel = new(); minLabel = new(); maxLabel = new();
        urlTextBox = new(); minZoomNumeric = new(); maxZoomNumeric = new(); localCacheCheckBox = new(); toolbarToolTip = new(components);
        splitContainer = new(); viewerControl = new(); detailsTextBox = new(); statusStrip = new(); statusLabel = new();
        ((System.ComponentModel.ISupportInitialize)minZoomNumeric).BeginInit(); ((System.ComponentModel.ISupportInitialize)maxZoomNumeric).BeginInit();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit(); splitContainer.Panel1.SuspendLayout(); splitContainer.Panel2.SuspendLayout(); splitContainer.SuspendLayout(); SuspendLayout();
        layoutPanel.ColumnCount = 1; layoutPanel.RowCount = 3; layoutPanel.Dock = DockStyle.Fill; layoutPanel.Margin = Padding.Empty;
        layoutPanel.RowStyles.Add(new(SizeType.Absolute, 40)); layoutPanel.RowStyles.Add(new(SizeType.Percent, 100)); layoutPanel.RowStyles.Add(new(SizeType.Absolute, 22));
        layoutPanel.Controls.Add(toolbarPanel, 0, 0); layoutPanel.Controls.Add(splitContainer, 0, 1); layoutPanel.Controls.Add(statusStrip, 0, 2);
        toolbarPanel.Dock = DockStyle.Fill; toolbarPanel.WrapContents = false; toolbarPanel.Padding = new(4, 2, 0, 0);
        ConfigureIconButton(zoomInButton, resources, "zoomInButton.Image", "Zoom In", zoomInButton_Click);
        ConfigureIconButton(zoomOutButton, resources, "zoomOutButton.Image", "Zoom Out", zoomOutButton_Click);
        ConfigureIconButton(fullExtentButton, resources, "fullExtentButton.Image", "Full Extent", fullExtentButton_Click);
        ConfigureIconButton(zoomRectButton, resources, "zoomRectButton.Image", "Zoom Rectangle", zoomRectButton_Click);
        ConfigureIconButton(panButton, resources, "panButton.Image", "Pan", panButton_Click);
        urlLabel.Text = "URL:"; urlLabel.AutoSize = true; urlLabel.Margin = new(8, 9, 3, 0);
        urlTextBox.Width = 430; urlTextBox.Margin = new(0, 6, 6, 0); urlTextBox.KeyDown += urlTextBox_KeyDown;
        minLabel.Text = "Min:"; minLabel.AutoSize = true; minLabel.Margin = new(2, 9, 3, 0);
        minZoomNumeric.Maximum = 21; minZoomNumeric.Width = 48; minZoomNumeric.Margin = new(0, 6, 6, 0);
        maxLabel.Text = "Max:"; maxLabel.AutoSize = true; maxLabel.Margin = new(2, 9, 3, 0);
        maxZoomNumeric.Maximum = 21; maxZoomNumeric.Width = 48; maxZoomNumeric.Margin = new(0, 6, 6, 0);
        localCacheCheckBox.Text = "Local cache"; localCacheCheckBox.AutoSize = true; localCacheCheckBox.Margin = new(2, 9, 8, 0);
        applyButton.Text = "Apply URL"; applyButton.AutoSize = true; applyButton.Height = 27; applyButton.Margin = new(0, 4, 0, 0); applyButton.Click += applyButton_Click;
        toolbarPanel.Controls.AddRange(new Control[] { zoomInButton, zoomOutButton, fullExtentButton, zoomRectButton, panButton, urlLabel, urlTextBox, minLabel, minZoomNumeric, maxLabel, maxZoomNumeric, localCacheCheckBox, applyButton });
        splitContainer.Dock = DockStyle.Fill; splitContainer.Margin = Padding.Empty; splitContainer.SplitterDistance = 880;
        splitContainer.Panel1.Controls.Add(viewerControl); splitContainer.Panel2.Controls.Add(detailsTextBox); viewerControl.Dock = DockStyle.Fill;
        detailsTextBox.Dock = DockStyle.Fill; detailsTextBox.Multiline = true; detailsTextBox.ReadOnly = true; detailsTextBox.ScrollBars = ScrollBars.Vertical; detailsTextBox.Font = new("Consolas", 9F);
        statusStrip.Items.Add(statusLabel); statusLabel.Text = "XyzCustomUrl ready.";
        ClientSize = new(1280, 800); Controls.Add(layoutPanel); Icon = (Icon)resources.GetObject("$this.Icon"); Name = "MainForm"; Text = "XyzCustomUrl"; StartPosition = FormStartPosition.CenterScreen; Shown += MainForm_Shown;
        ((System.ComponentModel.ISupportInitialize)minZoomNumeric).EndInit(); ((System.ComponentModel.ISupportInitialize)maxZoomNumeric).EndInit();
        splitContainer.Panel1.ResumeLayout(false); splitContainer.Panel2.ResumeLayout(false); ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit(); splitContainer.ResumeLayout(false); ResumeLayout(false);
    }

    private void ConfigureIconButton(Button button, System.ComponentModel.ComponentResourceManager resources, string key, string tooltip, EventHandler handler)
    {
        button.BackgroundImage = (Image)resources.GetObject(key); button.BackgroundImageLayout = ImageLayout.Center; button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0; button.Size = new(36, 35); button.Margin = Padding.Empty; button.TabStop = false;
        toolbarToolTip.SetToolTip(button, tooltip); button.Click += handler;
    }
}
