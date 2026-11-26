using Smobiler.Core;

namespace ElectricityApp
{
    partial class SmobilerForm1 : Smobiler.Core.Controls.MobileForm
    {
        #region "SmobilerForm Designer generated code "

        //SmobilerForm overrides dispose to clean up the component list.
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }


        //NOTE: The following procedure is required by the SmobilerForm Designer
        //It can be modified using the SmobilerForm Designer.  
        //Do not modify it using the code editor.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.button1 = new Smobiler.Core.Controls.Button();
            this.btnWorkOrder = new Smobiler.Core.Controls.Button();
            this.txtCode = new Smobiler.Core.Controls.TextBox();
            this.btnQuery = new Smobiler.Core.Controls.Button();
            this.btnScan = new Smobiler.Core.Controls.Button();
            this.lblResult = new Smobiler.Core.Controls.Label();
            this.barcodeReader1 = new Smobiler.Plugins.BarcodeReader();
            this.pnlCode = new Smobiler.Core.Controls.Panel();
            this.lblCodeHead = new Smobiler.Core.Controls.Label();
            this.lblCodeVal = new Smobiler.Core.Controls.Label();
            this.pnlName = new Smobiler.Core.Controls.Panel();
            this.lblNameHead = new Smobiler.Core.Controls.Label();
            this.lblNameVal = new Smobiler.Core.Controls.Label();
            this.pnlSpec = new Smobiler.Core.Controls.Panel();
            this.lblSpecHead = new Smobiler.Core.Controls.Label();
            this.lblSpecVal = new Smobiler.Core.Controls.Label();
            this.pnlUnit = new Smobiler.Core.Controls.Panel();
            this.lblUnitHead = new Smobiler.Core.Controls.Label();
            this.lblUnitVal = new Smobiler.Core.Controls.Label();
            this.pnlQty = new Smobiler.Core.Controls.Panel();
            this.lblQtyHead = new Smobiler.Core.Controls.Label();
            this.lblQtyVal = new Smobiler.Core.Controls.Label();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(10, 446);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 35);
            this.button1.Text = "v1125新版";
            this.button1.Press += new System.EventHandler(this.button1_Press);
            // 
            // btnWorkOrder
            // 
            this.btnWorkOrder.Location = new System.Drawing.Point(10, 490);
            this.btnWorkOrder.Name = "btnWorkOrder";
            this.btnWorkOrder.Size = new System.Drawing.Size(300, 36);
            this.btnWorkOrder.Text = "工单录入（空壳）";
            this.btnWorkOrder.Press += new System.EventHandler(this.btnWorkOrder_Press);
            // 
            // txtCode
            // 
            this.txtCode.Location = new System.Drawing.Point(10, 58);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(215, 32);
            // 
            // btnQuery
            // 
            this.btnQuery.Location = new System.Drawing.Point(225, 58);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(80, 32);
            this.btnQuery.Text = "查询";
            this.btnQuery.Press += new System.EventHandler(this.btnQuery_Press);
            // 
            // btnScan
            // 
            this.btnScan.Location = new System.Drawing.Point(10, 114);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(300, 36);
            this.btnScan.Text = "扫码查询";
            this.btnScan.Press += new System.EventHandler(this.btnScan_Press);
            // 
            // lblResult
            // 
            this.lblResult.Location = new System.Drawing.Point(10, 156);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(300, 28);
            this.lblResult.Text = "新版已加载，请输入或扫码查询";
            // 
            // barcodeReader1
            // 
            this.barcodeReader1.Name = "barcodeReader1";
            // 
            // pnlCode
            // 
            this.pnlCode.BackColor = System.Drawing.Color.White;
            this.pnlCode.Border = new Smobiler.Core.Controls.Border(1F);
            this.pnlCode.BorderColor = System.Drawing.Color.Gray;
            this.pnlCode.Controls.AddRange(new Smobiler.Core.Controls.MobileControl[] {
            this.lblCodeHead,
            this.lblCodeVal});
            this.pnlCode.Location = new System.Drawing.Point(10, 190);
            this.pnlCode.Name = "pnlCode";
            this.pnlCode.Size = new System.Drawing.Size(144, 60);
            // 
            // lblCodeHead
            // 
            this.lblCodeHead.BackColor = System.Drawing.Color.Silver;
            this.lblCodeHead.Location = new System.Drawing.Point(2, 2);
            this.lblCodeHead.Name = "lblCodeHead";
            this.lblCodeHead.Size = new System.Drawing.Size(138, 22);
            this.lblCodeHead.Text = "编码";
            // 
            // lblCodeVal
            // 
            this.lblCodeVal.Location = new System.Drawing.Point(2, 26);
            this.lblCodeVal.Name = "lblCodeVal";
            this.lblCodeVal.Size = new System.Drawing.Size(138, 30);
            // 
            // pnlName
            // 
            this.pnlName.BackColor = System.Drawing.Color.White;
            this.pnlName.Border = new Smobiler.Core.Controls.Border(1F);
            this.pnlName.BorderColor = System.Drawing.Color.Gray;
            this.pnlName.Controls.AddRange(new Smobiler.Core.Controls.MobileControl[] {
            this.lblNameHead,
            this.lblNameVal});
            this.pnlName.Location = new System.Drawing.Point(158, 190);
            this.pnlName.Name = "pnlName";
            this.pnlName.Size = new System.Drawing.Size(144, 60);
            // 
            // lblNameHead
            // 
            this.lblNameHead.BackColor = System.Drawing.Color.Silver;
            this.lblNameHead.Location = new System.Drawing.Point(2, 2);
            this.lblNameHead.Name = "lblNameHead";
            this.lblNameHead.Size = new System.Drawing.Size(138, 22);
            this.lblNameHead.Text = "名称";
            // 
            // lblNameVal
            // 
            this.lblNameVal.Location = new System.Drawing.Point(2, 26);
            this.lblNameVal.Name = "lblNameVal";
            this.lblNameVal.Size = new System.Drawing.Size(138, 30);
            // 
            // pnlSpec
            // 
            this.pnlSpec.BackColor = System.Drawing.Color.White;
            this.pnlSpec.Border = new Smobiler.Core.Controls.Border(1F);
            this.pnlSpec.BorderColor = System.Drawing.Color.Gray;
            this.pnlSpec.Controls.AddRange(new Smobiler.Core.Controls.MobileControl[] {
            this.lblSpecHead,
            this.lblSpecVal});
            this.pnlSpec.Location = new System.Drawing.Point(10, 256);
            this.pnlSpec.Name = "pnlSpec";
            this.pnlSpec.Size = new System.Drawing.Size(144, 60);
            // 
            // lblSpecHead
            // 
            this.lblSpecHead.BackColor = System.Drawing.Color.Silver;
            this.lblSpecHead.Location = new System.Drawing.Point(2, 2);
            this.lblSpecHead.Name = "lblSpecHead";
            this.lblSpecHead.Size = new System.Drawing.Size(138, 22);
            this.lblSpecHead.Text = "规格";
            // 
            // lblSpecVal
            // 
            this.lblSpecVal.Location = new System.Drawing.Point(2, 26);
            this.lblSpecVal.Name = "lblSpecVal";
            this.lblSpecVal.Size = new System.Drawing.Size(138, 30);
            // 
            // pnlUnit
            // 
            this.pnlUnit.BackColor = System.Drawing.Color.White;
            this.pnlUnit.Border = new Smobiler.Core.Controls.Border(1F);
            this.pnlUnit.BorderColor = System.Drawing.Color.Gray;
            this.pnlUnit.Controls.AddRange(new Smobiler.Core.Controls.MobileControl[] {
            this.lblUnitHead,
            this.lblUnitVal});
            this.pnlUnit.Location = new System.Drawing.Point(158, 256);
            this.pnlUnit.Name = "pnlUnit";
            this.pnlUnit.Size = new System.Drawing.Size(144, 60);
            // 
            // lblUnitHead
            // 
            this.lblUnitHead.BackColor = System.Drawing.Color.Silver;
            this.lblUnitHead.Location = new System.Drawing.Point(2, 2);
            this.lblUnitHead.Name = "lblUnitHead";
            this.lblUnitHead.Size = new System.Drawing.Size(138, 22);
            this.lblUnitHead.Text = "单位";
            // 
            // lblUnitVal
            // 
            this.lblUnitVal.Location = new System.Drawing.Point(2, 26);
            this.lblUnitVal.Name = "lblUnitVal";
            this.lblUnitVal.Size = new System.Drawing.Size(138, 30);
            // 
            // pnlQty
            // 
            this.pnlQty.BackColor = System.Drawing.Color.White;
            this.pnlQty.Border = new Smobiler.Core.Controls.Border(1F);
            this.pnlQty.BorderColor = System.Drawing.Color.Gray;
            this.pnlQty.Controls.AddRange(new Smobiler.Core.Controls.MobileControl[] {
            this.lblQtyHead,
            this.lblQtyVal});
            this.pnlQty.Location = new System.Drawing.Point(10, 322);
            this.pnlQty.Name = "pnlQty";
            this.pnlQty.Size = new System.Drawing.Size(292, 60);
            // 
            // lblQtyHead
            // 
            this.lblQtyHead.BackColor = System.Drawing.Color.Silver;
            this.lblQtyHead.Location = new System.Drawing.Point(2, 2);
            this.lblQtyHead.Name = "lblQtyHead";
            this.lblQtyHead.Size = new System.Drawing.Size(286, 22);
            this.lblQtyHead.Text = "数量";
            // 
            // lblQtyVal
            // 
            this.lblQtyVal.Location = new System.Drawing.Point(2, 26);
            this.lblQtyVal.Name = "lblQtyVal";
            this.lblQtyVal.Size = new System.Drawing.Size(286, 30);
            // 
            // SmobilerForm1
            // 
            this.Components.AddRange(new Smobiler.Core.Controls.MobileComponent[] {
            this.barcodeReader1});
            this.Controls.AddRange(new Smobiler.Core.Controls.MobileControl[] {
            this.txtCode,
            this.btnQuery,
            this.btnScan,
            this.lblResult,
            this.pnlCode,
            this.pnlName,
            this.pnlSpec,
            this.pnlUnit,
            this.pnlQty,
            this.button1,
            this.btnWorkOrder});
            this.Name = "SmobilerForm1";

        }
        #endregion

        private Smobiler.Core.Controls.Button button1;
        private Smobiler.Core.Controls.Button btnWorkOrder;
        private Smobiler.Core.Controls.TextBox txtCode;
        private Smobiler.Core.Controls.Button btnQuery;
        private Smobiler.Core.Controls.Button btnScan;
        private Smobiler.Core.Controls.Label lblResult;
        private Smobiler.Core.Controls.Panel pnlCode;
        private Smobiler.Core.Controls.Label lblCodeHead;
        private Smobiler.Core.Controls.Label lblCodeVal;
        private Smobiler.Core.Controls.Panel pnlName;
        private Smobiler.Core.Controls.Label lblNameHead;
        private Smobiler.Core.Controls.Label lblNameVal;
        private Smobiler.Core.Controls.Panel pnlSpec;
        private Smobiler.Core.Controls.Label lblSpecHead;
        private Smobiler.Core.Controls.Label lblSpecVal;
        private Smobiler.Core.Controls.Panel pnlUnit;
        private Smobiler.Core.Controls.Label lblUnitHead;
        private Smobiler.Core.Controls.Label lblUnitVal;
        private Smobiler.Core.Controls.Panel pnlQty;
        private Smobiler.Core.Controls.Label lblQtyHead;
        private Smobiler.Core.Controls.Label lblQtyVal;
        private Smobiler.Plugins.BarcodeReader barcodeReader1;
    }
}
