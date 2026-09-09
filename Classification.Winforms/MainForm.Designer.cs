namespace GeoKernel.Classification.Winforms;
partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel rootLayout; private FlowLayoutPanel controlsPanel; private ComboBox rendererComboBox; private ComboBox fieldComboBox; private ComboBox methodComboBox; private NumericUpDown classCountNumeric; private NumericUpDown intervalNumeric; private TextBox manualBreaksTextBox; private ComboBox rampComboBox; private ComboBox rampModeComboBox; private ComboBox targetComboBox; private CheckBox reverseCheckBox; private Button applyButton; private Button clearButton; private Button fullExtentButton; private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl; private StatusStrip statusStrip; private ToolStripStatusLabel statusLabel; private ToolStripProgressBar downloadProgressBar;
    protected override void Dispose(bool disposing) { if (disposing && components is not null) components.Dispose(); base.Dispose(disposing); }
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new(typeof(MainForm));
        rootLayout=new();controlsPanel=new();rendererComboBox=new();fieldComboBox=new();methodComboBox=new();classCountNumeric=new();intervalNumeric=new();manualBreaksTextBox=new();rampComboBox=new();rampModeComboBox=new();targetComboBox=new();reverseCheckBox=new();applyButton=new();clearButton=new();fullExtentButton=new();geoKernelViewerControl=new();statusStrip=new();statusLabel=new();downloadProgressBar=new();
        rootLayout.SuspendLayout();controlsPanel.SuspendLayout();statusStrip.SuspendLayout();SuspendLayout();
        rootLayout.ColumnCount=1;rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));rootLayout.Controls.Add(controlsPanel,0,0);rootLayout.Controls.Add(geoKernelViewerControl,0,1);rootLayout.Controls.Add(statusStrip,0,2);rootLayout.Dock=DockStyle.Fill;rootLayout.RowCount=3;rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,92));rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent,100));rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,24));
        controlsPanel.Dock=DockStyle.Fill;controlsPanel.Padding=new Padding(6);controlsPanel.WrapContents=true;
        AddControl("Renderer",rendererComboBox,130);AddControl("Field",fieldComboBox,140);AddControl("Method",methodComboBox,150);AddControl("Classes",classCountNumeric,60);AddControl("Interval",intervalNumeric,80);AddControl("Manual breaks",manualBreaksTextBox,330);AddControl("Ramp",rampComboBox,130);AddControl("Ramp mode",rampModeComboBox,110);AddControl("Render by",targetComboBox,120);
        classCountNumeric.Minimum=2;classCountNumeric.Maximum=12;classCountNumeric.Value=5;intervalNumeric.Maximum=100000000;intervalNumeric.DecimalPlaces=1;manualBreaksTextBox.Text="0, 100000, 500000, 1000000, 5000000, 10000000";
        reverseCheckBox.Text="Reverse";reverseCheckBox.AutoSize=true;controlsPanel.Controls.Add(reverseCheckBox);applyButton.Text="Apply";clearButton.Text="Clear";fullExtentButton.Text="Full Extent";foreach(var b in new[]{applyButton,clearButton,fullExtentButton}){b.AutoSize=true;controlsPanel.Controls.Add(b);}
        rendererComboBox.DropDownStyle=fieldComboBox.DropDownStyle=methodComboBox.DropDownStyle=rampComboBox.DropDownStyle=rampModeComboBox.DropDownStyle=targetComboBox.DropDownStyle=ComboBoxStyle.DropDownList;
        rendererComboBox.SelectedIndexChanged+=rendererComboBox_SelectedIndexChanged;methodComboBox.SelectedIndexChanged+=methodComboBox_SelectedIndexChanged;applyButton.Click+=applyButton_Click;clearButton.Click+=clearButton_Click;fullExtentButton.Click+=fullExtentButton_Click;
        geoKernelViewerControl.Dock=DockStyle.Fill;geoKernelViewerControl.BackColor=Color.White;
        statusStrip.Items.AddRange([statusLabel,downloadProgressBar]);statusStrip.Dock=DockStyle.Fill;statusStrip.SizingGrip=false;statusLabel.Spring=true;statusLabel.Text="Ready";statusLabel.TextAlign=ContentAlignment.MiddleLeft;downloadProgressBar.Size=new Size(180,18);downloadProgressBar.Visible=false;
        AutoScaleDimensions=new SizeF(7,15);AutoScaleMode=AutoScaleMode.Font;ClientSize=new Size(1280,820);Controls.Add(rootLayout);Icon=(Icon)resources.GetObject("$this.Icon");Name="MainForm";StartPosition=FormStartPosition.CenterScreen;Text="Classification";Shown+=MainForm_Shown;
        rootLayout.ResumeLayout(false);rootLayout.PerformLayout();controlsPanel.ResumeLayout(false);controlsPanel.PerformLayout();statusStrip.ResumeLayout(false);statusStrip.PerformLayout();ResumeLayout(false);
    }
    private void AddControl(string caption, Control control, int width) { var label=new Label{Text=caption,AutoSize=true,Margin=new Padding(6,7,2,0)};control.Width=width;control.Margin=new Padding(2,3,8,3);controlsPanel.Controls.Add(label);controlsPanel.Controls.Add(control); }
}
