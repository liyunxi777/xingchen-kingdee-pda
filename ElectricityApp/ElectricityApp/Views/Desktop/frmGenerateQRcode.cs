using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public partial class frmGenerateQRcode : Form
{
    public frmGenerateQRcode()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 生成服务器连接码：手机扫它连上 Smobiler 服务器（原功能，保留）。
    /// </summary>
    private void btnGenerateQRcode_Click(object sender, EventArgs e)
    {
        try
        {
            qrcodeControl.SetServerInfo(this.txtNetAddress.Text, int.Parse(this.txtTcpPort.Text), int.Parse(this.txtHTTPPort.Text), this.txtName.Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    /// <summary>
    /// 生成物料二维码：二维码内容直接就是物料编码/条码。
    /// 手机扫码后 e.Data 就是这个值，KdApiService.QueryByCode 按 number → barcode → search 查金蝶并列出。
    /// 复用 Smobiler 自带的 QrCodeImgControl，不依赖 Grid++，也不需安装 COM 组件。
    /// </summary>
    private void btnGenMaterialQR_Click(object sender, EventArgs e)
    {
        string code = this.txtMaterialCode.Text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            MessageBox.Show("请先输入物料编码或条码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        // Text 属性就是二维码承载的文本，不要加 http:// 或任何前缀
        this.qrcodeControl.Text = code;
    }
}
