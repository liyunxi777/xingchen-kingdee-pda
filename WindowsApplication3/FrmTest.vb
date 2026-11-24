Imports System.Net
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Text
Imports System.Data
Imports System.IO

Public Class FrmTest
	Dim strAppSecret As String = ""
	Dim strAppToken As String = ""
	Dim strAccessToken As String = ""
	Dim AppKey As String = "KM9vdpEa"    '
	Dim clientId As String = "368805"
	Dim clientSecret As String = "858679453e5074b823dff4e759dc85e6"
	Dim outerInstanceId As String = "604422060890001408"
	Dim uId As String = ""

	' 保存最后获取的物料数据用于数据库写入
	Private lastMaterialTable As DataTable = Nothing

	Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		Dim clsX As New LrGetToken.clsGetToken()
		strAppSecret = clsX.GetAppSecretB(clientId, clientSecret, AppKey, outerInstanceId)
		If strAppSecret = "" Then
			Exit Sub
		End If
		Dim jo As JObject = JsonConvert.DeserializeObject(strAppSecret)
		For i As Integer = 0 To jo("data").Count - 1
			If jo("data")(i).Item("appKey") = AppKey Then
				strAppSecret = jo("data")(i).Item("appSecret")
				Exit For
			End If
		Next
		strAppToken = clsX.GetToken(clientId, clientSecret, AppKey, strAppSecret)
		jo = JsonConvert.DeserializeObject(strAppToken)
		strAppToken = jo("data").Item("app-token")
		strAccessToken = jo("data").Item("access_token")
		uId = jo("data").Item("uid")

		' 初始化 DataGridView 列
		InitDataGridView()
	End Sub

	''' <summary>
	''' 初始化 DataGridView 列定义
	''' </summary>
	Private Sub InitDataGridView()
		dgvMaterials.Columns.Clear()
		dgvMaterials.Columns.Add("material_number", "物料编码")
		dgvMaterials.Columns.Add("material_name", "物料名称")
		dgvMaterials.Columns.Add("material_full_name", "物料全名")
		dgvMaterials.Columns.Add("specification", "规格")
		dgvMaterials.Columns.Add("model", "型号")
		dgvMaterials.Columns.Add("barcode", "条码")
		dgvMaterials.Columns.Add("material_group_number", "分类编码")
		dgvMaterials.Columns.Add("material_group_name", "分类名称")
		dgvMaterials.Columns.Add("measure_unit_name", "基本单位")
		dgvMaterials.Columns.Add("alt_units", "辅助单位")
		dgvMaterials.Columns.Add("aux_prop_name", "辅助属性")
		dgvMaterials.Columns.Add("labels", "标签")
		dgvMaterials.Columns.Add("custom_fields", "自定义字段")
		dgvMaterials.Columns.Add("is_multi_unit", "多单位")
		dgvMaterials.Columns.Add("enable", "启用")
		dgvMaterials.Columns.Add("remark", "备注")
		dgvMaterials.Columns.Add("create_time", "创建时间")
		dgvMaterials.Columns.Add("modify_time", "修改时间")
		dgvMaterials.Columns.Add("material_id", "物料ID")
		For Each col As DataGridViewColumn In dgvMaterials.Columns
			col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
		Next
	End Sub

	''' <summary>
	''' 将API返回的JSON拍平为DataTable
	''' </summary>
	Private Function FlattenMaterialJson(jsonStr As String) As DataTable
		Dim dt As New DataTable()
		dt.Columns.Add("material_id", GetType(String))
		dt.Columns.Add("material_number", GetType(String))
		dt.Columns.Add("material_name", GetType(String))
		dt.Columns.Add("material_full_name", GetType(String))
		dt.Columns.Add("specification", GetType(String))
		dt.Columns.Add("model", GetType(String))
		dt.Columns.Add("barcode", GetType(String))
		dt.Columns.Add("material_group_id", GetType(String))
		dt.Columns.Add("material_group_name", GetType(String))
		dt.Columns.Add("material_group_number", GetType(String))
		dt.Columns.Add("measure_unit_id", GetType(String))
		dt.Columns.Add("measure_unit_name", GetType(String))
		dt.Columns.Add("is_multi_unit", GetType(String))
		dt.Columns.Add("primary_unit_name", GetType(String))
		dt.Columns.Add("primary_unit_convert", GetType(String))
		dt.Columns.Add("alt_units", GetType(String))
		dt.Columns.Add("units_json", GetType(String))
		dt.Columns.Add("aux_prop_id", GetType(String))
		dt.Columns.Add("aux_prop_name", GetType(String))
		dt.Columns.Add("labels", GetType(String))
		dt.Columns.Add("labels_json", GetType(String))
		dt.Columns.Add("custom_fields", GetType(String))
		dt.Columns.Add("custom_field_json", GetType(String))
		dt.Columns.Add("enable", GetType(String))
		dt.Columns.Add("remark", GetType(String))
		dt.Columns.Add("create_time", GetType(String))
		dt.Columns.Add("modify_time", GetType(String))
		dt.Columns.Add("row_json", GetType(String))

		Dim jo As JObject = JsonConvert.DeserializeObject(jsonStr)
		Dim rows As JArray = jo("data")("rows")

		For Each row As JObject In rows
			Dim dr As DataRow = dt.NewRow()
			dr("material_id") = ValOrEmpty(row, "id")
			dr("material_number") = ValOrEmpty(row, "number")
			dr("material_name") = ValOrEmpty(row, "name")
			dr("material_full_name") = ValOrEmpty(row, "full_name")
			dr("specification") = ValOrEmpty(row, "spec")
			dr("model") = ValOrEmpty(row, "model")
			dr("barcode") = ValOrEmpty(row, "barcode")
			dr("material_group_id") = ValOrEmpty(row, "material_group_id")
			dr("material_group_name") = ValOrEmpty(row, "material_group_name")
			dr("material_group_number") = ValOrEmpty(row, "material_group_number")
			dr("measure_unit_id") = ValOrEmpty(row, "measure_unit_id")
			dr("measure_unit_name") = ValOrEmpty(row, "measure_unit_name")
			dr("is_multi_unit") = If(ValOrEmpty(row, "is_multi_unit") = "1", "是", "否")
			dr("aux_prop_id") = ValOrEmpty(row, "aux_prop_id")
			dr("aux_prop_name") = ValOrEmpty(row, "aux_prop_name")
			dr("enable") = If(ValOrEmpty(row, "enable") = "1", "是", "否")
			dr("remark") = ValOrEmpty(row, "remark")
			dr("create_time") = ValOrEmpty(row, "create_time")
			dr("modify_time") = ValOrEmpty(row, "modify_time")

			' ---------- 拍平 units ----------
			Dim unitsStr As String = ValOrEmpty(row, "units")
			dr("units_json") = unitsStr
			Dim unitsArr As JArray = Nothing
			If Not String.IsNullOrEmpty(unitsStr) Then
				Try
					unitsArr = JArray.Parse(unitsStr)
				Catch
				End Try
			End If
			Dim primaryUnit As String = ""
			Dim altUnits As New List(Of String)
			If unitsArr IsNot Nothing Then
				For Each u As JObject In unitsArr
					If ValOrEmpty(u, "unit_type") = "1" Then
						primaryUnit = ValOrEmpty(u, "unit_name")
						dr("primary_unit_convert") = ValOrEmpty(u, "unit_convert")
					Else
						altUnits.Add(ValOrEmpty(u, "unit_name") & "(换算率:" & ValOrEmpty(u, "unit_convert") & ")")
					End If
				Next
			End If
			dr("primary_unit_name") = primaryUnit
			dr("alt_units") = String.Join(", ", altUnits)

			' ---------- 拍平 labels ----------
			Dim labelsStr As String = ValOrEmpty(row, "mul_label")
			dr("labels_json") = labelsStr
			Dim labelsArr As JArray = Nothing
			If Not String.IsNullOrEmpty(labelsStr) Then
				Try
					labelsArr = JArray.Parse(labelsStr)
				Catch
				End Try
			End If
			Dim labelNames As New List(Of String)
			If labelsArr IsNot Nothing Then
				For Each lb As JObject In labelsArr
					labelNames.Add(ValOrEmpty(lb, "label_name"))
				Next
			End If
			dr("labels") = String.Join(", ", labelNames)

			' ---------- 拍平 custom_field ----------
			Dim cfStr As String = ValOrEmpty(row, "custom_field")
			dr("custom_field_json") = cfStr
			Dim cfObj As JObject = Nothing
			If Not String.IsNullOrEmpty(cfStr) Then
				Try
					cfObj = JObject.Parse(cfStr)
				Catch
				End Try
			End If
			Dim cfPairs As New List(Of String)
			If cfObj IsNot Nothing Then
				For Each prop As JProperty In cfObj.Properties()
					cfPairs.Add(prop.Name & "=" & prop.Value.ToString())
				Next
			End If
			dr("custom_fields") = String.Join("; ", cfPairs)

			dr("row_json") = row.ToString()
			dt.Rows.Add(dr)
		Next
		Return dt
	End Function

	''' <summary>
	''' 安全获取JObject中的值
	''' </summary>
	Private Function ValOrEmpty(jo As JObject, key As String) As String
		Dim token As JToken = Nothing
		If jo.TryGetValue(key, token) AndAlso token IsNot Nothing Then
			Return token.ToString()
		End If
		Return ""
	End Function

	''' <summary>
	''' 将DataTable数据显示到DataGridView
	''' </summary>
	Private Sub PopulateGridView(dt As DataTable)
		dgvMaterials.Rows.Clear()
		For Each dr As DataRow In dt.Rows
			Dim rowIdx As Integer = dgvMaterials.Rows.Add()
			Dim dgvRow As DataGridViewRow = dgvMaterials.Rows(rowIdx)
			dgvRow.Cells("material_number").Value = dr("material_number")
			dgvRow.Cells("material_name").Value = dr("material_name")
			dgvRow.Cells("material_full_name").Value = dr("material_full_name")
			dgvRow.Cells("specification").Value = dr("specification")
			dgvRow.Cells("model").Value = dr("model")
			dgvRow.Cells("barcode").Value = dr("barcode")
			dgvRow.Cells("material_group_number").Value = dr("material_group_number")
			dgvRow.Cells("material_group_name").Value = dr("material_group_name")
			dgvRow.Cells("measure_unit_name").Value = dr("primary_unit_name")
			dgvRow.Cells("alt_units").Value = dr("alt_units")
			dgvRow.Cells("aux_prop_name").Value = dr("aux_prop_name")
			dgvRow.Cells("labels").Value = dr("labels")
			dgvRow.Cells("custom_fields").Value = dr("custom_fields")
			dgvRow.Cells("is_multi_unit").Value = dr("is_multi_unit")
			dgvRow.Cells("enable").Value = dr("enable")
			dgvRow.Cells("remark").Value = dr("remark")
			dgvRow.Cells("create_time").Value = dr("create_time")
			dgvRow.Cells("modify_time").Value = dr("modify_time")
			dgvRow.Cells("material_id").Value = dr("material_id")
		Next
		dgvMaterials.AutoResizeColumns()
	End Sub

	Private Sub btnGetMtrl_Click(sender As Object, e As EventArgs) Handles btnGetMtrl.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim paramLst As New List(Of LrGetToken.LrParam)
		Dim param As New LrGetToken.LrParam()
		param.pKey = "page" : param.pValue = "1"
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "page_size" : param.pValue = "100"
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "show_units" : param.pValue = True
		paramLst.Add(param)
		Dim strJson As String = clsX.GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/bd/material")
		TextBox1.Text = strJson

		' 拍平JSON显示到表格
		Try
			lastMaterialTable = FlattenMaterialJson(strJson)
			PopulateGridView(lastMaterialTable)
			Dim total As String = "0"
			Try
				Dim jo As JObject = JsonConvert.DeserializeObject(strJson)
				total = jo("data")("total").ToString()
			Catch
			End Try
			Me.Text = "星辰API调用 - 物料管理 [共" & total & "条]"
		Catch ex As Exception
			MsgBox("数据解析失败: " & ex.Message, MsgBoxStyle.Critical, "错误")
		End Try
	End Sub

	''' <summary>
	''' 获取XML数据库文件路径</summary>
	Private Function GetDbPath() As String
		Return Path.Combine(Application.StartupPath, "MaterialsData.xml")
	End Function

	''' <summary>
	''' 初始化数据库（DataSet XML持久化，首次运行时创建空库）</summary>
	Private Function LoadDatabase() As DataSet
		Dim ds As New DataSet("MaterialsDB")
		Dim dbPath As String = GetDbPath()

		If File.Exists(dbPath) Then
			ds.ReadXml(dbPath)
			Return ds
		End If

		' 创建物料主表
		Dim dtMat As New DataTable("materials")
		dtMat.Columns.Add("material_id", GetType(String))
		dtMat.Columns.Add("material_number", GetType(String))
		dtMat.Columns.Add("material_name", GetType(String))
		dtMat.Columns.Add("material_full_name", GetType(String))
		dtMat.Columns.Add("specification", GetType(String))
		dtMat.Columns.Add("model", GetType(String))
		dtMat.Columns.Add("barcode", GetType(String))
		dtMat.Columns.Add("material_group_id", GetType(String))
		dtMat.Columns.Add("material_group_name", GetType(String))
		dtMat.Columns.Add("material_group_number", GetType(String))
		dtMat.Columns.Add("measure_unit_id", GetType(String))
		dtMat.Columns.Add("measure_unit_name", GetType(String))
		dtMat.Columns.Add("is_multi_unit", GetType(String))
		dtMat.Columns.Add("primary_unit_name", GetType(String))
		dtMat.Columns.Add("primary_unit_convert", GetType(String))
		dtMat.Columns.Add("alt_units", GetType(String))
		dtMat.Columns.Add("aux_prop_id", GetType(String))
		dtMat.Columns.Add("aux_prop_name", GetType(String))
		dtMat.Columns.Add("labels", GetType(String))
		dtMat.Columns.Add("custom_fields", GetType(String))
		dtMat.Columns.Add("enable", GetType(String))
		dtMat.Columns.Add("remark", GetType(String))
		dtMat.Columns.Add("create_time", GetType(String))
		dtMat.Columns.Add("modify_time", GetType(String))
		dtMat.Columns.Add("units_json", GetType(String))
		dtMat.Columns.Add("labels_json", GetType(String))
		dtMat.Columns.Add("custom_field_json", GetType(String))
		dtMat.Columns.Add("row_json", GetType(String))
		dtMat.Columns.Add("sync_time", GetType(String))
		dtMat.PrimaryKey = New DataColumn() {dtMat.Columns("material_id")}
		ds.Tables.Add(dtMat)

		' 创建单位明细表
		Dim dtUnits As New DataTable("material_units")
		dtUnits.Columns.Add("id", GetType(Integer))
		dtUnits.Columns.Add("material_id", GetType(String))
		dtUnits.Columns.Add("unit_id", GetType(String))
		dtUnits.Columns.Add("unit_name", GetType(String))
		dtUnits.Columns.Add("unit_type", GetType(String))
		dtUnits.Columns.Add("unit_convert", GetType(String))
		dtUnits.Columns("id").AutoIncrement = True
		dtUnits.Columns("id").AutoIncrementSeed = 1
		dtUnits.Columns("id").AutoIncrementStep = 1
		dtUnits.PrimaryKey = New DataColumn() {dtUnits.Columns("id")}
		ds.Tables.Add(dtUnits)

		' 创建标签表
		Dim dtLabels As New DataTable("material_labels")
		dtLabels.Columns.Add("id", GetType(Integer))
		dtLabels.Columns.Add("material_id", GetType(String))
		dtLabels.Columns.Add("label_id", GetType(String))
		dtLabels.Columns.Add("label_name", GetType(String))
		dtLabels.Columns("id").AutoIncrement = True
		dtLabels.Columns("id").AutoIncrementSeed = 1
		dtLabels.Columns("id").AutoIncrementStep = 1
		dtLabels.PrimaryKey = New DataColumn() {dtLabels.Columns("id")}
		ds.Tables.Add(dtLabels)

		' 创建同步日志表
		Dim dtLog As New DataTable("sync_log")
		dtLog.Columns.Add("id", GetType(Integer))
		dtLog.Columns.Add("sync_time", GetType(String))
		dtLog.Columns.Add("record_count", GetType(Integer))
		dtLog.Columns.Add("status", GetType(String))
		dtLog.Columns.Add("error_msg", GetType(String))
		dtLog.Columns.Add("api_endpoint", GetType(String))
		dtLog.Columns("id").AutoIncrement = True
		dtLog.Columns("id").AutoIncrementSeed = 1
		dtLog.Columns("id").AutoIncrementStep = 1
		dtLog.PrimaryKey = New DataColumn() {dtLog.Columns("id")}
		ds.Tables.Add(dtLog)

		' 建立外键关系
		ds.Relations.Add("FK_material_units", dtMat.Columns("material_id"), dtUnits.Columns("material_id"))
		ds.Relations.Add("FK_material_labels", dtMat.Columns("material_id"), dtLabels.Columns("material_id"))

		ds.AcceptChanges()
		ds.WriteXml(dbPath, XmlWriteMode.WriteSchema)
		Return ds
	End Function

	Private Sub btnSaveToDb_Click(sender As Object, e As EventArgs) Handles btnSaveToDb.Click
		If lastMaterialTable Is Nothing OrElse lastMaterialTable.Rows.Count = 0 Then
			MsgBox("请先点击【获取物料】按钮获取数据！", MsgBoxStyle.Information, "提示")
			Exit Sub
		End If
		Try
			Dim ds As DataSet = LoadDatabase()
			Dim dtMat As DataTable = ds.Tables("materials")
			Dim dtUnits As DataTable = ds.Tables("material_units")
			Dim dtLabels As DataTable = ds.Tables("material_labels")
			Dim dtLog As DataTable = ds.Tables("sync_log")
			Dim savedCount As Integer = 0
			Dim updatedCount As Integer = 0
			Dim nowStr As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")

			For Each dr As DataRow In lastMaterialTable.Rows
				Dim materialId As String = dr("material_id").ToString()
				Dim existingRows() As DataRow = dtMat.Select("material_id = '" & materialId.Replace("'", "''") & "'")

				Dim targetRow As DataRow
				If existingRows.Length > 0 Then
					targetRow = existingRows(0)
					updatedCount += 1
				Else
					targetRow = dtMat.NewRow()
					savedCount += 1
				End If

				targetRow("material_id") = dr("material_id")
				targetRow("material_number") = dr("material_number")
				targetRow("material_name") = dr("material_name")
				targetRow("material_full_name") = dr("material_full_name")
				targetRow("specification") = dr("specification")
				targetRow("model") = dr("model")
				targetRow("barcode") = dr("barcode")
				targetRow("material_group_id") = dr("material_group_id")
				targetRow("material_group_name") = dr("material_group_name")
				targetRow("material_group_number") = dr("material_group_number")
				targetRow("measure_unit_id") = dr("measure_unit_id")
				targetRow("measure_unit_name") = dr("measure_unit_name")
				targetRow("is_multi_unit") = dr("is_multi_unit")
				targetRow("primary_unit_name") = dr("primary_unit_name")
				targetRow("primary_unit_convert") = dr("primary_unit_convert")
				targetRow("alt_units") = dr("alt_units")
				targetRow("aux_prop_id") = dr("aux_prop_id")
				targetRow("aux_prop_name") = dr("aux_prop_name")
				targetRow("labels") = dr("labels")
				targetRow("custom_fields") = dr("custom_fields")
				targetRow("enable") = dr("enable")
				targetRow("remark") = dr("remark")
				targetRow("create_time") = dr("create_time")
				targetRow("modify_time") = dr("modify_time")
				targetRow("units_json") = dr("units_json")
				targetRow("labels_json") = dr("labels_json")
				targetRow("custom_field_json") = dr("custom_field_json")
				targetRow("row_json") = dr("row_json")
				targetRow("sync_time") = nowStr

				If existingRows.Length = 0 Then
					dtMat.Rows.Add(targetRow)
				End If

				' 删除旧单位明细
				Dim oldUnits() As DataRow = dtUnits.Select("material_id = '" & materialId.Replace("'", "''") & "'")
				For Each ou As DataRow In oldUnits
					ou.Delete()
				Next

				' 保存单位明细
				If Not String.IsNullOrEmpty(dr("units_json").ToString()) Then
					Try
						Dim unitsArr As JArray = JArray.Parse(dr("units_json").ToString())
						For Each u As JObject In unitsArr
							Dim unitRow As DataRow = dtUnits.NewRow()
							unitRow("material_id") = materialId
							unitRow("unit_id") = ValOrEmpty(u, "unit_id")
							unitRow("unit_name") = ValOrEmpty(u, "unit_name")
							unitRow("unit_type") = ValOrEmpty(u, "unit_type")
							unitRow("unit_convert") = ValOrEmpty(u, "unit_convert")
							dtUnits.Rows.Add(unitRow)
						Next
					Catch
					End Try
				End If

				' 删除旧标签
				Dim oldLabels() As DataRow = dtLabels.Select("material_id = '" & materialId.Replace("'", "''") & "'")
				For Each ol As DataRow In oldLabels
					ol.Delete()
				Next

				' 保存标签明细
				If Not String.IsNullOrEmpty(dr("labels_json").ToString()) Then
					Try
						Dim labelsArr As JArray = JArray.Parse(dr("labels_json").ToString())
						For Each lb As JObject In labelsArr
							Dim labelRow As DataRow = dtLabels.NewRow()
							labelRow("material_id") = materialId
							labelRow("label_id") = ValOrEmpty(lb, "label_id")
							labelRow("label_name") = ValOrEmpty(lb, "label_name")
							dtLabels.Rows.Add(labelRow)
						Next
					Catch
					End Try
				End If
			Next

			' 记录同步日志
			Dim logRow As DataRow = dtLog.NewRow()
			logRow("sync_time") = nowStr
			logRow("record_count") = lastMaterialTable.Rows.Count
			logRow("status") = "success"
			logRow("api_endpoint") = "jdy/v2/bd/material"
			dtLog.Rows.Add(logRow)

			ds.AcceptChanges()
			ds.WriteXml(GetDbPath(), XmlWriteMode.WriteSchema)

			MsgBox("保存完成！新增 " & savedCount & " 条，更新 " & updatedCount & " 条。" &
				   vbCrLf & "数据库位置: " & GetDbPath(),
				   MsgBoxStyle.Information, "保存成功")
		Catch ex As Exception
			MsgBox("保存失败: " & ex.Message, MsgBoxStyle.Critical, "错误")
		End Try
	End Sub

	' ========== 以下为原有按钮事件，保持不变 ==========

	Private Sub btnGetWh_Click(sender As Object, e As EventArgs) Handles btnGetWh.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim strJson As String = clsX.GetDatum(clientId, clientSecret, AppKey, strAppSecret, strAppToken, "", "", "jdy/v2/bd/store", 1, 100)
		TextBox1.Text = strJson
	End Sub

	Private Sub btnGetDept_Click(sender As Object, e As EventArgs) Handles btnGetDept.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim strJson As String = clsX.GetDatum(clientId, clientSecret, AppKey, strAppSecret, strAppToken, "", "", "jdy/v2/bd/department", 1, 100)
		TextBox1.Text = strJson
	End Sub

	Private Sub btnGetEmp_Click(sender As Object, e As EventArgs) Handles btnGetEmp.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim strJson As String = clsX.GetDatum(clientId, clientSecret, AppKey, strAppSecret, strAppToken, "", "", "jdy/v2/bd/emp", 1, 100)
		TextBox1.Text = strJson
	End Sub

	Private Sub btnGetCust_Click(sender As Object, e As EventArgs) Handles btnGetCust.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim paramLst As New List(Of LrGetToken.LrParam)
		Dim param As New LrGetToken.LrParam()
		param.pKey = "page_size" : param.pValue = "100"
		paramLst.Add(param)
		Dim strJson As String = clsX.GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/bd/customer")
		TextBox1.Text = strJson
	End Sub

	Private Sub btnGetUnits_Click(sender As Object, e As EventArgs) Handles btnGetUnits.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim strJson As String = clsX.GetDatum(clientId, clientSecret, AppKey, strAppSecret, strAppToken, "", "", "jdy/v2/bd/measure_unit", 1, 100)
		TextBox1.Text = strJson
	End Sub

	Private Sub btnGetLoc_Click(sender As Object, e As EventArgs) Handles btnGetLoc.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim strJson As String = clsX.GetDatum(clientId, clientSecret, AppKey, strAppSecret, strAppToken, "", "", "jdy/v2/bd/spaces", 1, 100)
		TextBox1.Text = strJson
	End Sub

	Private Sub btnSaveInStk_Click(sender As Object, e As EventArgs) Handles btnSaveInStk.Click
		Dim clsX As New LrGetToken.clsGetToken
		Dim strJson As String = clsX.SaveBill(clientId, clientSecret, strAppToken)
		TextBox1.Text = strJson
	End Sub

	Private Sub btnGetMtrlDetail_Click(sender As Object, e As EventArgs) Handles btnGetMtrlDetail.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim paramLst As New List(Of LrGetToken.LrParam)
		Dim param As New LrGetToken.LrParam()
		param.pKey = "number" : param.pValue = "CYJXG0014"
		paramLst.Add(param)
		Dim strJson As String = clsX.GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/bd/material_detail")
		TextBox1.Text = strJson
	End Sub

	Private Sub btnIcMoBill_Click(sender As Object, e As EventArgs) Handles btnIcMoBill.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim paramLst As New List(Of LrGetToken.LrParam)
		Dim param As New LrGetToken.LrParam()
		param = New LrGetToken.LrParam()
		param.pKey = "search" : param.pValue = "SCRW-20261101-00021"
		paramLst.Add(param)
		Dim strJson As String = clsX.GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/pm/mo_taskbill")
		TextBox1.Text = strJson
	End Sub

	Private Sub btnGetMoLst_Click(sender As Object, e As EventArgs) Handles btnGetMoLst.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim paramLst As New List(Of LrGetToken.LrParam)
		Dim param As New LrGetToken.LrParam()
		param = New LrGetToken.LrParam()
		param.pKey = "number" : param.pValue = "SCRW-20261101-00021"
		paramLst.Add(param)
		Dim strJson As String = clsX.GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/pm/mo_taskbill_detail")
		TextBox1.Text = strJson
	End Sub

	Private Sub btnGetInStkLst_Click(sender As Object, e As EventArgs) Handles btnGetInStkLst.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim paramLst As New List(Of LrGetToken.LrParam)
		Dim param As New LrGetToken.LrParam()
		param = New LrGetToken.LrParam()
		param.pKey = "bill_status" : param.pValue = "Z"
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "end_date" : param.pValue = "2024-08-21"
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "page" : param.pValue = "1"
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "page_size" : param.pValue = "100"
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "start_date" : param.pValue = "2024-08-21"
		paramLst.Add(param)
		Dim strJson As String = clsX.GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/scm/inv_product")
		TextBox1.Text = strJson
	End Sub

	Private Sub btnGetInStkDetail_Click(sender As Object, e As EventArgs) Handles btnGetInStkDetail.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim paramLst As New List(Of LrGetToken.LrParam)
		Dim param As New LrGetToken.LrParam()
		param.pKey = "id" : param.pValue = "2022101228657349632"
		paramLst.Add(param)
		Dim strJson As String = clsX.GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/scm/inv_product_detail")
		TextBox1.Text = strJson
	End Sub

	Private Sub btnGetMtrlSort_Click(sender As Object, e As EventArgs) Handles btnGetMtrlSort.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim strJson As String = clsX.GetDatum(clientId, clientSecret, AppKey, strAppSecret, strAppToken, "", "", "jdy/v2/bd/material_group", 1, 1000)
		TextBox1.Text = strJson
		Debug.WriteLine(strJson)
	End Sub

	Private Sub btnGetMtrlUnit_Click(sender As Object, e As EventArgs) Handles btnGetMtrlUnit.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim strJson As String = clsX.GetDatum(clientId, clientSecret, AppKey, strAppSecret, strAppToken, "", "", "jdy/v2/bd/material_unit", 1, 1000)
		TextBox1.Text = strJson
		Debug.WriteLine(strJson)
	End Sub

	Private Sub btnGetAux_Click(sender As Object, e As EventArgs) Handles btnGetAux.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim paramLst As New List(Of LrGetToken.LrParam)
		Dim param As New LrGetToken.LrParam()
		param.pKey = "enable" : param.pValue = "1"
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "group_id" : param.pValue = "2"
		paramLst.Add(param)
		Dim strJson As String = clsX.GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/bd/aux_attribute")
		TextBox1.Text = strJson
		Debug.WriteLine(strJson)
	End Sub

	Private Sub btnGetPurBill_Click(sender As Object, e As EventArgs) Handles btnGetPurBill.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim paramLst As New List(Of LrGetToken.LrParam)
		Dim param As New LrGetToken.LrParam()
		param = New LrGetToken.LrParam()
		param.pKey = "bill_no" : param.pValue = "CGDD-20260812-00001"
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "page_size" : param.pValue = "100"
		paramLst.Add(param)
		Dim strJson As String = clsX.GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/scm/pur_order")
		TextBox1.Text = strJson
	End Sub

	Private Sub btnGetPurOrderLst_Click(sender As Object, e As EventArgs) Handles btnGetPurOrderLst.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim paramLst As New List(Of LrGetToken.LrParam)
		Dim param As New LrGetToken.LrParam()
		param.pKey = "number" : param.pValue = "CGDD-20260602-00001"
		paramLst.Add(param)
		Dim strJson As String = clsX.GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/scm/pur_order_detail")
		TextBox1.Text = strJson
	End Sub

	Private Sub btnQueryLock_Click(sender As Object, e As EventArgs) Handles btnQueryLock.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim paramLst As New List(Of LrGetToken.LrParam)
		Dim param As New LrGetToken.LrParam()
		param.pKey = "bill_type" : param.pValue = "mo_task_bill"
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "page" : param.pValue = "1"
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "page_size" : param.pValue = "100"
		paramLst.Add(param)
		Dim strJson As String = clsX.GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/sys/mutex_list")
		TextBox1.Text = strJson
		Dim jo As JObject = JsonConvert.DeserializeObject(strJson)
		If jo("data")("count").ToString() > "0" Then
			Dim jrLst As JArray = jo("data")("rows")
			Dim strId As String = ""
			For Each joX As JObject In jrLst
				If strId = "" Then
					strId = joX("id").ToString()
				Else
					strId = strId & "," & joX("id").ToString()
				End If
			Next
			TextBox1.Text = strId
		End If
	End Sub

	Private Sub btnClearLock_Click(sender As Object, e As EventArgs) Handles btnClearLock.Click
		If TextBox1.Text = "" Then
			MsgBox("请先获取网络互斥信息！", MsgBoxStyle.Information, "提示")
			Exit Sub
		End If
		If MsgBox("确定清除吗？", MsgBoxStyle.OkCancel + MsgBoxStyle.Question, "提示") = MsgBoxResult.Cancel Then Exit Sub
		Dim idLst() As String = TextBox1.Text.Split(",")
		Dim client As New LrGetToken.APIClient(clientId, clientSecret)
		For i As Integer = 0 To idLst.Length - 1
			Dim jo As New JObject
			jo.Add("bill_type", "mo_task_bill")
			jo.Add("id", idLst(i))
			jo.Add("is_force", True)
			jo.Add("sign", "1")
			Dim api_nonce As String = New Random().Next(100000000, 999999999)
			Dim timeStamp As Integer = (DateTime.Now.ToLocalTime() - DateTime.Parse("1970-01-01").ToLocalTime()).TotalSeconds
			Dim strJson As String = client.HttpPost("jdy/v2/sys/mutex_operate", clientId, strAppToken, api_nonce.ToString(), timeStamp.ToString(), jo.ToString())
			TextBox1.Text = TextBox1.Text & Chr(13) & Chr(10) & strJson
		Next
	End Sub

	Private Sub btnUpdateCustField_Click(sender As Object, e As EventArgs)
		Dim jsonRoot As New JObject()
		jsonRoot.Add("bill_date", "2026-09-12")
		jsonRoot.Add("bill_no", "SCRW-20260912-00001")
		Dim custom_field As New JObject()
		custom_field.Add("custom_field__1__61alganrdcod", "MES已读单")
		jsonRoot.Add("custom_field", custom_field)
		jsonRoot.Add("dept_id", "1840551019277407232")
		jsonRoot.Add("id", "2567105010575950848")
		jsonRoot.Add("trans_type_id", "1601")
		Dim client As New LrGetToken.APIClient(clientId, clientSecret)
		Dim api_nonce As String = New Random().Next(100000000, 999999999)
		Dim timeStamp As Integer = (DateTime.Now.ToLocalTime() - DateTime.Parse("1970-01-01").ToLocalTime()).TotalSeconds
		Dim strJson As String = client.HttpPost("jdy/v2/pm/mo_task_bill", clientId, strAppToken, api_nonce.ToString(), timeStamp.ToString(), jsonRoot)
		TextBox1.Text = strJson
	End Sub

	Private Sub btnGetSalOrder_Click(sender As Object, e As EventArgs) Handles btnGetSalOrder.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim paramLst As New List(Of LrGetToken.LrParam)
		Dim param As New LrGetToken.LrParam()
		param = New LrGetToken.LrParam()
		param.pKey = "bill_no" : param.pValue = "SCRW-20261004-00006"
		paramLst.Add(param)
		Dim strJson As String = clsX.GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/scm/sal_order")
		TextBox1.Text = strJson
	End Sub

	Private Sub btnSalOrderLst_Click(sender As Object, e As EventArgs) Handles btnSalOrderLst.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim paramLst As New List(Of LrGetToken.LrParam)
		Dim param As New LrGetToken.LrParam()
		param = New LrGetToken.LrParam()
		param.pKey = "id" : param.pValue = "2442869596886233088"
		paramLst.Add(param)
		Dim strJson As String = clsX.GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/scm/sal_order_detail")
		TextBox1.Text = strJson
	End Sub

	Private Sub btnSalOutStk_Click(sender As Object, e As EventArgs) Handles btnSalOutStk.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim paramLst As New List(Of LrGetToken.LrParam)
		Dim param As New LrGetToken.LrParam()
		param = New LrGetToken.LrParam()
		param.pKey = "end_bill_date" : param.pValue = Today.ToString("yyyy-MM-dd")
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "filter_zero" : param.pValue = True
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "page_size" : param.pValue = "100"
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "start_bill_date" : param.pValue = Today.ToString("yyyy-MM-dd")
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "total_fields" : param.pValue = "allamount"
		paramLst.Add(param)
		Dim strJson As String = clsX.GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/scm/sal_out_bound")
		TextBox1.Text = strJson
	End Sub

	Private Sub btnSalOutStkLst_Click(sender As Object, e As EventArgs) Handles btnSalOutStkLst.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim paramLst As New List(Of LrGetToken.LrParam)
		Dim param As New LrGetToken.LrParam()
		param.pKey = "id" : param.pValue = "2592071176503190528"
		paramLst.Add(param)
		Dim strJson As String = clsX.GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/scm/sal_out_bound_detail")
		TextBox1.Text = strJson
	End Sub

	Private Sub btnSalTotalRpt_Click(sender As Object, e As EventArgs) Handles btnSalTotalRpt.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim paramLst As New List(Of LrGetToken.LrParam)
		Dim param As New LrGetToken.LrParam()
		param = New LrGetToken.LrParam()
		param.pKey = "end_date" : param.pValue = Today.ToString("yyyy-MM-dd")
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "filter_summary_field" : param.pValue = "customerid"
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "page" : param.pValue = "1"
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "page_size" : param.pValue = "100"
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "start_date" : param.pValue = "2026-05-01"
		paramLst.Add(param)
		Dim strJson As String = clsX.GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/sal/sal_summary_rpt_ims")
		TextBox1.Text = strJson
	End Sub

	Private Sub btnQueryStk_Click(sender As Object, e As EventArgs) Handles btnQueryStk.Click
		Dim clsX As New LrGetToken.LrGetMtrlDatum
		Dim paramLst As New List(Of LrGetToken.LrParam)
		Dim param As New LrGetToken.LrParam()
		param = New LrGetToken.LrParam()
		param.pKey = "filter_material_id" : param.pValue = "2440145414180164608"
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "opt_group_inv_query_type" : param.pValue = "1"
		paramLst.Add(param)
		param = New LrGetToken.LrParam()
		param.pKey = "type" : param.pValue = "1"
		paramLst.Add(param)
		Dim strJson As String = clsX.GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/inv/inv_inventory_ims")
		TextBox1.Text = strJson
		Dim jo As JObject = JsonConvert.DeserializeObject(strJson)
	End Sub
End Class