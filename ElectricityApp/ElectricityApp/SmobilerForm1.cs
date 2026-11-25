using Smobiler.Core;
using Smobiler.Core.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectricityApp
{
    partial class SmobilerForm1 : Smobiler.Core.Controls.MobileForm
    {
        // 金蝶查询服务：复用 LrGetToken 取数，凭证从 app.config 读取
        private readonly KdApiService _kd = new KdApiService();

        public SmobilerForm1() : base()
        {
            InitializeComponent();
        }

        // 保留原 hello world 测试按钮
        private void button1_Press(object sender, EventArgs e)
        {
            MessageBox.Show("hello world");
        }

        /// <summary>手输编码查询：TextBox 输入 → 查金蝶 → Label 显示。</summary>
        private void btnQuery_Press(object sender, EventArgs e)
        {
            QueryAndShow(txtCode.Text);
        }

        /// <summary>扫码按钮：调起摄像头，扫码结果回到 OnBarcodeRead。</summary>
        private void btnScan_Press(object sender, EventArgs e)
        {
            // 本机 Smobiler 版本的 GetBarcode 需要传入回调委托
            barcodeReader1.GetBarcode(new Smobiler.Plugins.BarcodeReaderEventHandler(OnBarcodeRead));
        }

        /// <summary>扫码回调：把码值填入输入框并立即查询。</summary>
        private void OnBarcodeRead(object sender, Smobiler.Plugins.BarcodeReaderEventArgs e)
        {
            string code = e.Data == null ? "" : e.Data.ToString();
            txtCode.Text = code;
            QueryAndShow(code);
        }

        /// <summary>清空格子表单的所有值栏。</summary>
        private void ClearCells()
        {
            lblCodeVal.Text = "";
            lblNameVal.Text = "";
            lblSpecVal.Text = "";
            lblUnitVal.Text = "";
            lblQtyVal.Text = "";
        }

        /// <summary>查询并把结果显示到格子表单（同步调用）。状态提示用 lblResult。</summary>
        private void QueryAndShow(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                lblResult.Text = "请输入或扫描编码";
                ClearCells();
                return;
            }

            lblResult.Text = "查询中，请稍候...";
            try
            {
                string json = _kd.QueryByCode(code);
                if (string.IsNullOrWhiteSpace(json))
                {
                    lblResult.Text = "查询失败：接口无返回（检查 token 或网络）";
                    ClearCells();
                    return;
                }

                System.Data.DataTable dt = _kd.BuildResultTable(json);
                if (dt.Rows.Count == 0)
                {
                    lblResult.Text = "未查到数据";
                    ClearCells();
                    return;
                }

                // 取第一条填入格子表单的空栏；多条时显示条数提示
                System.Data.DataRow row = dt.Rows[0];
                lblCodeVal.Text = row["number"] == null ? "" : row["number"].ToString();
                lblNameVal.Text = row["name"] == null ? "" : row["name"].ToString();
                lblSpecVal.Text = row["spec"] == null ? "" : row["spec"].ToString();
                lblUnitVal.Text = row["measure_unit_name"] == null ? "" : row["measure_unit_name"].ToString();
                lblQtyVal.Text = row["qty"] == null ? "" : row["qty"].ToString();
                lblResult.Text = dt.Rows.Count > 1
                    ? "共 " + dt.Rows.Count + " 条，显示第 1 条"
                    : "查询成功";
            }
            catch (Exception ex)
            {
                lblResult.Text = "查询失败：" + ex.Message;
                ClearCells();
            }
        }
    }
}
