using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

partial class frmMain : System.Windows.Forms.Form
{

    //Form 重写 Dispose，以清理组件列表。
    [System.Diagnostics.DebuggerNonUserCode()]
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            if (disposing && server != null)
            {
                server.StopServer();
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }

    //Windows 窗体设计器所必需的

    private System.ComponentModel.IContainer components;
    //注意:  以下过程是 Windows 窗体设计器所必需的
    //可以使用 Windows 窗体设计器修改它。  
    //不要使用代码编辑器修改它。
    [System.Diagnostics.DebuggerStepThrough()]
    private void InitializeComponent()
    {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.labNets = new System.Windows.Forms.Label();
            this.combNets = new System.Windows.Forms.ComboBox();
            this.txtNetAddress = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.labTCPPort = new System.Windows.Forms.Label();
            this.labHTTPPort = new System.Windows.Forms.Label();
            this.txtTcpPort = new System.Windows.Forms.TextBox();
            this.txtHTTPPort = new System.Windows.Forms.TextBox();
            this.btnSetting = new System.Windows.Forms.Button();
            this.qrcodeControl = new Smobiler.Utility.Encoding.Windows.Forms.QrCodeImgControl();
            this.btnHelp = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.btnsavename = new System.Windows.Forms.Button();
            this.btnGenerateQRcode = new System.Windows.Forms.Button();
            this.txtName = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.qrcodeControl)).BeginInit();
            this.SuspendLayout();
            // 
            // labNets
            // 
            this.labNets.AutoSize = true;
            this.labNets.Location = new System.Drawing.Point(28, 21);
            this.labNets.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labNets.Name = "labNets";
            this.labNets.Size = new System.Drawing.Size(80, 18);
            this.labNets.TabIndex = 2;
            this.labNets.Text = "选择网卡";
            // 
            // combNets
            // 
            this.combNets.BackColor = System.Drawing.Color.White;
            this.combNets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combNets.FormattingEnabled = true;
            this.combNets.Location = new System.Drawing.Point(135, 15);
            this.combNets.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.combNets.Name = "combNets";
            this.combNets.Size = new System.Drawing.Size(356, 26);
            this.combNets.TabIndex = 3;
            this.combNets.SelectedIndexChanged += new System.EventHandler(this.combNets_SelectedIndexChanged);
            // 
            // txtNetAddress
            // 
            this.txtNetAddress.BackColor = System.Drawing.Color.White;
            this.txtNetAddress.Location = new System.Drawing.Point(135, 86);
            this.txtNetAddress.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtNetAddress.Name = "txtNetAddress";
            this.txtNetAddress.ReadOnly = true;
            this.txtNetAddress.Size = new System.Drawing.Size(354, 28);
            this.txtNetAddress.TabIndex = 4;
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(28, 90);
            this.Label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(98, 18);
            this.Label1.TabIndex = 2;
            this.Label1.Text = "服务器地址";
            // 
            // labTCPPort
            // 
            this.labTCPPort.AutoSize = true;
            this.labTCPPort.Location = new System.Drawing.Point(28, 130);
            this.labTCPPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labTCPPort.Name = "labTCPPort";
            this.labTCPPort.Size = new System.Drawing.Size(71, 18);
            this.labTCPPort.TabIndex = 2;
            this.labTCPPort.Text = "TCP端口";
            // 
            // labHTTPPort
            // 
            this.labHTTPPort.AutoSize = true;
            this.labHTTPPort.Location = new System.Drawing.Point(288, 130);
            this.labHTTPPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labHTTPPort.Name = "labHTTPPort";
            this.labHTTPPort.Size = new System.Drawing.Size(80, 18);
            this.labHTTPPort.TabIndex = 2;
            this.labHTTPPort.Text = "HTTP端口";
            // 
            // txtTcpPort
            // 
            this.txtTcpPort.BackColor = System.Drawing.Color.White;
            this.txtTcpPort.Location = new System.Drawing.Point(135, 126);
            this.txtTcpPort.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtTcpPort.Name = "txtTcpPort";
            this.txtTcpPort.ReadOnly = true;
            this.txtTcpPort.Size = new System.Drawing.Size(112, 28);
            this.txtTcpPort.TabIndex = 4;
            // 
            // txtHTTPPort
            // 
            this.txtHTTPPort.BackColor = System.Drawing.Color.White;
            this.txtHTTPPort.Location = new System.Drawing.Point(376, 126);
            this.txtHTTPPort.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtHTTPPort.Name = "txtHTTPPort";
            this.txtHTTPPort.ReadOnly = true;
            this.txtHTTPPort.Size = new System.Drawing.Size(112, 28);
            this.txtHTTPPort.TabIndex = 4;
            // 
            // btnSetting
            // 
            this.btnSetting.Location = new System.Drawing.Point(32, 166);
            this.btnSetting.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSetting.Name = "btnSetting";
            this.btnSetting.Size = new System.Drawing.Size(165, 34);
            this.btnSetting.TabIndex = 6;
            this.btnSetting.Text = "更多设置";
            this.btnSetting.UseVisualStyleBackColor = true;
            this.btnSetting.Click += new System.EventHandler(this.btnSetting_Click);
            // 
            // qrcodeControl
            // 
            this.qrcodeControl.ErrorCorrectLevel = Smobiler.Utility.Encoding.ErrorCorrectionLevel.M;
            this.qrcodeControl.Image = ((System.Drawing.Image)(resources.GetObject("qrcodeControl.Image")));
            this.qrcodeControl.Location = new System.Drawing.Point(32, 210);
            this.qrcodeControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.qrcodeControl.Name = "qrcodeControl";
            this.qrcodeControl.QuietZoneModule = Smobiler.Utility.Encoding.Windows.Render.QuietZoneModules.Two;
            this.qrcodeControl.Size = new System.Drawing.Size(459, 414);
            this.qrcodeControl.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.qrcodeControl.TabIndex = 7;
            this.qrcodeControl.TabStop = false;
            this.qrcodeControl.Text = "lgdY7aMX70DtIt/TmG1ANg==";
            // 
            // btnHelp
            // 
            this.btnHelp.Location = new System.Drawing.Point(376, 166);
            this.btnHelp.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(114, 34);
            this.btnHelp.TabIndex = 8;
            this.btnHelp.Text = "帮助";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 58);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 18);
            this.label2.TabIndex = 10;
            this.label2.Text = "名称";
            // 
            // btnsavename
            // 
            this.btnsavename.Location = new System.Drawing.Point(393, 50);
            this.btnsavename.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnsavename.Name = "btnsavename";
            this.btnsavename.Size = new System.Drawing.Size(98, 32);
            this.btnsavename.TabIndex = 11;
            this.btnsavename.Text = "保存";
            this.btnsavename.UseVisualStyleBackColor = true;
            this.btnsavename.Click += new System.EventHandler(this.btnsavename_Click);
            // 
            // btnGenerateQRcode
            // 
            this.btnGenerateQRcode.Location = new System.Drawing.Point(206, 166);
            this.btnGenerateQRcode.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnGenerateQRcode.Name = "btnGenerateQRcode";
            this.btnGenerateQRcode.Size = new System.Drawing.Size(165, 34);
            this.btnGenerateQRcode.TabIndex = 12;
            this.btnGenerateQRcode.Text = "生成二维码";
            this.btnGenerateQRcode.UseVisualStyleBackColor = true;
            this.btnGenerateQRcode.Click += new System.EventHandler(this.btnGenerateQRcode_Click);
            // 
            // txtName
            // 
            this.txtName.BackColor = System.Drawing.Color.White;
            this.txtName.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::ElectricityApp.Properties.Settings.Default, "txtName", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtName.Location = new System.Drawing.Point(135, 50);
            this.txtName.Margin = new System.Windows.Forms.Padding(4);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(247, 28);
            this.txtName.TabIndex = 9;
            this.txtName.Text = global::ElectricityApp.Properties.Settings.Default.txtName;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(522, 627);
            this.Controls.Add(this.btnGenerateQRcode);
            this.Controls.Add(this.btnsavename);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.btnSetting);
            this.Controls.Add(this.txtHTTPPort);
            this.Controls.Add(this.txtTcpPort);
            this.Controls.Add(this.txtNetAddress);
            this.Controls.Add(this.combNets);
            this.Controls.Add(this.labHTTPPort);
            this.Controls.Add(this.labTCPPort);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.labNets);
            this.Controls.Add(this.qrcodeControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Smobiler Launcher";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.qrcodeControl)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }
    internal System.Windows.Forms.Label labNets;
    internal System.Windows.Forms.ComboBox combNets;
    internal System.Windows.Forms.TextBox txtNetAddress;
    internal System.Windows.Forms.Label Label1;
    public frmMain()
    {
        InitializeComponent();
    }

    internal System.Windows.Forms.Label labTCPPort;
    internal System.Windows.Forms.Label labHTTPPort;
    internal System.Windows.Forms.TextBox txtTcpPort;
    internal System.Windows.Forms.TextBox txtHTTPPort;
    private System.Windows.Forms.Button btnSetting;
    internal Smobiler.Utility.Encoding.Windows.Forms.QrCodeImgControl qrcodeControl;
    private System.Windows.Forms.Button btnHelp;
    internal System.Windows.Forms.TextBox txtName;
    internal System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button btnsavename;
    private System.Windows.Forms.Button btnGenerateQRcode;
}