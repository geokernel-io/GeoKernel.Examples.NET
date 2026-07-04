namespace GeoKernel.LayerAddRemove.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private Panel toolbarPanel;
    private Button addWorldButton;
    private Button addStatesButton;
    private Button addCitiesButton;
    private Panel addRemoveSeparator;
    private Button removeWorldButton;
    private Button removeStatesButton;
    private Button removeCitiesButton;
    private Panel clearSeparator;
    private Button clearLayersButton;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        layoutPanel = new TableLayoutPanel();
        toolbarPanel = new Panel();
        addWorldButton = new Button();
        addStatesButton = new Button();
        addCitiesButton = new Button();
        addRemoveSeparator = new Panel();
        removeWorldButton = new Button();
        removeStatesButton = new Button();
        removeCitiesButton = new Button();
        clearSeparator = new Panel();
        clearLayersButton = new Button();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        layoutPanel.SuspendLayout();
        toolbarPanel.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // layoutPanel
        // 
        layoutPanel.ColumnCount = 1;
        layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutPanel.Controls.Add(toolbarPanel, 0, 0);
        layoutPanel.Controls.Add(statusStrip, 0, 2);
        layoutPanel.Controls.Add(geoKernelViewerControl, 0, 1);
        layoutPanel.Dock = DockStyle.Fill;
        layoutPanel.Location = new Point(0, 0);
        layoutPanel.Margin = new Padding(0);
        layoutPanel.Name = "layoutPanel";
        layoutPanel.RowCount = 3;
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        layoutPanel.Size = new Size(1200, 800);
        layoutPanel.TabIndex = 0;
        // 
        // toolbarPanel
        // 
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Controls.Add(clearLayersButton);
        toolbarPanel.Controls.Add(clearSeparator);
        toolbarPanel.Controls.Add(removeCitiesButton);
        toolbarPanel.Controls.Add(removeStatesButton);
        toolbarPanel.Controls.Add(removeWorldButton);
        toolbarPanel.Controls.Add(addRemoveSeparator);
        toolbarPanel.Controls.Add(addCitiesButton);
        toolbarPanel.Controls.Add(addStatesButton);
        toolbarPanel.Controls.Add(addWorldButton);
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Padding = new Padding(0);
        toolbarPanel.Size = new Size(1200, 32);
        toolbarPanel.TabIndex = 0;
        // 
        // addWorldButton
        // 
        addWorldButton.FlatStyle = FlatStyle.System;
        addWorldButton.Location = new Point(0, 0);
        addWorldButton.Margin = new Padding(0);
        addWorldButton.Name = "addWorldButton";
        addWorldButton.Size = new Size(82, 32);
        addWorldButton.TabIndex = 0;
        addWorldButton.Text = "Add World";
        addWorldButton.UseVisualStyleBackColor = true;
        addWorldButton.Click += addWorldButton_Click;
        // 
        // addStatesButton
        // 
        addStatesButton.FlatStyle = FlatStyle.System;
        addStatesButton.Location = new Point(82, 0);
        addStatesButton.Margin = new Padding(0);
        addStatesButton.Name = "addStatesButton";
        addStatesButton.Size = new Size(82, 32);
        addStatesButton.TabIndex = 1;
        addStatesButton.Text = "Add States";
        addStatesButton.UseVisualStyleBackColor = true;
        addStatesButton.Click += addStatesButton_Click;
        // 
        // addCitiesButton
        // 
        addCitiesButton.FlatStyle = FlatStyle.System;
        addCitiesButton.Location = new Point(164, 0);
        addCitiesButton.Margin = new Padding(0);
        addCitiesButton.Name = "addCitiesButton";
        addCitiesButton.Size = new Size(82, 32);
        addCitiesButton.TabIndex = 2;
        addCitiesButton.Text = "Add Cities";
        addCitiesButton.UseVisualStyleBackColor = true;
        addCitiesButton.Click += addCitiesButton_Click;
        // 
        // addRemoveSeparator
        // 
        addRemoveSeparator.BackColor = SystemColors.ControlDark;
        addRemoveSeparator.Location = new Point(252, 5);
        addRemoveSeparator.Margin = new Padding(0);
        addRemoveSeparator.Name = "addRemoveSeparator";
        addRemoveSeparator.Size = new Size(1, 22);
        addRemoveSeparator.TabIndex = 3;
        // 
        // removeWorldButton
        // 
        removeWorldButton.FlatStyle = FlatStyle.System;
        removeWorldButton.Location = new Point(260, 0);
        removeWorldButton.Margin = new Padding(0);
        removeWorldButton.Name = "removeWorldButton";
        removeWorldButton.Size = new Size(106, 32);
        removeWorldButton.TabIndex = 4;
        removeWorldButton.Text = "Remove World";
        removeWorldButton.UseVisualStyleBackColor = true;
        removeWorldButton.Click += removeWorldButton_Click;
        // 
        // removeStatesButton
        // 
        removeStatesButton.FlatStyle = FlatStyle.System;
        removeStatesButton.Location = new Point(366, 0);
        removeStatesButton.Margin = new Padding(0);
        removeStatesButton.Name = "removeStatesButton";
        removeStatesButton.Size = new Size(106, 32);
        removeStatesButton.TabIndex = 5;
        removeStatesButton.Text = "Remove States";
        removeStatesButton.UseVisualStyleBackColor = true;
        removeStatesButton.Click += removeStatesButton_Click;
        // 
        // removeCitiesButton
        // 
        removeCitiesButton.FlatStyle = FlatStyle.System;
        removeCitiesButton.Location = new Point(472, 0);
        removeCitiesButton.Margin = new Padding(0);
        removeCitiesButton.Name = "removeCitiesButton";
        removeCitiesButton.Size = new Size(106, 32);
        removeCitiesButton.TabIndex = 6;
        removeCitiesButton.Text = "Remove Cities";
        removeCitiesButton.UseVisualStyleBackColor = true;
        removeCitiesButton.Click += removeCitiesButton_Click;
        // 
        // clearSeparator
        // 
        clearSeparator.BackColor = SystemColors.ControlDark;
        clearSeparator.Location = new Point(584, 5);
        clearSeparator.Margin = new Padding(0);
        clearSeparator.Name = "clearSeparator";
        clearSeparator.Size = new Size(1, 22);
        clearSeparator.TabIndex = 7;
        // 
        // clearLayersButton
        // 
        clearLayersButton.FlatStyle = FlatStyle.System;
        clearLayersButton.Location = new Point(592, 0);
        clearLayersButton.Margin = new Padding(0);
        clearLayersButton.Name = "clearLayersButton";
        clearLayersButton.Size = new Size(96, 32);
        clearLayersButton.TabIndex = 8;
        clearLayersButton.Text = "Clear Layers";
        clearLayersButton.UseVisualStyleBackColor = true;
        clearLayersButton.Click += clearLayersButton_Click;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.TabIndex = 0;
        // 
        // statusStrip
        // 
        statusStrip.Dock = DockStyle.Fill;
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 776);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1200, 24);
        statusStrip.SizingGrip = false;
        statusStrip.TabIndex = 2;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(52, 19);
        statusLabel.Text = "Layers: 0";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 800);
        Controls.Add(layoutPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "LayerAddRemove";
        Shown += MainForm_Shown;
        layoutPanel.ResumeLayout(false);
        layoutPanel.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
