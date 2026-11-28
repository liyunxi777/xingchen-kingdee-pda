using Smobiler.Core;

namespace ElectricityApp
{
    partial class ScanForm : Smobiler.Core.Controls.MobileForm
    {
        #region "ScanForm Designer generated code "

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.lblTitle = new Smobiler.Core.Controls.Label();
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
            // lblTitle
            // 
            this.lblTitle.BackColor = System.Drawing.Color.RoyalBlue;
            this.lblTitle.FontSize = 16F;
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(320, 44);
            this.lblTitle.Text = "扫码查料";
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
            this.btnScan.BackColor = System.Drawing.Color.RoyalBlue;
            this.btnScan.Location = new System.Drawing.Point(10, 100);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(295, 38);
            this.btnScan.Text = "扫码查询";
            this.btnScan.Press += new System.EventHandler(this.btnScan_Press);
            // 
            // lblResult
            // 
            this.lblResult.FontSize = 12F;
            this.lblResult.ForeColor = System.Drawing.Color.Gray;
            this.lblResult.Location = new System.Drawing.Point(10, 146);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(295, 30);
            this.lblResult.Text = "请输入或扫描物料编码";
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
            this.pnlCode.Location = new System.Drawing.Point(10, 186);
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
            this.pnlName.Location = new System.Drawing.Point(161, 186);
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
            this.pnlSpec.Location = new System.Drawing.Point(10, 252);
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
            this.pnlUnit.Location = new System.Drawing.Point(161, 252);
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
            this.pnlQty.Location = new System.Drawing.Point(10, 318);
            this.pnlQty.Name = "pnlQty";
            this.pnlQty.Size = new System.Drawing.Size(295, 60);
            // 
            // lblQtyHead
            // 
            this.lblQtyHead.BackColor = System.Drawing.Color.Silver;
            this.lblQtyHead.Location = new System.Drawing.Point(2, 2);
            this.lblQtyHead.Name = "lblQtyHead";
            this.lblQtyHead.Size = new System.Drawing.Size(289, 22);
            this.lblQtyHead.Text = "数量";
            // 
            // lblQtyVal
            // 
            this.lblQtyVal.Location = new System.Drawing.Point(2, 26);
            this.lblQtyVal.Name = "lblQtyVal";
            this.lblQtyVal.Size = new System.Drawing.Size(289, 30);
            // 
            // ScanForm
            // 
            this.Components.AddRange(new Smobiler.Core.Controls.MobileComponent[] {
            this.barcodeReader1});
            this.Controls.AddRange(new Smobiler.Core.Controls.MobileControl[] {
            this.lblTitle,
            this.txtCode,
            this.btnQuery,
            this.btnScan,
            this.lblResult,
            this.pnlCode,
            this.pnlName,
            this.pnlSpec,
            this.pnlUnit,
            this.pnlQty});
            this.Name = "ScanForm";

        }
        #endregion

        private Smobiler.Core.Controls.Label lblTitle;
        private Smobiler.Core.Controls.TextBox txtCode;
        private Smobiler.Core.Controls.Button btnQuery;
        private Smobiler.Core.Controls.Button btnScan;
        private Smobiler.Core.Controls.Label lblResult;
        private Smobiler.Plugins.BarcodeReader barcodeReader1;
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
    }
}
