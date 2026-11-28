using Smobiler.Core;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace ElectricityApp
{
    /// <summary>
    /// 生产任务单界面：扫码工单号 → 查金蝶主表（jdy/v2/pm/mo_taskbill）→ 填表头四个格子。
    /// 字段映射取自真实返回：bill_no / bill_date / dept_name / creator_name。
    /// 产品明细 / 用料明细待拿到 mo_taskbill_detail 的真实返回后再接；补录暂不上传。
    /// </summary>
    partial class WorkOrderForm : Smobiler.Core.Controls.MobileForm
    {
        // 金蝶查询服务：内部负责 token 缓存与续期
        private readonly KdApiService _kd = new KdApiService();

        public WorkOrderForm() : base()
        {
            InitializeComponent();
        }

        /// <summary>扫码按钮：调起摄像头，扫码结果回到 OnBarcodeRead。</summary>
        private void btnScan_Press(object sender, EventArgs e)
        {
            // 与 ScanForm 一致：本机 Smobiler 版本的 GetBarcode 需要传入回调委托
            barcodeReader1.GetBarcode(new Smobiler.Plugins.BarcodeReaderEventHandler(OnBarcodeRead));
        }

        /// <summary>扫码回调：码值作为工单号查金蝶，成功后填表头。</summary>
        private void OnBarcodeRead(object sender, Smobiler.Plugins.BarcodeReaderEventArgs e)
        {
            string billNo = e.Data == null ? "" : e.Data.ToString().Trim();
            if (string.IsNullOrEmpty(billNo))
            {
                lblBillNoVal.Text = "扫码无内容";
                return;
            }

            // 先把扫到的码显示出来，即使后续查询失败用户也能看到扫到了什么
            lblBillNoVal.Text = billNo;
            try
            {
                string json = _kd.QueryWorkOrder(billNo);
                if (string.IsNullOrWhiteSpace(json))
                {
                    lblBillNoVal.Text = billNo + "（查询失败：token或网络）";
                    return;
                }

                var ser = new JavaScriptSerializer();
                var root = ser.DeserializeObject(json) as Dictionary<string, object>;
                if (root == null)
                {
                    lblBillNoVal.Text = billNo + "（返回格式无法解析）";
                    return;
                }

                // 兼容 {data:{rows:[]}} 与 {rows:[]} 两种结构
                object dataObj;
                if (!root.TryGetValue("data", out dataObj)) dataObj = root;
                var data = dataObj as Dictionary<string, object>;
                if (data == null)
                {
                    lblBillNoVal.Text = billNo + "（无数据节点）";
                    return;
                }

                object rowsObj;
                if (!data.TryGetValue("rows", out rowsObj))
                {
                    lblBillNoVal.Text = billNo + "（接口未返回 rows）";
                    return;
                }
                var rows = rowsObj as object[];
                if (rows == null || rows.Length == 0)
                {
                    lblBillNoVal.Text = billNo + "（金蝶无此工单）";
                    return;
                }

                var row = rows[0] as Dictionary<string, object>;
                if (row == null)
                {
                    lblBillNoVal.Text = billNo + "（数据行无法解析）";
                    return;
                }

                lblBillNoVal.Text = Get(row, "bill_no");
                lblDateVal.Text = Get(row, "bill_date");
                lblWorkshopVal.Text = Get(row, "dept_name");
                lblPlannerVal.Text = Get(row, "creator_name");
            }
            catch (Exception ex)
            {
                lblBillNoVal.Text = billNo + "（查询失败：" + ex.Message + "）";
            }
        }

        /// <summary>安全取字段值：字段缺失或为 null 时返回空串。</summary>
        private static string Get(Dictionary<string, object> row, string key)
        {
            object v;
            return row.TryGetValue(key, out v) && v != null ? v.ToString() : "";
        }

        /// <summary>保存补录：空壳阶段只提示，不写数据、不上传。</summary>
        private void btnSave_Press(object sender, EventArgs e)
        {
            MessageBox.Show("界面骨架已加载；保存与上传功能待接金蝶明细后开放");
        }
    }
}
