using Smobiler.Core;

namespace ElectricityApp
{
    partial class HomeForm : Smobiler.Core.Controls.MobileForm
    {
        #region "HomeForm Designer generated code "

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.lblAppName = new Smobiler.Core.Controls.Label();
            this.lblWelcome = new Smobiler.Core.Controls.Label();
            this.btnScan = new Smobiler.Core.Controls.Button();
            this.btnWorkOrder = new Smobiler.Core.Controls.Button();
            this.lblScanName = new Smobiler.Core.Controls.Label();
            this.lblWorkOrderName = new Smobiler.Core.Controls.Label();
            this.lblFooter = new Smobiler.Core.Controls.Label();
            // 
            // lblAppName
            // 
            this.lblAppName.BackColor = System.Drawing.Color.Black;
            this.lblAppName.ForeColor = System.Drawing.Color.White;
            this.lblAppName.FontSize = 20F;
            this.lblAppName.Location = new System.Drawing.Point(0, 0);
            this.lblAppName.Name = "lblAppName";
            this.lblAppName.Size = new System.Drawing.Size(320, 54);
            this.lblAppName.Text = "星辰金蝶 PDA";
            // 
            // lblWelcome
            // 
            this.lblWelcome.FontSize = 13F;
            this.lblWelcome.ForeColor = System.Drawing.Color.Gray;
            this.lblWelcome.Location = new System.Drawing.Point(10, 62);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.Size = new System.Drawing.Size(300, 26);
            this.lblWelcome.Text = "请选择功能";
            // 
            // btnScan —— 扫码查料 app 图标（黑底白字，圆角方块 + 大字 logo）
            // 
            this.btnScan.BackColor = System.Drawing.Color.Black;
            this.btnScan.ForeColor = System.Drawing.Color.White;
            this.btnScan.FontSize = 46F;
            this.btnScan.BorderRadius = 18;
            this.btnScan.HorizontalAlignment = Smobiler.Core.Controls.HorizontalAlignment.Center;
            this.btnScan.Location = new System.Drawing.Point(48, 104);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(105, 105);
            this.btnScan.Text = "扫";
            this.btnScan.Press += new System.EventHandler(this.btnScan_Press);
            // 
            // btnWorkOrder —— 生产任务单 app 图标（白底黑字 + 灰边框，圆角方块 + 大字 logo）
            // 
            this.btnWorkOrder.BackColor = System.Drawing.Color.White;
            this.btnWorkOrder.ForeColor = System.Drawing.Color.Black;
            this.btnWorkOrder.Border = new Smobiler.Core.Controls.Border(1F);
            this.btnWorkOrder.BorderColor = System.Drawing.Color.Gray;
            this.btnWorkOrder.FontSize = 46F;
            this.btnWorkOrder.BorderRadius = 18;
            this.btnWorkOrder.HorizontalAlignment = Smobiler.Core.Controls.HorizontalAlignment.Center;
            this.btnWorkOrder.Location = new System.Drawing.Point(170, 104);
            this.btnWorkOrder.Name = "btnWorkOrder";
            this.btnWorkOrder.Size = new System.Drawing.Size(105, 105);
            this.btnWorkOrder.Text = "单";
            this.btnWorkOrder.Press += new System.EventHandler(this.btnWorkOrder_Press);
            // 
            // lblScanName
            // 
            this.lblScanName.FontSize = 13F;
            this.lblScanName.ForeColor = System.Drawing.Color.DimGray;
            this.lblScanName.Location = new System.Drawing.Point(74, 216);
            this.lblScanName.Name = "lblScanName";
            this.lblScanName.Size = new System.Drawing.Size(80, 26);
            this.lblScanName.Text = "扫码查料";
            // 
            // lblWorkOrderName
            // 
            this.lblWorkOrderName.FontSize = 13F;
            this.lblWorkOrderName.ForeColor = System.Drawing.Color.DimGray;
            this.lblWorkOrderName.Location = new System.Drawing.Point(188, 216);
            this.lblWorkOrderName.Name = "lblWorkOrderName";
            this.lblWorkOrderName.Size = new System.Drawing.Size(80, 26);
            this.lblWorkOrderName.Text = "生产任务单";
            // 
            // lblFooter
            // 
            this.lblFooter.FontSize = 11F;
            this.lblFooter.ForeColor = System.Drawing.Color.Gray;
            this.lblFooter.Location = new System.Drawing.Point(10, 340);
            this.lblFooter.Name = "lblFooter";
            this.lblFooter.Size = new System.Drawing.Size(300, 24);
            this.lblFooter.Text = "v1127 主页导航";
            // 
            // HomeForm
            // 
            this.Controls.AddRange(new Smobiler.Core.Controls.MobileControl[] {
            this.lblAppName,
            this.lblWelcome,
            this.btnScan,
            this.btnWorkOrder,
            this.lblScanName,
            this.lblWorkOrderName,
            this.lblFooter});
            this.Name = "HomeForm";

        }
        #endregion

        private Smobiler.Core.Controls.Label lblAppName;
        private Smobiler.Core.Controls.Label lblWelcome;
        private Smobiler.Core.Controls.Button btnScan;
        private Smobiler.Core.Controls.Button btnWorkOrder;
        private Smobiler.Core.Controls.Label lblScanName;
        private Smobiler.Core.Controls.Label lblWorkOrderName;
        private Smobiler.Core.Controls.Label lblFooter;
    }
}
