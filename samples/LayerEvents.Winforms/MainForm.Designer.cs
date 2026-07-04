namespace GeoKernel.LayerEvents.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private SplitContainer splitContainer;
    private TableLayoutPanel sidePanelLayout;
    private Button addWorldButton;
    private Button addStatesButton;
    private Button addCitiesButton;
    private Button removeSelectedButton;
    private Button clearLayersButton;
    private Button toggleVisibilityButton;
    private Button moveUpButton;
    private Button moveDownButton;
    private Button refreshButton;
    private Button clearLogButton;
    private ListBox layerListBox;
    private TextBox eventLogTextBox;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        splitContainer = new SplitContainer();
        sidePanelLayout = new TableLayoutPanel();
        addWorldButton = new Button();
        addStatesButton = new Button();
        addCitiesButton = new Button();
        removeSelectedButton = new Button();
        clearLayersButton = new Button();
        toggleVisibilityButton = new Button();
        moveUpButton = new Button();
        moveDownButton = new Button();
        refreshButton = new Button();
        clearLogButton = new Button();
        layerListBox = new ListBox();
        eventLogTextBox = new TextBox();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        sidePanelLayout.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.FixedPanel = FixedPanel.Panel1;
        splitContainer.Location = new Point(0, 0);
        splitContainer.Margin = new Padding(0);
        splitContainer.Name = "splitContainer";
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(sidePanelLayout);
        splitContainer.Panel1MinSize = 260;
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(geoKernelViewerControl);
        splitContainer.Size = new Size(1280, 798);
        splitContainer.SplitterDistance = 300;
        splitContainer.TabIndex = 0;
        // 
        // sidePanelLayout
        // 
        sidePanelLayout.ColumnCount = 1;
        sidePanelLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        sidePanelLayout.Controls.Add(addWorldButton, 0, 0);
        sidePanelLayout.Controls.Add(addStatesButton, 0, 1);
        sidePanelLayout.Controls.Add(addCitiesButton, 0, 2);
        sidePanelLayout.Controls.Add(removeSelectedButton, 0, 3);
        sidePanelLayout.Controls.Add(clearLayersButton, 0, 4);
        sidePanelLayout.Controls.Add(toggleVisibilityButton, 0, 5);
        sidePanelLayout.Controls.Add(moveUpButton, 0, 6);
        sidePanelLayout.Controls.Add(moveDownButton, 0, 7);
        sidePanelLayout.Controls.Add(refreshButton, 0, 8);
        sidePanelLayout.Controls.Add(clearLogButton, 0, 9);
        sidePanelLayout.Controls.Add(layerListBox, 0, 10);
        sidePanelLayout.Controls.Add(eventLogTextBox, 0, 11);
        sidePanelLayout.Dock = DockStyle.Fill;
        sidePanelLayout.Location = new Point(0, 0);
        sidePanelLayout.Name = "sidePanelLayout";
        sidePanelLayout.Padding = new Padding(8);
        sidePanelLayout.RowCount = 12;
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        sidePanelLayout.Size = new Size(300, 798);
        sidePanelLayout.TabIndex = 0;
        // 
        // addWorldButton
        // 
        addWorldButton.Dock = DockStyle.Fill;
        addWorldButton.Location = new Point(8, 8);
        addWorldButton.Margin = new Padding(0, 0, 0, 6);
        addWorldButton.Name = "addWorldButton";
        addWorldButton.Size = new Size(284, 26);
        addWorldButton.TabIndex = 0;
        addWorldButton.Text = "Add World";
        addWorldButton.UseVisualStyleBackColor = true;
        addWorldButton.Click += addWorldButton_Click;
        // 
        // addStatesButton
        // 
        addStatesButton.Dock = DockStyle.Fill;
        addStatesButton.Location = new Point(8, 40);
        addStatesButton.Margin = new Padding(0, 0, 0, 6);
        addStatesButton.Name = "addStatesButton";
        addStatesButton.Size = new Size(284, 26);
        addStatesButton.TabIndex = 1;
        addStatesButton.Text = "Add States";
        addStatesButton.UseVisualStyleBackColor = true;
        addStatesButton.Click += addStatesButton_Click;
        // 
        // addCitiesButton
        // 
        addCitiesButton.Dock = DockStyle.Fill;
        addCitiesButton.Location = new Point(8, 72);
        addCitiesButton.Margin = new Padding(0, 0, 0, 6);
        addCitiesButton.Name = "addCitiesButton";
        addCitiesButton.Size = new Size(284, 26);
        addCitiesButton.TabIndex = 2;
        addCitiesButton.Text = "Add Cities";
        addCitiesButton.UseVisualStyleBackColor = true;
        addCitiesButton.Click += addCitiesButton_Click;
        // 
        // removeSelectedButton
        // 
        removeSelectedButton.Dock = DockStyle.Fill;
        removeSelectedButton.Location = new Point(8, 104);
        removeSelectedButton.Margin = new Padding(0, 0, 0, 6);
        removeSelectedButton.Name = "removeSelectedButton";
        removeSelectedButton.Size = new Size(284, 26);
        removeSelectedButton.TabIndex = 3;
        removeSelectedButton.Text = "Remove Selected";
        removeSelectedButton.UseVisualStyleBackColor = true;
        removeSelectedButton.Click += removeSelectedButton_Click;
        // 
        // clearLayersButton
        // 
        clearLayersButton.Dock = DockStyle.Fill;
        clearLayersButton.Location = new Point(8, 136);
        clearLayersButton.Margin = new Padding(0, 0, 0, 6);
        clearLayersButton.Name = "clearLayersButton";
        clearLayersButton.Size = new Size(284, 26);
        clearLayersButton.TabIndex = 4;
        clearLayersButton.Text = "Clear Layers";
        clearLayersButton.UseVisualStyleBackColor = true;
        clearLayersButton.Click += clearLayersButton_Click;
        // 
        // toggleVisibilityButton
        // 
        toggleVisibilityButton.Dock = DockStyle.Fill;
        toggleVisibilityButton.Location = new Point(8, 168);
        toggleVisibilityButton.Margin = new Padding(0, 0, 0, 6);
        toggleVisibilityButton.Name = "toggleVisibilityButton";
        toggleVisibilityButton.Size = new Size(284, 26);
        toggleVisibilityButton.TabIndex = 5;
        toggleVisibilityButton.Text = "Toggle Visibility";
        toggleVisibilityButton.UseVisualStyleBackColor = true;
        toggleVisibilityButton.Click += toggleVisibilityButton_Click;
        // 
        // moveUpButton
        // 
        moveUpButton.Dock = DockStyle.Fill;
        moveUpButton.Location = new Point(8, 200);
        moveUpButton.Margin = new Padding(0, 0, 0, 6);
        moveUpButton.Name = "moveUpButton";
        moveUpButton.Size = new Size(284, 26);
        moveUpButton.TabIndex = 6;
        moveUpButton.Text = "Move Up";
        moveUpButton.UseVisualStyleBackColor = true;
        moveUpButton.Click += moveUpButton_Click;
        // 
        // moveDownButton
        // 
        moveDownButton.Dock = DockStyle.Fill;
        moveDownButton.Location = new Point(8, 232);
        moveDownButton.Margin = new Padding(0, 0, 0, 6);
        moveDownButton.Name = "moveDownButton";
        moveDownButton.Size = new Size(284, 26);
        moveDownButton.TabIndex = 7;
        moveDownButton.Text = "Move Down";
        moveDownButton.UseVisualStyleBackColor = true;
        moveDownButton.Click += moveDownButton_Click;
        // 
        // refreshButton
        // 
        refreshButton.Dock = DockStyle.Fill;
        refreshButton.Location = new Point(8, 264);
        refreshButton.Margin = new Padding(0, 0, 0, 6);
        refreshButton.Name = "refreshButton";
        refreshButton.Size = new Size(284, 26);
        refreshButton.TabIndex = 8;
        refreshButton.Text = "Refresh";
        refreshButton.UseVisualStyleBackColor = true;
        refreshButton.Click += refreshButton_Click;
        // 
        // clearLogButton
        // 
        clearLogButton.Dock = DockStyle.Fill;
        clearLogButton.Location = new Point(8, 296);
        clearLogButton.Margin = new Padding(0, 0, 0, 6);
        clearLogButton.Name = "clearLogButton";
        clearLogButton.Size = new Size(284, 26);
        clearLogButton.TabIndex = 9;
        clearLogButton.Text = "Clear Log";
        clearLogButton.UseVisualStyleBackColor = true;
        clearLogButton.Click += clearLogButton_Click;
        // 
        // layerListBox
        // 
        layerListBox.Dock = DockStyle.Fill;
        layerListBox.FormattingEnabled = true;
        layerListBox.Location = new Point(8, 328);
        layerListBox.Margin = new Padding(0, 0, 0, 8);
        layerListBox.Name = "layerListBox";
        layerListBox.Size = new Size(284, 186);
        layerListBox.TabIndex = 10;
        // 
        // eventLogTextBox
        // 
        eventLogTextBox.Dock = DockStyle.Fill;
        eventLogTextBox.Location = new Point(8, 522);
        eventLogTextBox.Margin = new Padding(0);
        eventLogTextBox.Multiline = true;
        eventLogTextBox.Name = "eventLogTextBox";
        eventLogTextBox.ReadOnly = true;
        eventLogTextBox.ScrollBars = ScrollBars.Vertical;
        eventLogTextBox.Size = new Size(284, 268);
        eventLogTextBox.TabIndex = 11;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(976, 798);
        geoKernelViewerControl.TabIndex = 0;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 798);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1280, 22);
        statusStrip.SizingGrip = false;
        statusStrip.TabIndex = 1;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(39, 17);
        statusLabel.Text = "Ready";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1280, 820);
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "LayerEvents";
        Shown += MainForm_Shown;
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        sidePanelLayout.ResumeLayout(false);
        sidePanelLayout.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
