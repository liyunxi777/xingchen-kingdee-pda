using Smobiler.Core;
using System;

namespace ElectricityApp
{
    /// <summary>
    /// 主页导航：app 图标式入口。
    /// 扫码查料 → ScanForm（扫码/手输编码查金蝶物料）；生产任务单 → WorkOrderForm。
    /// </summary>
    partial class HomeForm : Smobiler.Core.Controls.MobileForm
    {
        public HomeForm() : base()
        {
            InitializeComponent();
        }

        /// <summary>扫码查料：进入扫码/手输编码查询界面。</summary>
        private void btnScan_Press(object sender, EventArgs e)
        {
            this.Show(new ScanForm(), null, null);
        }

        /// <summary>生产任务单：进入工单录入界面。</summary>
        private void btnWorkOrder_Press(object sender, EventArgs e)
        {
            this.Show(new WorkOrderForm(), null, null);
        }
    }
}
