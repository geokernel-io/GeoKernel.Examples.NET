#nullable enable
namespace GeoKernel.RasterTileCache.Winforms;
partial class MainForm
{
    private System.ComponentModel.IContainer? components;
    private Button loadDisabledButton=null!, loadSmallButton=null!, loadLargeButton=null!, benchmarkButton=null!, clearCacheButton=null!, fullExtentButton=null!;
    private SplitContainer splitContainer=null!; private global::GeoKernel.NET.WinForms.GeoKernelViewerControl viewerControl=null!; private TextBox detailsTextBox=null!;
    private ToolStripStatusLabel statusLabel=null!; private ToolStripProgressBar progressBar=null!;
    protected override void Dispose(bool disposing) { if(disposing) components?.Dispose(); base.Dispose(disposing); }
    private void InitializeComponent()
    {
        components=new System.ComponentModel.Container(); var resources=new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        var root=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,Margin=Padding.Empty}; root.RowStyles.Add(new(SizeType.Absolute,38)); root.RowStyles.Add(new(SizeType.Percent,100)); root.RowStyles.Add(new(SizeType.Absolute,24));
        var bar=new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=false,Padding=new Padding(4),Margin=Padding.Empty};
        loadDisabledButton=MakeButton("Load Cache Disabled",loadDisabledButton_Click,145); loadSmallButton=MakeButton("Load Small Budget",loadSmallButton_Click,135); loadLargeButton=MakeButton("Load Large Budget",loadLargeButton_Click,135); benchmarkButton=MakeButton("Run Tile Benchmark",benchmarkButton_Click,145); clearCacheButton=MakeButton("Clear Tile Cache",clearCacheButton_Click,120); fullExtentButton=MakeButton("Full Extent",fullExtentButton_Click,85);
        bar.Controls.AddRange([loadDisabledButton,loadSmallButton,loadLargeButton,benchmarkButton,clearCacheButton,fullExtentButton]);
        splitContainer=new SplitContainer{Dock=DockStyle.Fill,SplitterDistance=760,FixedPanel=FixedPanel.Panel2}; viewerControl=new(){Dock=DockStyle.Fill}; detailsTextBox=new(){Dock=DockStyle.Fill,Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,Font=new Font("Consolas",9F)}; splitContainer.Panel1.Controls.Add(viewerControl); splitContainer.Panel2.Controls.Add(detailsTextBox);
        var status=new StatusStrip(); statusLabel=new("Ready."){Spring=true,TextAlign=ContentAlignment.MiddleLeft}; progressBar=new(){Size=new Size(180,16),Minimum=0,Maximum=100}; status.Items.AddRange([statusLabel,progressBar]);
        root.Controls.Add(bar,0,0); root.Controls.Add(splitContainer,0,1); root.Controls.Add(status,0,2); AutoScaleMode=AutoScaleMode.Font; ClientSize=new Size(1180,760); Controls.Add(root); Icon=(Icon?)resources.GetObject("$this.Icon"); Name="MainForm"; StartPosition=FormStartPosition.CenterScreen; Text="RasterTileCache"; Shown+=MainForm_Shown;
    }
    private static Button MakeButton(string text,EventHandler click,int width){var b=new Button{Text=text,Width=width,Height=27,Margin=new Padding(0,0,6,0)};b.Click+=click;return b;}
}
