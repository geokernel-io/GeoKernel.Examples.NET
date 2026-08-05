namespace GeoKernel.SetAttributes.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private FlowLayoutPanel toolbarPanel;
    private Button applyButton;
    private Button undoButton;
    private Button redoButton;
    private Button resetButton;
    private Button fullExtentButton;
    private Label stateLabel;
    private SplitContainer splitContainer;
    private TableLayoutPanel editorLayout;
    private GroupBox attributeGroupBox;
    private TableLayoutPanel formLayout;
    private Label nameLabel;
    private TextBox nameTextBox;
    private Label statusFieldLabel;
    private ComboBox statusComboBox;
    private Label priorityLabel;
    private NumericUpDown priorityNumeric;
    private Label gridLabel;
    private DataGridView featureGrid;
    private DataGridViewTextBoxColumn idColumn;
    private DataGridViewTextBoxColumn nameColumn;
    private DataGridViewTextBoxColumn statusColumn;
    private DataGridViewTextBoxColumn priorityColumn;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusText;
    private ToolStripProgressBar downloadProgressBar;
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
        components = new System.ComponentModel.Container();
        toolbarPanel = new FlowLayoutPanel();
        applyButton = new Button();
        undoButton = new Button();
        redoButton = new Button();
        resetButton = new Button();
        fullExtentButton = new Button();
        stateLabel = new Label();
        splitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        editorLayout = new TableLayoutPanel();
        attributeGroupBox = new GroupBox();
        formLayout = new TableLayoutPanel();
        nameLabel = new Label();
        nameTextBox = new TextBox();
        statusFieldLabel = new Label();
        statusComboBox = new ComboBox();
        priorityLabel = new Label();
        priorityNumeric = new NumericUpDown();
        gridLabel = new Label();
        featureGrid = new DataGridView();
        idColumn = new DataGridViewTextBoxColumn();
        nameColumn = new DataGridViewTextBoxColumn();
        statusColumn = new DataGridViewTextBoxColumn();
        priorityColumn = new DataGridViewTextBoxColumn();
        statusStrip = new StatusStrip();
        statusText = new ToolStripStatusLabel();
        downloadProgressBar = new ToolStripProgressBar();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        editorLayout.SuspendLayout();
        attributeGroupBox.SuspendLayout();
        formLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)priorityNumeric).BeginInit();
        ((System.ComponentModel.ISupportInitialize)featureGrid).BeginInit();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // toolbarPanel
        // 
        toolbarPanel.AutoSize = false;
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Dock = DockStyle.Top;
        toolbarPanel.FlowDirection = FlowDirection.LeftToRight;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Padding = new Padding(0);
        toolbarPanel.Controls.AddRange(new Control[] { applyButton, undoButton, redoButton, resetButton, fullExtentButton, stateLabel });
        toolbarPanel.Size = new Size(980, 32);
        toolbarPanel.TabIndex = 0;
        // 
        // applyButton
        // 
        applyButton.Name = "applyButton";
        applyButton.AutoSize = true;
        applyButton.Height = 28;
        applyButton.Margin = new Padding(0, 2, 4, 2);
        applyButton.Padding = new Padding(8, 0, 8, 0);
        applyButton.UseVisualStyleBackColor = true;
        applyButton.Size = new Size(100, 22);
        applyButton.Text = "Apply Attributes";
        applyButton.Click += applyButton_Click;
        // 
        // undoButton
        // 
        undoButton.Name = "undoButton";
        undoButton.AutoSize = true;
        undoButton.Height = 28;
        undoButton.Margin = new Padding(0, 2, 4, 2);
        undoButton.Padding = new Padding(8, 0, 8, 0);
        undoButton.UseVisualStyleBackColor = true;
        undoButton.Size = new Size(40, 22);
        undoButton.Text = "Undo";
        undoButton.Click += undoButton_Click;
        // 
        // redoButton
        // 
        redoButton.Name = "redoButton";
        redoButton.AutoSize = true;
        redoButton.Height = 28;
        redoButton.Margin = new Padding(0, 2, 4, 2);
        redoButton.Padding = new Padding(8, 0, 8, 0);
        redoButton.UseVisualStyleBackColor = true;
        redoButton.Size = new Size(38, 22);
        redoButton.Text = "Redo";
        redoButton.Click += redoButton_Click;
        // 
        // resetButton
        // 
        resetButton.Name = "resetButton";
        resetButton.AutoSize = true;
        resetButton.Height = 28;
        resetButton.Margin = new Padding(0, 2, 4, 2);
        resetButton.Padding = new Padding(8, 0, 8, 0);
        resetButton.UseVisualStyleBackColor = true;
        resetButton.Size = new Size(39, 22);
        resetButton.Text = "Reset";
        resetButton.Click += resetButton_Click;
        // 
        // fullExtentButton
        // 
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.AutoSize = true;
        fullExtentButton.Height = 28;
        fullExtentButton.Margin = new Padding(0, 2, 4, 2);
        fullExtentButton.Padding = new Padding(8, 0, 8, 0);
        fullExtentButton.UseVisualStyleBackColor = true;
        fullExtentButton.Size = new Size(64, 22);
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // stateLabel
        // 
        stateLabel.Name = "stateLabel";
        stateLabel.AutoSize = true;
        stateLabel.Margin = new Padding(8, 8, 8, 0);
        stateLabel.TextAlign = ContentAlignment.MiddleLeft;
        stateLabel.Size = new Size(145, 22);
        stateLabel.Text = "Editing: OFF | Selected: -";
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.FixedPanel = FixedPanel.Panel2;
        splitContainer.Location = new Point(0, 25);
        splitContainer.Name = "splitContainer";
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(editorLayout);
        splitContainer.Size = new Size(1120, 713);
        splitContainer.SplitterDistance = 784;
        splitContainer.SplitterWidth = 1;
        splitContainer.TabIndex = 1;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(784, 713);
        geoKernelViewerControl.TabIndex = 0;
        // 
        // editorLayout
        // 
        editorLayout.ColumnCount = 1;
        editorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        editorLayout.Controls.Add(attributeGroupBox, 0, 0);
        editorLayout.Controls.Add(gridLabel, 0, 1);
        editorLayout.Controls.Add(featureGrid, 0, 2);
        editorLayout.Dock = DockStyle.Fill;
        editorLayout.Location = new Point(0, 0);
        editorLayout.Name = "editorLayout";
        editorLayout.Padding = new Padding(8);
        editorLayout.RowCount = 3;
        editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 154F));
        editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        editorLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        editorLayout.Size = new Size(335, 713);
        editorLayout.TabIndex = 0;
        // 
        // attributeGroupBox
        // 
        attributeGroupBox.Controls.Add(formLayout);
        attributeGroupBox.Dock = DockStyle.Fill;
        attributeGroupBox.Location = new Point(8, 8);
        attributeGroupBox.Margin = new Padding(0, 0, 0, 8);
        attributeGroupBox.Name = "attributeGroupBox";
        attributeGroupBox.Padding = new Padding(8);
        attributeGroupBox.Size = new Size(319, 146);
        attributeGroupBox.TabIndex = 0;
        attributeGroupBox.TabStop = false;
        attributeGroupBox.Text = "Selected feature attributes";
        // 
        // formLayout
        // 
        formLayout.ColumnCount = 2;
        formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76F));
        formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        formLayout.Controls.Add(nameLabel, 0, 0);
        formLayout.Controls.Add(nameTextBox, 1, 0);
        formLayout.Controls.Add(statusFieldLabel, 0, 1);
        formLayout.Controls.Add(statusComboBox, 1, 1);
        formLayout.Controls.Add(priorityLabel, 0, 2);
        formLayout.Controls.Add(priorityNumeric, 1, 2);
        formLayout.Dock = DockStyle.Fill;
        formLayout.Location = new Point(8, 24);
        formLayout.Name = "formLayout";
        formLayout.RowCount = 4;
        formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        formLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        formLayout.Size = new Size(303, 114);
        formLayout.TabIndex = 0;
        // 
        // nameLabel
        // 
        nameLabel.Dock = DockStyle.Fill;
        nameLabel.Location = new Point(0, 0);
        nameLabel.Margin = new Padding(0);
        nameLabel.Name = "nameLabel";
        nameLabel.Size = new Size(76, 32);
        nameLabel.TabIndex = 0;
        nameLabel.Text = "Name";
        nameLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // nameTextBox
        // 
        nameTextBox.Dock = DockStyle.Fill;
        nameTextBox.Location = new Point(76, 3);
        nameTextBox.Margin = new Padding(0, 3, 0, 0);
        nameTextBox.Name = "nameTextBox";
        nameTextBox.Size = new Size(227, 23);
        nameTextBox.TabIndex = 1;
        // 
        // statusFieldLabel
        // 
        statusFieldLabel.Dock = DockStyle.Fill;
        statusFieldLabel.Location = new Point(0, 32);
        statusFieldLabel.Margin = new Padding(0);
        statusFieldLabel.Name = "statusFieldLabel";
        statusFieldLabel.Size = new Size(76, 32);
        statusFieldLabel.TabIndex = 2;
        statusFieldLabel.Text = "Status";
        statusFieldLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // statusComboBox
        // 
        statusComboBox.Dock = DockStyle.Fill;
        statusComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        statusComboBox.FormattingEnabled = true;
        statusComboBox.Location = new Point(76, 35);
        statusComboBox.Margin = new Padding(0, 3, 0, 0);
        statusComboBox.Name = "statusComboBox";
        statusComboBox.Size = new Size(227, 23);
        statusComboBox.TabIndex = 3;
        // 
        // priorityLabel
        // 
        priorityLabel.Dock = DockStyle.Fill;
        priorityLabel.Location = new Point(0, 64);
        priorityLabel.Margin = new Padding(0);
        priorityLabel.Name = "priorityLabel";
        priorityLabel.Size = new Size(76, 32);
        priorityLabel.TabIndex = 4;
        priorityLabel.Text = "Priority";
        priorityLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // priorityNumeric
        // 
        priorityNumeric.Dock = DockStyle.Fill;
        priorityNumeric.Location = new Point(76, 67);
        priorityNumeric.Margin = new Padding(0, 3, 0, 0);
        priorityNumeric.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
        priorityNumeric.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        priorityNumeric.Name = "priorityNumeric";
        priorityNumeric.Size = new Size(227, 23);
        priorityNumeric.TabIndex = 5;
        priorityNumeric.Value = new decimal(new int[] { 1, 0, 0, 0 });
        // 
        // gridLabel
        // 
        gridLabel.Dock = DockStyle.Fill;
        gridLabel.Location = new Point(8, 162);
        gridLabel.Margin = new Padding(0);
        gridLabel.Name = "gridLabel";
        gridLabel.Size = new Size(319, 24);
        gridLabel.TabIndex = 1;
        gridLabel.Text = "Editable points";
        gridLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // featureGrid
        // 
        featureGrid.AllowUserToAddRows = false;
        featureGrid.AllowUserToDeleteRows = false;
        featureGrid.AllowUserToResizeRows = false;
        featureGrid.BackgroundColor = Color.White;
        featureGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        featureGrid.Columns.AddRange(new DataGridViewColumn[] { idColumn, nameColumn, statusColumn, priorityColumn });
        featureGrid.Dock = DockStyle.Fill;
        featureGrid.Location = new Point(8, 186);
        featureGrid.Margin = new Padding(0);
        featureGrid.MultiSelect = false;
        featureGrid.Name = "featureGrid";
        featureGrid.ReadOnly = true;
        featureGrid.RowHeadersVisible = false;
        featureGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        featureGrid.Size = new Size(319, 519);
        featureGrid.TabIndex = 2;
        featureGrid.SelectionChanged += featureGrid_SelectionChanged;
        // 
        // idColumn
        // 
        idColumn.HeaderText = "#";
        idColumn.Name = "idColumn";
        idColumn.ReadOnly = true;
        idColumn.Width = 42;
        // 
        // nameColumn
        // 
        nameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        nameColumn.HeaderText = "Name";
        nameColumn.Name = "nameColumn";
        nameColumn.ReadOnly = true;
        // 
        // statusColumn
        // 
        statusColumn.HeaderText = "Status";
        statusColumn.Name = "statusColumn";
        statusColumn.ReadOnly = true;
        statusColumn.Width = 78;
        // 
        // priorityColumn
        // 
        priorityColumn.HeaderText = "Priority";
        priorityColumn.Name = "priorityColumn";
        priorityColumn.ReadOnly = true;
        priorityColumn.Width = 64;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { statusText, downloadProgressBar });
        downloadProgressBar.Size = new Size(180, 18);
        downloadProgressBar.Visible = false;
        statusStrip.Location = new Point(0, 738);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1120, 22);
        statusStrip.TabIndex = 2;
        // 
        // statusText
        // 
        statusText.Name = "statusText";
        statusText.Size = new Size(39, 17);
        statusText.Text = "Ready";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1120, 760);
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Controls.Add(toolbarPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "SetAttributes";
        Shown += MainForm_Shown;
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        editorLayout.ResumeLayout(false);
        attributeGroupBox.ResumeLayout(false);
        formLayout.ResumeLayout(false);
        formLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)priorityNumeric).EndInit();
        ((System.ComponentModel.ISupportInitialize)featureGrid).EndInit();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
