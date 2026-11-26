using Smobiler.Core;
using System;

namespace ElectricityApp
{
    /// <summary>
    /// 生产任务单界面（空壳版）。
    /// 只画布局：表头信息 + 产品明细表 + 用料明细表 + 操作员/备注补录框。
    /// 暂不接金蝶数据、暂不上传——等拿到 mo_taskbill_detail 的真实返回再补查询与填表。
    /// </summary>
    partial class WorkOrderForm : Smobiler.Core.Controls.MobileForm
    {
        public WorkOrderForm() : base()
        {
            InitializeComponent();
        }

        /// <summary>返回上一界面。</summary>
        private void btnBack_Press(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>保存补录：空壳阶段只提示，不写数据、不上传。</summary>
        private void btnSave_Press(object sender, EventArgs e)
        {
            MessageBox.Show("界面骨架已加载；保存与上传功能待接金蝶明细后开放");
        }
    }
}
