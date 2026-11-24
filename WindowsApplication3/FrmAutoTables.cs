using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LrGetToken;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WindowsApplication3.Data;

namespace WindowsApplication3
{
    /// <summary>
    /// 独立实时数据看板窗口：底部 TabControl 展示「物料」「工单」两个表格。
    /// 启动后立即拉取一次，之后每 5 分钟比对缓存：数据相同则表格不动，不同才更新。
    /// 与主窗体 FrmTest 完全独立——不写 token、不入库、不弹框、不触碰主表格。
    /// 凭证（clientId / clientSecret / appToken）由 FrmTest 在获取 token 后传入。
    /// </summary>
    public partial class FrmAutoTables : Form
    {
        // ===== 金蝶 API 凭证（由主窗体传入）=====
        private readonly string _clientId;
        private readonly string _clientSecret;
        private string _appToken;   // 可由主窗体在主窗口自动续期后推送更新

        /// <summary>
        /// 主窗体续期 app-token 成功后调用，替换本窗口持有的旧 token。
        /// string 引用赋值是原子的；看板拉取在下个周期自然会用到新卡。
        /// </summary>
        public void UpdateToken(string newToken)
        {
            if (!string.IsNullOrEmpty(newToken))
                _appToken = newToken;
        }

        // ===== 定时拉取 + 缓存比对（相同不变、不同才更新）=====
        private System.Windows.Forms.Timer _tabDataTimer = null;
        private readonly Dictionary<string, string> _tabDataHashCache = new Dictionary<string, string>();
        private readonly HashSet<string> _tabDataFetching = new HashSet<string>();
        private readonly Dictionary<string, DateTime> _tabDataLastFail = new Dictionary<string, DateTime>();
        private static readonly TimeSpan TAB_DATA_INTERVAL = TimeSpan.FromMinutes(5);

        public FrmAutoTables(string clientId, string clientSecret, string appToken)
        {
            InitializeComponent();
            _clientId = clientId;
            _clientSecret = clientSecret;
            _appToken = appToken;
            this.Load += FrmAutoTables_Load;
        }

        private void FrmAutoTables_Load(object sender, EventArgs e)
        {
            StartTabDataRefresh();
        }

        /// <summary>
        /// 窗体关闭时停止定时器，避免定时器回调访问已释放的控件。
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_tabDataTimer != null)
            {
                _tabDataTimer.Stop();
                _tabDataTimer.Dispose();
                _tabDataTimer = null;
            }
            base.OnFormClosed(e);
        }

        // ==================== 定时拉取（缓存比对）====================

        private void StartTabDataRefresh()
        {
            if (_tabDataTimer != null) return;   // 防止重复启动
            _tabDataTimer = new System.Windows.Forms.Timer();
            _tabDataTimer.Interval = (int)TAB_DATA_INTERVAL.TotalMilliseconds;
            _tabDataTimer.Tick += TabDataTimer_Tick;
            _tabDataTimer.Start();
            FetchAllTabData();   // 启动即加载一次
        }

        private void TabDataTimer_Tick(object sender, EventArgs e)
        {
            FetchAllTabData();
        }

        /// <summary>并发拉取「物料」「工单」两个页签的数据，互不阻塞。</summary>
        private void FetchAllTabData()
        {
            FetchOneTabData("tab_material", "物料", dgvAutoTabMaterial, FetchMaterialForTab);
            FetchOneTabData("tab_workorder", "工单", dgvAutoTabWorkOrder, FetchWorkOrderForTab);
        }

        /// <summary>
        /// 单个页签：后台拉金蝶数据 → 计算哈希与缓存比对 →
        /// 相同则跳过（表格不动），不同则拍平并更新表格、刷新缓存。全程静默。
        /// </summary>
        private void FetchOneTabData(string key, string label, DataGridView grid, Func<string> fetchFunc)
        {
            // 并发锁 + 失败退避（失败后至少等一个刷新周期再重试）
            lock (_tabDataFetching)
            {
                if (_tabDataFetching.Contains(key)) return;
                DateTime lastFail;
                if (_tabDataLastFail.TryGetValue(key, out lastFail)
                    && (DateTime.Now - lastFail) < TAB_DATA_INTERVAL) return;
                _tabDataFetching.Add(key);
            }

            FetchOneTabDataFireForget(key, label, grid, fetchFunc);
        }

        private async void FetchOneTabDataFireForget(string key, string label, DataGridView grid, Func<string> fetchFunc)
        {
            await FetchOneTabDataAsync(key, label, grid, fetchFunc);
        }

        private async Task FetchOneTabDataAsync(string key, string label, DataGridView grid, Func<string> fetchFunc)
        {
            try
            {
                // 后台线程执行同步阻塞的金蝶请求
                string json = await Task.Run(fetchFunc);
                if (string.IsNullOrWhiteSpace(json))
                    throw new Exception("接口返回空数据");

                // 哈希比对：字段顺序/缩进不同但内容相同 => 同一哈希 => 跳过更新
                string hash = JsonFlattener.ComputeHash(JToken.Parse(json)["data"]);
                string cached;
                lock (_tabDataHashCache)
                {
                    if (_tabDataHashCache.TryGetValue(key, out cached)
                        && string.Equals(cached, hash, StringComparison.Ordinal))
                        return;   // 数据未变化：表格保持不变
                }

                // 数据有变化：拍平后更新对应页签表格
                DataTable dt = FlattenGenericJson(json);
                FillGridView(grid, dt);
                lock (_tabDataHashCache) _tabDataHashCache[key] = hash;
                lock (_tabDataLastFail) _tabDataLastFail.Remove(key);   // 成功后清除失败记录
            }
            catch
            {
                // 静默失败：记录失败时间用于退避，不弹框
                lock (_tabDataLastFail) _tabDataLastFail[key] = DateTime.Now;
            }
            finally
            {
                lock (_tabDataFetching) _tabDataFetching.Remove(key);
            }
        }

        /// <summary>物料页签取数：与「获取物料」按钮相同的接口，但不追加 DEMO 测试数据。</summary>
        private string FetchMaterialForTab()
        {
            var paramLst = new List<LrParam>();
            var p1 = new LrParam(); p1.pKey = "page"; p1.pValue = "1"; paramLst.Add(p1);
            var p2 = new LrParam(); p2.pKey = "page_size"; p2.pValue = "100"; paramLst.Add(p2);
            var p3 = new LrParam(); p3.pKey = "show_units"; p3.pValue = "true"; paramLst.Add(p3);
            return new LrGetMtrlDatum().GetDatumX(_clientId, _clientSecret, _appToken, paramLst, "jdy/v2/bd/material");
        }

        /// <summary>工单页签取数：与「生产工单」按钮相同的接口。</summary>
        private string FetchWorkOrderForTab()
        {
            var paramLst = new List<LrParam>();
            var p = new LrParam(); p.pKey = "search"; p.pValue = "SCRW-20261101-00021"; paramLst.Add(p);
            return new LrGetMtrlDatum().GetDatumX(_clientId, _clientSecret, _appToken, paramLst, "jdy/v2/pm/mo_taskbill");
        }

        // ==================== JSON 拍平 / 表格填充（从 FrmTest 迁移，窗口独立所需）====================

        /// <summary>将任意 API 返回的 JSON（含 data.rows）动态拍平为 DataTable。</summary>
        private DataTable FlattenGenericJson(string jsonStr)
        {
            DataTable dt = new DataTable();
            JObject jo = JsonConvert.DeserializeObject<JObject>(jsonStr);
            JToken dataToken = jo["data"];
            if (dataToken == null || dataToken.Type == JTokenType.Null)
            {
                dt.Columns.Add("raw_response", typeof(string));
                dt.Rows.Add(jsonStr);
                return dt;
            }

            JArray rows = null;
            if (dataToken is JArray)
            {
                rows = (JArray)dataToken;
            }
            else if (dataToken is JObject)
            {
                JObject dataObj = (JObject)dataToken;
                if (dataObj["rows"] != null && dataObj["rows"] is JArray)
                    rows = (JArray)dataObj["rows"];
                else
                    rows = new JArray { dataToken };
            }
            else
            {
                rows = new JArray { dataToken };
            }

            if (rows == null || rows.Count == 0)
            {
                dt.Columns.Add("message", typeof(string));
                dt.Rows.Add("无数据或数据格式不支持");
                return dt;
            }

            List<JObject> objRows = new List<JObject>();
            foreach (JToken item in rows)
            {
                if (item is JObject)
                    objRows.Add((JObject)item);
                else if (item is JArray)
                {
                    foreach (JToken subItem in (JArray)item)
                    {
                        if (subItem is JObject)
                            objRows.Add((JObject)subItem);
                    }
                }
            }
            if (objRows.Count == 0)
            {
                dt.Columns.Add("message", typeof(string));
                dt.Rows.Add("无有效数据行");
                return dt;
            }

            HashSet<string> allKeys = new HashSet<string>();
            foreach (JObject row in objRows)
                CollectKeys(row, "", allKeys);
            List<string> sortedKeys = allKeys.OrderBy(k => k).ToList();

            foreach (string key in sortedKeys)
                dt.Columns.Add(key, typeof(string));

            foreach (JObject row in objRows)
            {
                DataRow dr = dt.NewRow();
                foreach (string key in sortedKeys)
                    dr[key] = GetFlattenedValue(row, key);
                dt.Rows.Add(dr);
            }
            return dt;
        }

        private void CollectKeys(JObject obj, string prefix, HashSet<string> keys)
        {
            foreach (var prop in obj.Properties())
            {
                string fullKey = string.IsNullOrEmpty(prefix) ? prop.Name : prefix + "." + prop.Name;
                if (prop.Value is JObject)
                {
                    CollectKeys((JObject)prop.Value, fullKey, keys);
                }
                else if (prop.Value is JArray)
                {
                    JArray arr = (JArray)prop.Value;
                    if (arr.Count > 0 && arr[0] is JObject)
                    {
                        for (int i = 0; i < arr.Count; i++)
                        {
                            if (arr[i] is JObject)
                            {
                                string arrPrefix = fullKey + "[" + i + "]";
                                CollectKeys((JObject)arr[i], arrPrefix, keys);
                            }
                        }
                    }
                    else
                    {
                        keys.Add(fullKey);
                    }
                }
                else
                {
                    keys.Add(fullKey);
                }
            }
        }

        private string GetFlattenedValue(JObject obj, string keyPath)
        {
            string[] parts = keyPath.Split('.');
            JToken current = obj;
            foreach (string part in parts)
            {
                if (current == null) return "";
                string propName = part;
                int arrIndex = -1;
                int bracketIdx = part.IndexOf('[');
                if (bracketIdx > 0)
                {
                    propName = part.Substring(0, bracketIdx);
                    string idxStr = part.Substring(bracketIdx + 1, part.Length - bracketIdx - 2);
                    int.TryParse(idxStr, out arrIndex);
                }

                if (current is JObject)
                {
                    JObject curObj = (JObject)current;
                    JToken child;
                    if (curObj.TryGetValue(propName, out child) && child != null)
                    {
                        if (arrIndex >= 0 && child is JArray)
                        {
                            JArray arr = (JArray)child;
                            if (arrIndex < arr.Count)
                                current = arr[arrIndex];
                            else
                                return "";
                        }
                        else
                        {
                            current = child;
                        }
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }

            if (current == null) return "";
            if (current is JObject || current is JArray)
                return current.ToString(Formatting.None);
            return current.ToString();
        }

        /// <summary>把 DataTable 手动填充到指定 DataGridView（含跨线程防御）。</summary>
        private void FillGridView(DataGridView grid, DataTable dt)
        {
            if (grid == null || dt == null) return;
            // 只有句柄已创建且确实跨线程时才 BeginInvoke；窗口尚未显示时直接同步填充，
            // 避免 InvokeRequired 在句柄未创建时抛异常。
            if (grid.IsHandleCreated && grid.InvokeRequired)
            {
                grid.BeginInvoke(new Action(() => FillGridView(grid, dt)));
                return;
            }
            if (grid.DataSource != null) grid.DataSource = null;
            grid.Columns.Clear();
            grid.Rows.Clear();

            foreach (DataColumn dc in dt.Columns)
                grid.Columns.Add(dc.ColumnName, dc.ColumnName);

            foreach (DataRow dr in dt.Rows)
            {
                int rowIdx = grid.Rows.Add();
                DataGridViewRow dgvRow = grid.Rows[rowIdx];
                for (int i = 0; i < dt.Columns.Count; i++)
                    dgvRow.Cells[i].Value = dr[i];
            }
        }
    }
}
