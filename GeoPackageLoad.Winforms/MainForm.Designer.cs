namespace GeoKernel.GeoPackageLoad.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel mainLayout;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl viewerControl;
    private TableLayoutPanel rightLayout;
    private TextBox detailsTextBox;
    private DataGridView schemaGrid;
    private DataGridView attributesGrid;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar downloadProgressBar;
    protected override void Dispose(bool disposing)
    { if (disposing && components is not null) components.Dispose(); base.Dispose(disposing); }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new(typeof(MainForm));
        components = new System.ComponentModel.Container(); mainLayout = new TableLayoutPanel();
        viewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl(); rightLayout = new TableLayoutPanel();
        detailsTextBox = new TextBox(); schemaGrid = new DataGridView(); attributesGrid = new DataGridView();
        statusStrip = new StatusStrip(); statusLabel = new ToolStripStatusLabel();
        downloadProgressBar = new ToolStripProgressBar(); mainLayout.SuspendLayout(); rightLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)schemaGrid).BeginInit();
        ((System.ComponentModel.ISupportInitialize)attributesGrid).BeginInit(); statusStrip.SuspendLayout(); SuspendLayout();
        mainLayout.ColumnCount = 2; mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 440F));
        mainLayout.Controls.Add(viewerControl, 0, 0); mainLayout.Controls.Add(rightLayout, 1, 0);
        mainLayout.Dock = DockStyle.Fill; mainLayout.Margin = Padding.Empty; mainLayout.RowCount = 1;
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        viewerControl.Dock = DockStyle.Fill; viewerControl.Margin = Padding.Empty;
        rightLayout.ColumnCount = 1; rightLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rightLayout.Dock = DockStyle.Fill; rightLayout.Padding = new Padding(6); rightLayout.RowCount = 6;
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 190F));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 67F));
        rightLayout.Controls.Add(new Label { Text = "Layer metadata", Dock = DockStyle.Fill }, 0, 0);
        rightLayout.Controls.Add(detailsTextBox, 0, 1);
        rightLayout.Controls.Add(new Label { Text = "Attribute schema", Dock = DockStyle.Fill }, 0, 2);
        rightLayout.Controls.Add(schemaGrid, 0, 3);
        rightLayout.Controls.Add(new Label { Text = "First 12 attribute rows", Dock = DockStyle.Fill }, 0, 4);
        rightLayout.Controls.Add(attributesGrid, 0, 5);
        detailsTextBox.Dock = DockStyle.Fill; detailsTextBox.Multiline = true; detailsTextBox.ReadOnly = true;
        detailsTextBox.ScrollBars = ScrollBars.Vertical;
        foreach (var grid in new[] { schemaGrid, attributesGrid })
        {
            grid.AllowUserToAddRows = false; grid.AllowUserToDeleteRows = false; grid.ReadOnly = true;
            grid.RowHeadersVisible = false; grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.Dock = DockStyle.Fill; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        }
        schemaGrid.Columns.Add("field", "Field"); schemaGrid.Columns.Add("type", "Type");
        schemaGrid.Columns.Add("length", "Length"); schemaGrid.Columns.Add("decimals", "Decimals");
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, downloadProgressBar });
        statusLabel.Text = "Preparing GeoPackage sample data...";
        downloadProgressBar.Size = new Size(180, 16); downloadProgressBar.Visible = false;
        AutoScaleDimensions = new SizeF(7F, 15F); AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 760); MinimumSize = new Size(980, 640);
        Controls.Add(mainLayout); Controls.Add(statusStrip); Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm"; StartPosition = FormStartPosition.CenterScreen; Text = "GeoPackageLoad";
        Shown += MainForm_Shown; mainLayout.ResumeLayout(false); rightLayout.ResumeLayout(false);
        rightLayout.PerformLayout(); ((System.ComponentModel.ISupportInitialize)schemaGrid).EndInit();
        ((System.ComponentModel.ISupportInitialize)attributesGrid).EndInit(); statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout(); ResumeLayout(false); PerformLayout();
    }
}
