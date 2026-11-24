<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmTest
	Inherits System.Windows.Forms.Form

	'Form 重写 Dispose，以清理组件列表。
	<System.Diagnostics.DebuggerNonUserCode()> _
	Protected Overrides Sub Dispose(ByVal disposing As Boolean)
		Try
			If disposing AndAlso components IsNot Nothing Then
				components.Dispose()
			End If
		Finally
			MyBase.Dispose(disposing)
		End Try
	End Sub

	'Windows 窗体设计器所必需的
	Private components As System.ComponentModel.IContainer

	'注意:  以下过程是 Windows 窗体设计器所必需的
	'可以使用 Windows 窗体设计器修改它。
	'不要使用代码编辑器修改它。
	<System.Diagnostics.DebuggerStepThrough()> _
	Private Sub InitializeComponent()
		Me.btnGetMtrl = New System.Windows.Forms.Button()
		Me.btnGetWh = New System.Windows.Forms.Button()
		Me.btnGetDept = New System.Windows.Forms.Button()
		Me.btnGetEmp = New System.Windows.Forms.Button()
		Me.btnSaveInStk = New System.Windows.Forms.Button()
		Me.btnGetUnits = New System.Windows.Forms.Button()
		Me.btnGetLoc = New System.Windows.Forms.Button()
		Me.btnGetMtrlDetail = New System.Windows.Forms.Button()
		Me.btnIcMoBill = New System.Windows.Forms.Button()
		Me.btnGetMoLst = New System.Windows.Forms.Button()
		Me.btnGetInStkLst = New System.Windows.Forms.Button()
		Me.btnGetInStkDetail = New System.Windows.Forms.Button()
		Me.btnGetCust = New System.Windows.Forms.Button()
		Me.btnGetMtrlSort = New System.Windows.Forms.Button()
		Me.btnGetMtrlUnit = New System.Windows.Forms.Button()
		Me.btnGetAux = New System.Windows.Forms.Button()
		Me.btnGetPurOrderLst = New System.Windows.Forms.Button()
		Me.btnGetPurBill = New System.Windows.Forms.Button()
		Me.TextBox1 = New System.Windows.Forms.TextBox()
		Me.btnGetSalOrder = New System.Windows.Forms.Button()
		Me.btnQueryLock = New System.Windows.Forms.Button()
		Me.btnClearLock = New System.Windows.Forms.Button()
		Me.btnSalOrderLst = New System.Windows.Forms.Button()
		Me.btnSalOutStk = New System.Windows.Forms.Button()
		Me.btnSalOutStkLst = New System.Windows.Forms.Button()
		Me.btnSalTotalRpt = New System.Windows.Forms.Button()
		Me.btnQueryStk = New System.Windows.Forms.Button()
		Me.btnSaveToDb = New System.Windows.Forms.Button()
		Me.dgvMaterials = New System.Windows.Forms.DataGridView()
		CType(Me.dgvMaterials, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'btnGetMtrl
		'
		Me.btnGetMtrl.Location = New System.Drawing.Point(12, 12)
		Me.btnGetMtrl.Name = "btnGetMtrl"
		Me.btnGetMtrl.Size = New System.Drawing.Size(104, 40)
		Me.btnGetMtrl.TabIndex = 0
		Me.btnGetMtrl.Text = "获取物料"
		Me.btnGetMtrl.UseVisualStyleBackColor = True
		'
		'btnGetWh
		'
		Me.btnGetWh.Location = New System.Drawing.Point(122, 12)
		Me.btnGetWh.Name = "btnGetWh"
		Me.btnGetWh.Size = New System.Drawing.Size(104, 40)
		Me.btnGetWh.TabIndex = 1
		Me.btnGetWh.Text = "获取仓库"
		Me.btnGetWh.UseVisualStyleBackColor = True
		'
		'btnGetDept
		'
		Me.btnGetDept.Location = New System.Drawing.Point(232, 12)
		Me.btnGetDept.Name = "btnGetDept"
		Me.btnGetDept.Size = New System.Drawing.Size(104, 40)
		Me.btnGetDept.TabIndex = 2
		Me.btnGetDept.Text = "获取部门"
		Me.btnGetDept.UseVisualStyleBackColor = True
		'
		'btnGetEmp
		'
		Me.btnGetEmp.Location = New System.Drawing.Point(342, 12)
		Me.btnGetEmp.Name = "btnGetEmp"
		Me.btnGetEmp.Size = New System.Drawing.Size(104, 40)
		Me.btnGetEmp.TabIndex = 3
		Me.btnGetEmp.Text = "获取人员"
		Me.btnGetEmp.UseVisualStyleBackColor = True
		'
		'btnSaveInStk
		'
		Me.btnSaveInStk.Location = New System.Drawing.Point(672, 68)
		Me.btnSaveInStk.Name = "btnSaveInStk"
		Me.btnSaveInStk.Size = New System.Drawing.Size(104, 40)
		Me.btnSaveInStk.TabIndex = 4
		Me.btnSaveInStk.Text = "保存入库"
		Me.btnSaveInStk.UseVisualStyleBackColor = True
		'
		'btnGetUnits
		'
		Me.btnGetUnits.Location = New System.Drawing.Point(452, 12)
		Me.btnGetUnits.Name = "btnGetUnits"
		Me.btnGetUnits.Size = New System.Drawing.Size(104, 40)
		Me.btnGetUnits.TabIndex = 5
		Me.btnGetUnits.Text = "获取单位"
		Me.btnGetUnits.UseVisualStyleBackColor = True
		'
		'btnGetLoc
		'
		Me.btnGetLoc.Location = New System.Drawing.Point(12, 68)
		Me.btnGetLoc.Name = "btnGetLoc"
		Me.btnGetLoc.Size = New System.Drawing.Size(104, 40)
		Me.btnGetLoc.TabIndex = 6
		Me.btnGetLoc.Text = "获取库位"
		Me.btnGetLoc.UseVisualStyleBackColor = True
		'
		'btnGetMtrlDetail
		'
		Me.btnGetMtrlDetail.Location = New System.Drawing.Point(122, 68)
		Me.btnGetMtrlDetail.Name = "btnGetMtrlDetail"
		Me.btnGetMtrlDetail.Size = New System.Drawing.Size(104, 40)
		Me.btnGetMtrlDetail.TabIndex = 7
		Me.btnGetMtrlDetail.Text = "获取物料详情"
		Me.btnGetMtrlDetail.UseVisualStyleBackColor = True
		'
		'btnIcMoBill
		'
		Me.btnIcMoBill.Location = New System.Drawing.Point(232, 68)
		Me.btnIcMoBill.Name = "btnIcMoBill"
		Me.btnIcMoBill.Size = New System.Drawing.Size(104, 40)
		Me.btnIcMoBill.TabIndex = 8
		Me.btnIcMoBill.Text = "获取生产工单"
		Me.btnIcMoBill.UseVisualStyleBackColor = True
		'
		'btnGetMoLst
		'
		Me.btnGetMoLst.Location = New System.Drawing.Point(342, 68)
		Me.btnGetMoLst.Name = "btnGetMoLst"
		Me.btnGetMoLst.Size = New System.Drawing.Size(104, 40)
		Me.btnGetMoLst.TabIndex = 9
		Me.btnGetMoLst.Text = "获取工单详情"
		Me.btnGetMoLst.UseVisualStyleBackColor = True
		'
		'btnGetInStkLst
		'
		Me.btnGetInStkLst.Location = New System.Drawing.Point(12, 124)
		Me.btnGetInStkLst.Name = "btnGetInStkLst"
		Me.btnGetInStkLst.Size = New System.Drawing.Size(104, 40)
		Me.btnGetInStkLst.TabIndex = 10
		Me.btnGetInStkLst.Text = "获取入库列表"
		Me.btnGetInStkLst.UseVisualStyleBackColor = True
		'
		'btnGetInStkDetail
		'
		Me.btnGetInStkDetail.Location = New System.Drawing.Point(122, 124)
		Me.btnGetInStkDetail.Name = "btnGetInStkDetail"
		Me.btnGetInStkDetail.Size = New System.Drawing.Size(104, 40)
		Me.btnGetInStkDetail.TabIndex = 11
		Me.btnGetInStkDetail.Text = "获取入库明细"
		Me.btnGetInStkDetail.UseVisualStyleBackColor = True
		'
		'btnGetCust
		'
		Me.btnGetCust.Location = New System.Drawing.Point(452, 68)
		Me.btnGetCust.Name = "btnGetCust"
		Me.btnGetCust.Size = New System.Drawing.Size(104, 40)
		Me.btnGetCust.TabIndex = 12
		Me.btnGetCust.Text = "获取客户"
		Me.btnGetCust.UseVisualStyleBackColor = True
		'
		'btnGetMtrlSort
		'
		Me.btnGetMtrlSort.Location = New System.Drawing.Point(562, 12)
		Me.btnGetMtrlSort.Name = "btnGetMtrlSort"
		Me.btnGetMtrlSort.Size = New System.Drawing.Size(104, 40)
		Me.btnGetMtrlSort.TabIndex = 13
		Me.btnGetMtrlSort.Text = "获取物料分类"
		Me.btnGetMtrlSort.UseVisualStyleBackColor = True
		'
		'btnGetMtrlUnit
		'
		Me.btnGetMtrlUnit.Location = New System.Drawing.Point(672, 12)
		Me.btnGetMtrlUnit.Name = "btnGetMtrlUnit"
		Me.btnGetMtrlUnit.Size = New System.Drawing.Size(104, 40)
		Me.btnGetMtrlUnit.TabIndex = 14
		Me.btnGetMtrlUnit.Text = "获取物料单位"
		Me.btnGetMtrlUnit.UseVisualStyleBackColor = True
		'
		'btnGetAux
		'
		Me.btnGetAux.Location = New System.Drawing.Point(562, 68)
		Me.btnGetAux.Name = "btnGetAux"
		Me.btnGetAux.Size = New System.Drawing.Size(104, 40)
		Me.btnGetAux.TabIndex = 15
		Me.btnGetAux.Text = "获取辅助属性"
		Me.btnGetAux.UseVisualStyleBackColor = True
		'
		'btnGetPurOrderLst
		'
		Me.btnGetPurOrderLst.Location = New System.Drawing.Point(122, 169)
		Me.btnGetPurOrderLst.Name = "btnGetPurOrderLst"
		Me.btnGetPurOrderLst.Size = New System.Drawing.Size(104, 40)
		Me.btnGetPurOrderLst.TabIndex = 17
		Me.btnGetPurOrderLst.Text = "采购订单详情"
		Me.btnGetPurOrderLst.UseVisualStyleBackColor = True
		'
		'btnGetPurBill
		'
		Me.btnGetPurBill.Location = New System.Drawing.Point(12, 169)
		Me.btnGetPurBill.Name = "btnGetPurBill"
		Me.btnGetPurBill.Size = New System.Drawing.Size(104, 40)
		Me.btnGetPurBill.TabIndex = 16
		Me.btnGetPurBill.Text = "获取采购订单"
		Me.btnGetPurBill.UseVisualStyleBackColor = True
		'
		'TextBox1
		'
		Me.TextBox1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
						Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.TextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
		Me.TextBox1.Location = New System.Drawing.Point(12, 482)
		Me.TextBox1.Multiline = True
		Me.TextBox1.Name = "TextBox1"
		Me.TextBox1.ReadOnly = True
		Me.TextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both
		Me.TextBox1.Size = New System.Drawing.Size(1083, 100)
		Me.TextBox1.TabIndex = 18
		'
		'btnGetSalOrder
		'
		Me.btnGetSalOrder.Location = New System.Drawing.Point(12, 214)
		Me.btnGetSalOrder.Name = "btnGetSalOrder"
		Me.btnGetSalOrder.Size = New System.Drawing.Size(104, 40)
		Me.btnGetSalOrder.TabIndex = 19
		Me.btnGetSalOrder.Text = "获取销售订单"
		Me.btnGetSalOrder.UseVisualStyleBackColor = True
		'
		'btnQueryLock
		'
		Me.btnQueryLock.Location = New System.Drawing.Point(122, 214)
		Me.btnQueryLock.Name = "btnQueryLock"
		Me.btnQueryLock.Size = New System.Drawing.Size(104, 40)
		Me.btnQueryLock.TabIndex = 20
		Me.btnQueryLock.Text = "网络互斥查询"
		Me.btnQueryLock.UseVisualStyleBackColor = True
		'
		'btnClearLock
		'
		Me.btnClearLock.Location = New System.Drawing.Point(122, 260)
		Me.btnClearLock.Name = "btnClearLock"
		Me.btnClearLock.Size = New System.Drawing.Size(104, 40)
		Me.btnClearLock.TabIndex = 21
		Me.btnClearLock.Text = "清除网络互斥"
		Me.btnClearLock.UseVisualStyleBackColor = True
		'
		'btnSalOrderLst
		'
		Me.btnSalOrderLst.Location = New System.Drawing.Point(12, 260)
		Me.btnSalOrderLst.Name = "btnSalOrderLst"
		Me.btnSalOrderLst.Size = New System.Drawing.Size(104, 40)
		Me.btnSalOrderLst.TabIndex = 22
		Me.btnSalOrderLst.Text = "销售订单明细"
		Me.btnSalOrderLst.UseVisualStyleBackColor = True
		'
		'btnSalOutStk
		'
		Me.btnSalOutStk.Location = New System.Drawing.Point(12, 306)
		Me.btnSalOutStk.Name = "btnSalOutStk"
		Me.btnSalOutStk.Size = New System.Drawing.Size(104, 40)
		Me.btnSalOutStk.TabIndex = 24
		Me.btnSalOutStk.Text = "销售出库单列表"
		Me.btnSalOutStk.UseVisualStyleBackColor = True
		'
		'btnSalOutStkLst
		'
		Me.btnSalOutStkLst.Location = New System.Drawing.Point(122, 306)
		Me.btnSalOutStkLst.Name = "btnSalOutStkLst"
		Me.btnSalOutStkLst.Size = New System.Drawing.Size(104, 40)
		Me.btnSalOutStkLst.TabIndex = 23
		Me.btnSalOutStkLst.Text = "销售出库明细"
		Me.btnSalOutStkLst.UseVisualStyleBackColor = True
		'
		'btnSalTotalRpt
		'
		Me.btnSalTotalRpt.Location = New System.Drawing.Point(782, 12)
		Me.btnSalTotalRpt.Name = "btnSalTotalRpt"
		Me.btnSalTotalRpt.Size = New System.Drawing.Size(104, 40)
		Me.btnSalTotalRpt.TabIndex = 25
		Me.btnSalTotalRpt.Text = "销售汇总表"
		Me.btnSalTotalRpt.UseVisualStyleBackColor = True
		'
		'btnQueryStk
		'
		Me.btnQueryStk.Location = New System.Drawing.Point(782, 68)
		Me.btnQueryStk.Name = "btnQueryStk"
		Me.btnQueryStk.Size = New System.Drawing.Size(104, 40)
		Me.btnQueryStk.TabIndex = 26
		Me.btnQueryStk.Text = "查询库存"
		Me.btnQueryStk.UseVisualStyleBackColor = True
		'
		'btnSaveToDb
		'
		Me.btnSaveToDb.BackColor = System.Drawing.Color.LightGreen
		Me.btnSaveToDb.Location = New System.Drawing.Point(342, 124)
		Me.btnSaveToDb.Name = "btnSaveToDb"
		Me.btnSaveToDb.Size = New System.Drawing.Size(104, 40)
		Me.btnSaveToDb.TabIndex = 27
		Me.btnSaveToDb.Text = "保存到数据库"
		Me.btnSaveToDb.UseVisualStyleBackColor = False
		'
		'dgvMaterials
		'
		Me.dgvMaterials.AllowUserToAddRows = False
		Me.dgvMaterials.AllowUserToDeleteRows = False
		Me.dgvMaterials.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
						Or System.Windows.Forms.AnchorStyles.Left) _
						Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.dgvMaterials.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
		Me.dgvMaterials.Location = New System.Drawing.Point(232, 124)
		Me.dgvMaterials.Name = "dgvMaterials"
		Me.dgvMaterials.ReadOnly = True
		Me.dgvMaterials.RowHeadersWidth = 40
		Me.dgvMaterials.RowTemplate.Height = 23
		Me.dgvMaterials.Size = New System.Drawing.Size(863, 345)
		Me.dgvMaterials.TabIndex = 28
		'
		'FrmTest
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(1107, 594)
		Me.Controls.Add(Me.dgvMaterials)
		Me.Controls.Add(Me.btnSaveToDb)
		Me.Controls.Add(Me.btnQueryStk)
		Me.Controls.Add(Me.btnSalTotalRpt)
		Me.Controls.Add(Me.btnSalOutStk)
		Me.Controls.Add(Me.btnSalOutStkLst)
		Me.Controls.Add(Me.btnSalOrderLst)
		Me.Controls.Add(Me.btnClearLock)
		Me.Controls.Add(Me.btnQueryLock)
		Me.Controls.Add(Me.btnGetSalOrder)
		Me.Controls.Add(Me.TextBox1)
		Me.Controls.Add(Me.btnGetPurOrderLst)
		Me.Controls.Add(Me.btnGetPurBill)
		Me.Controls.Add(Me.btnGetAux)
		Me.Controls.Add(Me.btnGetMtrlUnit)
		Me.Controls.Add(Me.btnGetMtrlSort)
		Me.Controls.Add(Me.btnGetCust)
		Me.Controls.Add(Me.btnGetInStkDetail)
		Me.Controls.Add(Me.btnGetInStkLst)
		Me.Controls.Add(Me.btnGetMoLst)
		Me.Controls.Add(Me.btnIcMoBill)
		Me.Controls.Add(Me.btnGetMtrlDetail)
		Me.Controls.Add(Me.btnGetLoc)
		Me.Controls.Add(Me.btnGetUnits)
		Me.Controls.Add(Me.btnSaveInStk)
		Me.Controls.Add(Me.btnGetEmp)
		Me.Controls.Add(Me.btnGetDept)
		Me.Controls.Add(Me.btnGetWh)
		Me.Controls.Add(Me.btnGetMtrl)
		Me.Name = "FrmTest"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
		Me.Text = "星辰API调用 - 物料管理"
		CType(Me.dgvMaterials, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub
	Friend WithEvents btnGetMtrl As Button
	Friend WithEvents btnGetWh As Button
	Friend WithEvents btnGetDept As Button
	Friend WithEvents btnGetEmp As Button
	Friend WithEvents btnSaveInStk As Button
	Friend WithEvents btnGetUnits As Button
	Friend WithEvents btnGetLoc As Button
	Friend WithEvents btnGetMtrlDetail As Button
	Friend WithEvents btnIcMoBill As Button
	Friend WithEvents btnGetMoLst As Button
	Friend WithEvents btnGetInStkLst As Button
	Friend WithEvents btnGetInStkDetail As Button
	Friend WithEvents btnGetCust As System.Windows.Forms.Button
	Friend WithEvents btnGetMtrlSort As System.Windows.Forms.Button
	Friend WithEvents btnGetMtrlUnit As System.Windows.Forms.Button
	Friend WithEvents btnGetAux As System.Windows.Forms.Button
	Friend WithEvents btnGetPurOrderLst As System.Windows.Forms.Button
	Friend WithEvents btnGetPurBill As System.Windows.Forms.Button
	Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
	Friend WithEvents btnGetSalOrder As System.Windows.Forms.Button
	Friend WithEvents btnQueryLock As System.Windows.Forms.Button
	Friend WithEvents btnClearLock As System.Windows.Forms.Button
	Friend WithEvents btnSalOrderLst As System.Windows.Forms.Button
	Friend WithEvents btnSalOutStk As System.Windows.Forms.Button
	Friend WithEvents btnSalOutStkLst As System.Windows.Forms.Button
	Friend WithEvents btnSalTotalRpt As System.Windows.Forms.Button
	Friend WithEvents btnQueryStk As System.Windows.Forms.Button
	Friend WithEvents btnSaveToDb As System.Windows.Forms.Button
	Friend WithEvents dgvMaterials As System.Windows.Forms.DataGridView

End Class