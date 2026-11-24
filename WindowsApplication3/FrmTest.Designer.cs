namespace WindowsApplication3
{
    partial class FrmTest
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnGetMtrl = new System.Windows.Forms.Button();
            this.btnGetWh = new System.Windows.Forms.Button();
            this.btnGetDept = new System.Windows.Forms.Button();
            this.btnGetEmp = new System.Windows.Forms.Button();
            this.btnSaveInStk = new System.Windows.Forms.Button();
            this.btnGetUnits = new System.Windows.Forms.Button();
            this.btnGetLoc = new System.Windows.Forms.Button();
            this.btnGetMtrlDetail = new System.Windows.Forms.Button();
            this.btnIcMoBill = new System.Windows.Forms.Button();
            this.btnGetMoLst = new System.Windows.Forms.Button();
            this.btnGetInStkLst = new System.Windows.Forms.Button();
            this.btnGetInStkDetail = new System.Windows.Forms.Button();
            this.btnGetCust = new System.Windows.Forms.Button();
            this.btnGetMtrlSort = new System.Windows.Forms.Button();
            this.btnGetMtrlUnit = new System.Windows.Forms.Button();
            this.btnGetAux = new System.Windows.Forms.Button();
            this.btnGetPurOrderLst = new System.Windows.Forms.Button();
            this.btnGetPurBill = new System.Windows.Forms.Button();
            this.TextBox1 = new System.Windows.Forms.TextBox();
            this.btnGetSalOrder = new System.Windows.Forms.Button();
            this.btnQueryLock = new System.Windows.Forms.Button();
            this.btnClearLock = new System.Windows.Forms.Button();
            this.btnSalOrderLst = new System.Windows.Forms.Button();
            this.btnSalOutStk = new System.Windows.Forms.Button();
            this.btnSalOutStkLst = new System.Windows.Forms.Button();
            this.btnSalTotalRpt = new System.Windows.Forms.Button();
            this.btnQueryStk = new System.Windows.Forms.Button();
            this.btnSaveToDb = new System.Windows.Forms.Button();
            this.btnTestDb = new System.Windows.Forms.Button();
            this.btnTestInsert = new System.Windows.Forms.Button();
            this.chkAutoSave = new System.Windows.Forms.CheckBox();
            this.lblDbStatus = new System.Windows.Forms.Label();
            this.dgvMaterials = new System.Windows.Forms.DataGridView();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.tabTables = new System.Windows.Forms.TabControl();
            this.tabPageMaterial = new System.Windows.Forms.TabPage();
            this.dgvAutoTabMaterial = new System.Windows.Forms.DataGridView();
            this.tabPageWorkOrder = new System.Windows.Forms.TabPage();
            this.dgvAutoTabWorkOrder = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMaterials)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.tabTables.SuspendLayout();
            this.tabPageMaterial.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAutoTabMaterial)).BeginInit();
            this.tabPageWorkOrder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAutoTabWorkOrder)).BeginInit();
            this.SuspendLayout();
            //
            // 布局说明：
            //   顶部 y=12/62/112 三行操作按钮；中部 y=162~556 为 SplitContainer 左右分栏
            //   （左=TabControl 物料/工单自动刷新，右=按钮结果表 dgvMaterials）；
            //   y=566 数据库操作区；y=616 JSON 输出框。
            //
            // btnGetMtrl
            //
            this.btnGetMtrl.Location = new System.Drawing.Point(12, 12);
            this.btnGetMtrl.Name = "btnGetMtrl";
            this.btnGetMtrl.Size = new System.Drawing.Size(104, 40);
            this.btnGetMtrl.TabIndex = 0;
            this.btnGetMtrl.Text = "获取物料";
            this.btnGetMtrl.UseVisualStyleBackColor = true;
            this.btnGetMtrl.Click += new System.EventHandler(this.btnGetMtrl_Click);
            //
            // btnGetWh
            //
            this.btnGetWh.Location = new System.Drawing.Point(122, 12);
            this.btnGetWh.Name = "btnGetWh";
            this.btnGetWh.Size = new System.Drawing.Size(104, 40);
            this.btnGetWh.TabIndex = 1;
            this.btnGetWh.Text = "获取仓库";
            this.btnGetWh.UseVisualStyleBackColor = true;
            this.btnGetWh.Click += new System.EventHandler(this.btnGetWh_Click);
            //
            // btnGetDept
            //
            this.btnGetDept.Location = new System.Drawing.Point(232, 12);
            this.btnGetDept.Name = "btnGetDept";
            this.btnGetDept.Size = new System.Drawing.Size(104, 40);
            this.btnGetDept.TabIndex = 2;
            this.btnGetDept.Text = "获取部门";
            this.btnGetDept.UseVisualStyleBackColor = true;
            this.btnGetDept.Click += new System.EventHandler(this.btnGetDept_Click);
            //
            // btnGetEmp
            //
            this.btnGetEmp.Location = new System.Drawing.Point(342, 12);
            this.btnGetEmp.Name = "btnGetEmp";
            this.btnGetEmp.Size = new System.Drawing.Size(104, 40);
            this.btnGetEmp.TabIndex = 3;
            this.btnGetEmp.Text = "获取人员";
            this.btnGetEmp.UseVisualStyleBackColor = true;
            this.btnGetEmp.Click += new System.EventHandler(this.btnGetEmp_Click);
            //
            // btnGetUnits
            //
            this.btnGetUnits.Location = new System.Drawing.Point(452, 12);
            this.btnGetUnits.Name = "btnGetUnits";
            this.btnGetUnits.Size = new System.Drawing.Size(104, 40);
            this.btnGetUnits.TabIndex = 5;
            this.btnGetUnits.Text = "获取单位";
            this.btnGetUnits.UseVisualStyleBackColor = true;
            this.btnGetUnits.Click += new System.EventHandler(this.btnGetUnits_Click);
            //
            // btnGetMtrlSort
            //
            this.btnGetMtrlSort.Location = new System.Drawing.Point(562, 12);
            this.btnGetMtrlSort.Name = "btnGetMtrlSort";
            this.btnGetMtrlSort.Size = new System.Drawing.Size(104, 40);
            this.btnGetMtrlSort.TabIndex = 13;
            this.btnGetMtrlSort.Text = "获取物料分类";
            this.btnGetMtrlSort.UseVisualStyleBackColor = true;
            this.btnGetMtrlSort.Click += new System.EventHandler(this.btnGetMtrlSort_Click);
            //
            // btnGetMtrlUnit
            //
            this.btnGetMtrlUnit.Location = new System.Drawing.Point(672, 12);
            this.btnGetMtrlUnit.Name = "btnGetMtrlUnit";
            this.btnGetMtrlUnit.Size = new System.Drawing.Size(104, 40);
            this.btnGetMtrlUnit.TabIndex = 14;
            this.btnGetMtrlUnit.Text = "获取物料单位";
            this.btnGetMtrlUnit.UseVisualStyleBackColor = true;
            this.btnGetMtrlUnit.Click += new System.EventHandler(this.btnGetMtrlUnit_Click);
            //
            // btnGetAux
            //
            this.btnGetAux.Location = new System.Drawing.Point(782, 12);
            this.btnGetAux.Name = "btnGetAux";
            this.btnGetAux.Size = new System.Drawing.Size(104, 40);
            this.btnGetAux.TabIndex = 15;
            this.btnGetAux.Text = "获取辅助属性";
            this.btnGetAux.UseVisualStyleBackColor = true;
            this.btnGetAux.Click += new System.EventHandler(this.btnGetAux_Click);
            //
            // btnGetCust
            //
            this.btnGetCust.Location = new System.Drawing.Point(892, 12);
            this.btnGetCust.Name = "btnGetCust";
            this.btnGetCust.Size = new System.Drawing.Size(104, 40);
            this.btnGetCust.TabIndex = 12;
            this.btnGetCust.Text = "获取客户";
            this.btnGetCust.UseVisualStyleBackColor = true;
            this.btnGetCust.Click += new System.EventHandler(this.btnGetCust_Click);
            //
            // btnGetLoc
            //
            this.btnGetLoc.Location = new System.Drawing.Point(12, 62);
            this.btnGetLoc.Name = "btnGetLoc";
            this.btnGetLoc.Size = new System.Drawing.Size(104, 40);
            this.btnGetLoc.TabIndex = 6;
            this.btnGetLoc.Text = "获取库位";
            this.btnGetLoc.UseVisualStyleBackColor = true;
            this.btnGetLoc.Click += new System.EventHandler(this.btnGetLoc_Click);
            //
            // btnGetMtrlDetail
            //
            this.btnGetMtrlDetail.Location = new System.Drawing.Point(122, 62);
            this.btnGetMtrlDetail.Name = "btnGetMtrlDetail";
            this.btnGetMtrlDetail.Size = new System.Drawing.Size(104, 40);
            this.btnGetMtrlDetail.TabIndex = 7;
            this.btnGetMtrlDetail.Text = "获取物料详情";
            this.btnGetMtrlDetail.UseVisualStyleBackColor = true;
            this.btnGetMtrlDetail.Click += new System.EventHandler(this.btnGetMtrlDetail_Click);
            //
            // btnIcMoBill
            //
            this.btnIcMoBill.Location = new System.Drawing.Point(232, 62);
            this.btnIcMoBill.Name = "btnIcMoBill";
            this.btnIcMoBill.Size = new System.Drawing.Size(104, 40);
            this.btnIcMoBill.TabIndex = 8;
            this.btnIcMoBill.Text = "获取生产工单";
            this.btnIcMoBill.UseVisualStyleBackColor = true;
            this.btnIcMoBill.Click += new System.EventHandler(this.btnIcMoBill_Click);
            //
            // btnGetMoLst
            //
            this.btnGetMoLst.Location = new System.Drawing.Point(342, 62);
            this.btnGetMoLst.Name = "btnGetMoLst";
            this.btnGetMoLst.Size = new System.Drawing.Size(104, 40);
            this.btnGetMoLst.TabIndex = 9;
            this.btnGetMoLst.Text = "获取工单详情";
            this.btnGetMoLst.UseVisualStyleBackColor = true;
            this.btnGetMoLst.Click += new System.EventHandler(this.btnGetMoLst_Click);
            //
            // btnGetPurBill
            //
            this.btnGetPurBill.Location = new System.Drawing.Point(452, 62);
            this.btnGetPurBill.Name = "btnGetPurBill";
            this.btnGetPurBill.Size = new System.Drawing.Size(104, 40);
            this.btnGetPurBill.TabIndex = 16;
            this.btnGetPurBill.Text = "获取采购订单";
            this.btnGetPurBill.UseVisualStyleBackColor = true;
            this.btnGetPurBill.Click += new System.EventHandler(this.btnGetPurBill_Click);
            //
            // btnGetPurOrderLst
            //
            this.btnGetPurOrderLst.Location = new System.Drawing.Point(562, 62);
            this.btnGetPurOrderLst.Name = "btnGetPurOrderLst";
            this.btnGetPurOrderLst.Size = new System.Drawing.Size(104, 40);
            this.btnGetPurOrderLst.TabIndex = 17;
            this.btnGetPurOrderLst.Text = "采购订单详情";
            this.btnGetPurOrderLst.UseVisualStyleBackColor = true;
            this.btnGetPurOrderLst.Click += new System.EventHandler(this.btnGetPurOrderLst_Click);
            //
            // btnGetInStkLst
            //
            this.btnGetInStkLst.Location = new System.Drawing.Point(672, 62);
            this.btnGetInStkLst.Name = "btnGetInStkLst";
            this.btnGetInStkLst.Size = new System.Drawing.Size(104, 40);
            this.btnGetInStkLst.TabIndex = 10;
            this.btnGetInStkLst.Text = "获取入库列表";
            this.btnGetInStkLst.UseVisualStyleBackColor = true;
            this.btnGetInStkLst.Click += new System.EventHandler(this.btnGetInStkLst_Click);
            //
            // btnGetInStkDetail
            //
            this.btnGetInStkDetail.Location = new System.Drawing.Point(782, 62);
            this.btnGetInStkDetail.Name = "btnGetInStkDetail";
            this.btnGetInStkDetail.Size = new System.Drawing.Size(104, 40);
            this.btnGetInStkDetail.TabIndex = 11;
            this.btnGetInStkDetail.Text = "获取入库明细";
            this.btnGetInStkDetail.UseVisualStyleBackColor = true;
            this.btnGetInStkDetail.Click += new System.EventHandler(this.btnGetInStkDetail_Click);
            //
            // btnSaveInStk
            //
            this.btnSaveInStk.Location = new System.Drawing.Point(892, 62);
            this.btnSaveInStk.Name = "btnSaveInStk";
            this.btnSaveInStk.Size = new System.Drawing.Size(104, 40);
            this.btnSaveInStk.TabIndex = 4;
            this.btnSaveInStk.Text = "保存入库";
            this.btnSaveInStk.UseVisualStyleBackColor = true;
            this.btnSaveInStk.Click += new System.EventHandler(this.btnSaveInStk_Click);
            //
            // btnGetSalOrder
            //
            this.btnGetSalOrder.Location = new System.Drawing.Point(12, 112);
            this.btnGetSalOrder.Name = "btnGetSalOrder";
            this.btnGetSalOrder.Size = new System.Drawing.Size(104, 40);
            this.btnGetSalOrder.TabIndex = 19;
            this.btnGetSalOrder.Text = "获取销售订单";
            this.btnGetSalOrder.UseVisualStyleBackColor = true;
            this.btnGetSalOrder.Click += new System.EventHandler(this.btnGetSalOrder_Click);
            //
            // btnSalOrderLst
            //
            this.btnSalOrderLst.Location = new System.Drawing.Point(122, 112);
            this.btnSalOrderLst.Name = "btnSalOrderLst";
            this.btnSalOrderLst.Size = new System.Drawing.Size(104, 40);
            this.btnSalOrderLst.TabIndex = 22;
            this.btnSalOrderLst.Text = "销售订单明细";
            this.btnSalOrderLst.UseVisualStyleBackColor = true;
            this.btnSalOrderLst.Click += new System.EventHandler(this.btnSalOrderLst_Click);
            //
            // btnSalOutStk
            //
            this.btnSalOutStk.Location = new System.Drawing.Point(232, 112);
            this.btnSalOutStk.Name = "btnSalOutStk";
            this.btnSalOutStk.Size = new System.Drawing.Size(104, 40);
            this.btnSalOutStk.TabIndex = 24;
            this.btnSalOutStk.Text = "销售出库单列表";
            this.btnSalOutStk.UseVisualStyleBackColor = true;
            this.btnSalOutStk.Click += new System.EventHandler(this.btnSalOutStk_Click);
            //
            // btnSalOutStkLst
            //
            this.btnSalOutStkLst.Location = new System.Drawing.Point(342, 112);
            this.btnSalOutStkLst.Name = "btnSalOutStkLst";
            this.btnSalOutStkLst.Size = new System.Drawing.Size(104, 40);
            this.btnSalOutStkLst.TabIndex = 23;
            this.btnSalOutStkLst.Text = "销售出库明细";
            this.btnSalOutStkLst.UseVisualStyleBackColor = true;
            this.btnSalOutStkLst.Click += new System.EventHandler(this.btnSalOutStkLst_Click);
            //
            // btnSalTotalRpt
            //
            this.btnSalTotalRpt.Location = new System.Drawing.Point(452, 112);
            this.btnSalTotalRpt.Name = "btnSalTotalRpt";
            this.btnSalTotalRpt.Size = new System.Drawing.Size(104, 40);
            this.btnSalTotalRpt.TabIndex = 25;
            this.btnSalTotalRpt.Text = "销售汇总表";
            this.btnSalTotalRpt.UseVisualStyleBackColor = true;
            this.btnSalTotalRpt.Click += new System.EventHandler(this.btnSalTotalRpt_Click);
            //
            // btnQueryStk
            //
            this.btnQueryStk.Location = new System.Drawing.Point(562, 112);
            this.btnQueryStk.Name = "btnQueryStk";
            this.btnQueryStk.Size = new System.Drawing.Size(104, 40);
            this.btnQueryStk.TabIndex = 26;
            this.btnQueryStk.Text = "查询库存";
            this.btnQueryStk.UseVisualStyleBackColor = true;
            this.btnQueryStk.Click += new System.EventHandler(this.btnQueryStk_Click);
            //
            // btnQueryLock
            //
            this.btnQueryLock.Location = new System.Drawing.Point(672, 112);
            this.btnQueryLock.Name = "btnQueryLock";
            this.btnQueryLock.Size = new System.Drawing.Size(104, 40);
            this.btnQueryLock.TabIndex = 20;
            this.btnQueryLock.Text = "网络互斥查询";
            this.btnQueryLock.UseVisualStyleBackColor = true;
            this.btnQueryLock.Click += new System.EventHandler(this.btnQueryLock_Click);
            //
            // btnClearLock
            //
            this.btnClearLock.Location = new System.Drawing.Point(782, 112);
            this.btnClearLock.Name = "btnClearLock";
            this.btnClearLock.Size = new System.Drawing.Size(104, 40);
            this.btnClearLock.TabIndex = 21;
            this.btnClearLock.Text = "清除网络互斥";
            this.btnClearLock.UseVisualStyleBackColor = true;
            this.btnClearLock.Click += new System.EventHandler(this.btnClearLock_Click);
            //
            // btnSaveToDb
            //
            this.btnSaveToDb.BackColor = System.Drawing.Color.LightGreen;
            this.btnSaveToDb.Location = new System.Drawing.Point(12, 566);
            this.btnSaveToDb.Name = "btnSaveToDb";
            this.btnSaveToDb.Size = new System.Drawing.Size(104, 40);
            this.btnSaveToDb.TabIndex = 27;
            this.btnSaveToDb.Text = "保存到数据库";
            this.btnSaveToDb.UseVisualStyleBackColor = false;
            this.btnSaveToDb.Click += new System.EventHandler(this.btnSaveToDb_Click);
            //
            // chkAutoSave
            //
            this.chkAutoSave.Checked = false;
            this.chkAutoSave.CheckState = System.Windows.Forms.CheckState.Unchecked;
            this.chkAutoSave.Location = new System.Drawing.Point(122, 566);
            this.chkAutoSave.Name = "chkAutoSave";
            this.chkAutoSave.Size = new System.Drawing.Size(104, 40);
            this.chkAutoSave.TabIndex = 30;
            this.chkAutoSave.Text = "自动入库";
            this.chkAutoSave.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkAutoSave.UseVisualStyleBackColor = true;
            //
            // btnTestDb
            //
            this.btnTestDb.BackColor = System.Drawing.Color.LightSkyBlue;
            this.btnTestDb.Location = new System.Drawing.Point(232, 566);
            this.btnTestDb.Name = "btnTestDb";
            this.btnTestDb.Size = new System.Drawing.Size(104, 40);
            this.btnTestDb.TabIndex = 29;
            this.btnTestDb.Text = "测试数据库";
            this.btnTestDb.UseVisualStyleBackColor = false;
            this.btnTestDb.Click += new System.EventHandler(this.btnTestDb_Click);
            //
            // btnTestInsert
            //
            this.btnTestInsert.BackColor = System.Drawing.Color.LightYellow;
            this.btnTestInsert.Location = new System.Drawing.Point(342, 566);
            this.btnTestInsert.Name = "btnTestInsert";
            this.btnTestInsert.Size = new System.Drawing.Size(104, 40);
            this.btnTestInsert.TabIndex = 32;
            this.btnTestInsert.Text = "测试入库";
            this.btnTestInsert.UseVisualStyleBackColor = false;
            this.btnTestInsert.Click += new System.EventHandler(this.btnTestInsert_Click);
            //
            // lblDbStatus
            //
            this.lblDbStatus.Location = new System.Drawing.Point(462, 566);
            this.lblDbStatus.Name = "lblDbStatus";
            this.lblDbStatus.Size = new System.Drawing.Size(240, 40);
            this.lblDbStatus.TabIndex = 31;
            this.lblDbStatus.Text = "数据库：初始化中";
            //
            // TextBox1
            //
            this.TextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextBox1.Location = new System.Drawing.Point(12, 616);
            this.TextBox1.Multiline = true;
            this.TextBox1.Name = "TextBox1";
            this.TextBox1.ReadOnly = true;
            this.TextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.TextBox1.Size = new System.Drawing.Size(1083, 94);
            this.TextBox1.TabIndex = 18;
            //
            // splitMain（中部左右分栏）
            //
            this.splitMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.splitMain.Location = new System.Drawing.Point(12, 162);
            this.splitMain.Name = "splitMain";
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Vertical;
            //
            // splitMain.Panel1 —— 左侧：TabControl（物料/工单自动刷新）
            //
            this.splitMain.Panel1.Controls.Add(this.tabTables);
            //
            // splitMain.Panel2 —— 右侧：按钮结果表
            //
            this.splitMain.Panel2.Controls.Add(this.dgvMaterials);
            this.splitMain.Size = new System.Drawing.Size(1083, 394);
            this.splitMain.SplitterDistance = 520;
            this.splitMain.SplitterWidth = 6;
            this.splitMain.TabIndex = 33;
            //
            // tabTables（标签在底部，Excel 风格）
            //
            this.tabTables.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabTables.Controls.Add(this.tabPageMaterial);
            this.tabTables.Controls.Add(this.tabPageWorkOrder);
            this.tabTables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabTables.Location = new System.Drawing.Point(0, 0);
            this.tabTables.Name = "tabTables";
            this.tabTables.SelectedIndex = 0;
            this.tabTables.Size = new System.Drawing.Size(520, 394);
            this.tabTables.TabIndex = 0;
            //
            // tabPageMaterial
            //
            this.tabPageMaterial.Controls.Add(this.dgvAutoTabMaterial);
            this.tabPageMaterial.Location = new System.Drawing.Point(0, 0);
            this.tabPageMaterial.Name = "tabPageMaterial";
            this.tabPageMaterial.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMaterial.Size = new System.Drawing.Size(520, 366);
            this.tabPageMaterial.TabIndex = 0;
            this.tabPageMaterial.Text = "物料";
            this.tabPageMaterial.UseVisualStyleBackColor = true;
            //
            // dgvAutoTabMaterial
            //
            this.dgvAutoTabMaterial.AllowUserToAddRows = false;
            this.dgvAutoTabMaterial.AllowUserToDeleteRows = false;
            this.dgvAutoTabMaterial.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAutoTabMaterial.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAutoTabMaterial.Location = new System.Drawing.Point(3, 3);
            this.dgvAutoTabMaterial.Name = "dgvAutoTabMaterial";
            this.dgvAutoTabMaterial.ReadOnly = true;
            this.dgvAutoTabMaterial.RowHeadersWidth = 40;
            this.dgvAutoTabMaterial.Size = new System.Drawing.Size(514, 360);
            this.dgvAutoTabMaterial.TabIndex = 0;
            //
            // tabPageWorkOrder
            //
            this.tabPageWorkOrder.Controls.Add(this.dgvAutoTabWorkOrder);
            this.tabPageWorkOrder.Location = new System.Drawing.Point(0, 0);
            this.tabPageWorkOrder.Name = "tabPageWorkOrder";
            this.tabPageWorkOrder.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageWorkOrder.Size = new System.Drawing.Size(520, 366);
            this.tabPageWorkOrder.TabIndex = 1;
            this.tabPageWorkOrder.Text = "工单";
            this.tabPageWorkOrder.UseVisualStyleBackColor = true;
            //
            // dgvAutoTabWorkOrder
            //
            this.dgvAutoTabWorkOrder.AllowUserToAddRows = false;
            this.dgvAutoTabWorkOrder.AllowUserToDeleteRows = false;
            this.dgvAutoTabWorkOrder.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAutoTabWorkOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAutoTabWorkOrder.Location = new System.Drawing.Point(3, 3);
            this.dgvAutoTabWorkOrder.Name = "dgvAutoTabWorkOrder";
            this.dgvAutoTabWorkOrder.ReadOnly = true;
            this.dgvAutoTabWorkOrder.RowHeadersWidth = 40;
            this.dgvAutoTabWorkOrder.Size = new System.Drawing.Size(514, 360);
            this.dgvAutoTabWorkOrder.TabIndex = 0;
            //
            // dgvMaterials（右侧按钮结果表）
            //
            this.dgvMaterials.AllowUserToAddRows = false;
            this.dgvMaterials.AllowUserToDeleteRows = false;
            this.dgvMaterials.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMaterials.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMaterials.Location = new System.Drawing.Point(0, 0);
            this.dgvMaterials.Name = "dgvMaterials";
            this.dgvMaterials.ReadOnly = true;
            this.dgvMaterials.RowHeadersWidth = 40;
            this.dgvMaterials.Size = new System.Drawing.Size(557, 394);
            this.dgvMaterials.TabIndex = 28;
            //
            // FrmTest
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1107, 720);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.lblDbStatus);
            this.Controls.Add(this.chkAutoSave);
            this.Controls.Add(this.btnTestDb);
            this.Controls.Add(this.btnTestInsert);
            this.Controls.Add(this.btnSaveToDb);
            this.Controls.Add(this.btnQueryStk);
            this.Controls.Add(this.btnSalTotalRpt);
            this.Controls.Add(this.btnSalOutStk);
            this.Controls.Add(this.btnSalOutStkLst);
            this.Controls.Add(this.btnSalOrderLst);
            this.Controls.Add(this.btnClearLock);
            this.Controls.Add(this.btnQueryLock);
            this.Controls.Add(this.btnGetSalOrder);
            this.Controls.Add(this.TextBox1);
            this.Controls.Add(this.btnGetPurOrderLst);
            this.Controls.Add(this.btnGetPurBill);
            this.Controls.Add(this.btnGetAux);
            this.Controls.Add(this.btnGetMtrlUnit);
            this.Controls.Add(this.btnGetMtrlSort);
            this.Controls.Add(this.btnGetCust);
            this.Controls.Add(this.btnGetInStkDetail);
            this.Controls.Add(this.btnGetInStkLst);
            this.Controls.Add(this.btnGetMoLst);
            this.Controls.Add(this.btnIcMoBill);
            this.Controls.Add(this.btnGetMtrlDetail);
            this.Controls.Add(this.btnGetLoc);
            this.Controls.Add(this.btnGetUnits);
            this.Controls.Add(this.btnSaveInStk);
            this.Controls.Add(this.btnGetEmp);
            this.Controls.Add(this.btnGetDept);
            this.Controls.Add(this.btnGetWh);
            this.Controls.Add(this.btnGetMtrl);
            this.Name = "FrmTest";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "星辰API调用 - 物料管理";
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.tabPageMaterial.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAutoTabMaterial)).EndInit();
            this.tabPageWorkOrder.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAutoTabWorkOrder)).EndInit();
            this.tabTables.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMaterials)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button btnGetMtrl;
        private System.Windows.Forms.Button btnGetWh;
        private System.Windows.Forms.Button btnGetDept;
        private System.Windows.Forms.Button btnGetEmp;
        private System.Windows.Forms.Button btnSaveInStk;
        private System.Windows.Forms.Button btnGetUnits;
        private System.Windows.Forms.Button btnGetLoc;
        private System.Windows.Forms.Button btnGetMtrlDetail;
        private System.Windows.Forms.Button btnIcMoBill;
        private System.Windows.Forms.Button btnGetMoLst;
        private System.Windows.Forms.Button btnGetInStkLst;
        private System.Windows.Forms.Button btnGetInStkDetail;
        private System.Windows.Forms.Button btnGetCust;
        private System.Windows.Forms.Button btnGetMtrlSort;
        private System.Windows.Forms.Button btnGetMtrlUnit;
        private System.Windows.Forms.Button btnGetAux;
        private System.Windows.Forms.Button btnGetPurOrderLst;
        private System.Windows.Forms.Button btnGetPurBill;
        private System.Windows.Forms.TextBox TextBox1;
        private System.Windows.Forms.Button btnGetSalOrder;
        private System.Windows.Forms.Button btnQueryLock;
        private System.Windows.Forms.Button btnClearLock;
        private System.Windows.Forms.Button btnSalOrderLst;
        private System.Windows.Forms.Button btnSalOutStk;
        private System.Windows.Forms.Button btnSalOutStkLst;
        private System.Windows.Forms.Button btnSalTotalRpt;
        private System.Windows.Forms.Button btnQueryStk;
        private System.Windows.Forms.Button btnSaveToDb;
        private System.Windows.Forms.Button btnTestDb;
        private System.Windows.Forms.Button btnTestInsert;
        private System.Windows.Forms.CheckBox chkAutoSave;
        private System.Windows.Forms.Label lblDbStatus;
        private System.Windows.Forms.DataGridView dgvMaterials;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.TabControl tabTables;
        private System.Windows.Forms.TabPage tabPageMaterial;
        private System.Windows.Forms.DataGridView dgvAutoTabMaterial;
        private System.Windows.Forms.TabPage tabPageWorkOrder;
        private System.Windows.Forms.DataGridView dgvAutoTabWorkOrder;
    }
}
