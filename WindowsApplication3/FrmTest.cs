using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LrGetToken;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WindowsApplication3.Data;

namespace WindowsApplication3
{
    public partial class FrmTest : Form
    {
        private string strAppSecret = "";
        private string strAppToken = "";
        private string strAccessToken = "";
        private string AppKey = "KM9vdpEa";
        private string clientId = "368805";
        private string clientSecret = "858679453e5074b823dff4e759dc85e6";
        private string outerInstanceId = "604422060890001408";
        private string uId = "";

        // 保存最后获取的物料数据用于数据库写入
        private DataTable lastMaterialTable = null;

        // ===== SQL Server（EF Core）入库相关 =====
        private bool _dbReady = false;          // 数据库是否可用
        private string _lastJson = null;        // 最近一次按钮输出的 JSON
        private string _lastLabel = null;       // 最近一次按钮的中文名
        private int _dbSavedTotal = 0;          // 累计入库记录数（本次运行）

        // ===== 每按钮数据 Token（5 分钟有效期）=====
        // 每个按钮持有一个 Token，Token 在生成时抓取一份数据并与之绑定：
        //   · 5 分钟内再次点击 => 认证到同一个 Token => 只显示 Token 绑定的数据，不发起请求；
        //   · 超过 5 分钟     => Token 过期 => 生成新 Token、抓取最新数据并显示，旧 Token 与旧数据作废。
        private readonly TimeSpan _fetchCooldown = TimeSpan.FromMinutes(5);   // Token 有效期
        // 请求状态锁：同一按钮上一次请求未结束时连点，直接拦截，避免并发。
        private readonly HashSet<string> _fetchingKeys = new HashSet<string>();
        // key(按钮接口标识) -> 当前有效 Token；每个按钮各自独立，互不干扰。
        private readonly Dictionary<string, ButtonDataToken> _buttonTokens = new Dictionary<string, ButtonDataToken>();

        // ===== 金蝶 app-token 自动续期（有效期约24小时，提前换卡）=====
        private DateTime _tokenIssuedAt = DateTime.MinValue;                 // 最近一次领到 token 的时间
        private System.Windows.Forms.Timer _tokenRefreshTimer = null;        // 每30分钟检查一次是否需要续期
        private const int TOKEN_CHECK_MS = 30 * 60 * 1000;                   // 检查周期：30分钟
        private static readonly TimeSpan TOKEN_REFRESH_BEFORE = TimeSpan.FromHours(20); // 满20小时就换（留4小时余量）
        private readonly object _tokenRefreshLock = new object();            // 防止续期检查并发重叠
        private bool _tokenRefreshing = false;

        // ===== 左侧 TabControl 自动刷新（物料 / 工单，每 5 分钟比对缓存）=====
        private System.Windows.Forms.Timer _tabDataTimer = null;
        private readonly Dictionary<string, string> _tabDataHashCache = new Dictionary<string, string>();
        private readonly HashSet<string> _tabDataFetching = new HashSet<string>();
        private readonly Dictionary<string, DateTime> _tabDataLastFail = new Dictionary<string, DateTime>();
        private static readonly TimeSpan TAB_DATA_INTERVAL = TimeSpan.FromMinutes(5);

        // ===== 当前显示按钮追踪：用于 token 过期时自动刷新（无需手动点击）=====
        private System.Windows.Forms.Timer _autoRefreshTimer = null;   // 轮询检测当前按钮 token 是否过期
        private string _currentKey = null;                             // 当前表格显示的按钮 key（null=启动默认数据，不自动刷新）
        private string _currentLabel = null;                           // 当前按钮中文名
        private Func<string> _currentFetchFunc = null;                 // 当前按钮的抓取委托（自动刷新复用）
        private Action<string> _currentAfterFetch = null;              // 当前按钮的额外处理（复用）
        private readonly object _currentLock = new object();           // 保护"当前显示上下文"的锁

        // ===== 启动默认展示（仅加载一次，无定时器周期刷新）=====
        private string _realtimeEndpoint = null;                    // 启动加载查询的接口（null=全部接口）

        /// <summary>
        /// 按钮数据令牌：唯一令牌值 + 生成时间 + 生成时抓取的数据快照。
        /// 点击按钮时先认证令牌：未过期则复用其数据；过期则换新令牌并抓取新数据。
        /// </summary>
        private class ButtonDataToken
        {
            public string Token;        // 令牌值（Guid，唯一标识本次数据抓取）
            public DateTime CreatedAt;  // 令牌生成时间（5 分钟有效期的起点）
            public string DataJson;     // 令牌绑定的数据快照（认证通过时显示的就是它）
            public string Label;        // 按钮中文名
        }

        public FrmTest()
        {
            InitializeComponent();
            this.Load += FrmTest_Load;
        }

        /// <summary>
        /// 窗体关闭时停止定时器，避免回调在窗口释放后继续访问控件。
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_tabDataTimer != null) { _tabDataTimer.Stop(); _tabDataTimer.Dispose(); _tabDataTimer = null; }
            if (_tokenRefreshTimer != null) { _tokenRefreshTimer.Stop(); _tokenRefreshTimer.Dispose(); _tokenRefreshTimer = null; }
            if (_autoRefreshTimer != null) { _autoRefreshTimer.Stop(); _autoRefreshTimer.Dispose(); _autoRefreshTimer = null; }
            base.OnFormClosed(e);
        }

        /// <summary>
        /// 窗体加载：立即把控制权交还 UI 线程让窗口先显示；
        /// 数据库初始化与两次金蝶网络请求都是阻塞操作，全部移到后台线程执行，避免启动假死。
        /// 分两阶段：数据库就绪后立刻展示默认数据，凭证/token 就绪后再启动左侧物料/工单自动刷新。
        /// </summary>
        private async void FrmTest_Load(object sender, EventArgs e)
        {
            // （瞬时）挂接请求观测回调：每次真实 API 请求都记一条到 api_request_log，
            // 回答"哪台设备/用户、何时、请求了哪个接口、参数、返回条数、成败"。
            // 回调运行在后台线程，RequestLogStore 内部已加锁并用独立 DbContext，不碰 UI。
            LrGetMtrlDatum.RequestObserver = (path, paramText, json, success, msg) =>
                RequestLogStore.Write(path, EndpointMap.ResolveLabel(path), paramText, json, success, msg);

            // （UI 线程，瞬时）先启动"当前按钮 token 过期自动刷新"定时器，它与后台加载无依赖。
            StartAutoRefreshTimer();

            // 阶段A（后台）：接通 SQL Server，库/表不存在时自动创建。
            string dbMsg = "";
            try
            {
                dbMsg = await InitDatabaseAsync();
            }
            catch
            {
                _dbReady = false;
            }
            // await 之后已回到 UI 线程：更新状态栏 + 失败提示
            ApplyDbInitResult(dbMsg);
            // 数据库就绪后立刻异步加载默认数据（fire-and-forget，不等待凭证/token）
            RefreshRealtimeTable();

            // 阶段B（后台）：获取金蝶 appSecret 与 token（成功时内部会记下领卡时间）。
            try
            {
                await GetTokenAsync();
            }
            catch
            {
                // 凭证/token 获取失败时保持空 token，看板窗口内拉取会静默失败退避，不影响主界面
            }
            // 回到 UI 线程：token 已写入字段，启动左侧 TabControl 物料/工单自动刷新（就在本窗口）
            StartTabDataRefresh();
            // 启动 app-token 自动续期：无论首取是否成功都启动定时器，没拿到卡则30分钟后重试
            StartTokenRefreshTimer();
        }

        /// <summary>
        /// 后台线程：接通数据库并记录就绪状态。不触碰任何 UI 控件。
        /// </summary>
        private Task<string> InitDatabaseAsync()
        {
            return Task.Run(() =>
            {
                bool ok;
                string msg = XcApiDbContext.EnsureDatabase(out ok);
                _dbReady = ok;
                return msg;
            });
        }

        /// <summary>
        /// UI 线程：把数据库初始化结果显示到状态栏；失败时给出可操作提示。
        /// </summary>
        private void ApplyDbInitResult(string msg)
        {
            if (lblDbStatus != null)
            {
                lblDbStatus.Text = _dbReady ? "数据库：已连接\nXingChenDb" : "数据库：连接失败";
                lblDbStatus.ForeColor = _dbReady ? Color.DarkGreen : Color.Firebrick;
            }
            if (!_dbReady && !string.IsNullOrEmpty(msg))
            {
                MessageBox.Show(msg + "\n\n请确认：\n1) SQL Server 服务已启动\n2) App.config 里 XingChenDb 连接字符串正确\n\n（API查询功能仍可正常使用，只是数据不会入库）",
                    "数据库连接失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 后台线程：获取金蝶 appSecret 与 token，结果写入凭证字段。不触碰任何 UI 控件。
        /// 返回是否成功拿到 token（失败时字段保持为空，看板窗口仍会打开但拉取静默失败）。
        /// </summary>
        private Task<bool> GetTokenAsync()
        {
            return Task.Run(() =>
            {
                var clsX = new clsGetToken();
                strAppSecret = clsX.GetAppSecretB(clientId, clientSecret, AppKey, outerInstanceId);
                if (string.IsNullOrEmpty(strAppSecret)) return false;

                JObject jo = JsonConvert.DeserializeObject<JObject>(strAppSecret);
                string appSecret = null;
                if (jo != null && jo["data"] != null)
                {
                    for (int i = 0; i < jo["data"].Count(); i++)
                    {
                        if (jo["data"][i]["appKey"] != null
                            && jo["data"][i]["appKey"].ToString() == AppKey
                            && jo["data"][i]["appSecret"] != null)
                        {
                            appSecret = jo["data"][i]["appSecret"].ToString();
                            break;
                        }
                    }
                }
                if (appSecret == null) return false;
                strAppSecret = appSecret;

                string tokenResp = clsX.GetToken(clientId, clientSecret, AppKey, strAppSecret);
                if (string.IsNullOrEmpty(tokenResp)) return false;

                jo = JsonConvert.DeserializeObject<JObject>(tokenResp);
                if (jo == null || jo["data"] == null) return false;
                strAppToken = jo["data"]["app-token"].ToString();
                strAccessToken = jo["data"]["access_token"].ToString();
                uId = jo["data"]["uid"].ToString();
                _tokenIssuedAt = DateTime.Now;   // 记下领卡时间（启动与续期共用此方法）
                return true;
            });
        }

        /// <summary>
        /// 启动 app-token 续期定时器：每30分钟检查一次，卡快满20小时就自动换新卡。
        /// </summary>
        private void StartTokenRefreshTimer()
        {
            if (_tokenRefreshTimer != null) return;   // 防止重复启动
            _tokenRefreshTimer = new System.Windows.Forms.Timer();
            _tokenRefreshTimer.Interval = TOKEN_CHECK_MS;
            _tokenRefreshTimer.Tick += TokenRefreshTimer_Tick;
            _tokenRefreshTimer.Start();
        }

        /// <summary>
        /// 定时器回调：没领过卡（启动失败）或卡已满20小时 => 触发换卡；否则什么都不做。
        /// </summary>
        private void TokenRefreshTimer_Tick(object sender, EventArgs e)
        {
            lock (_tokenRefreshLock)
            {
                if (_tokenRefreshing) return;   // 上一次换卡还没结束，跳过本次检查
                bool expired = _tokenIssuedAt == DateTime.MinValue
                    || (DateTime.Now - _tokenIssuedAt) >= TOKEN_REFRESH_BEFORE;
                if (!expired) return;
                _tokenRefreshing = true;
            }
            RefreshTokenFireAndForget();
        }

        /// <summary>
        /// fire-and-forget：后台换卡。新卡直接写入本窗口的 strAppToken 字段，
        /// 左侧物料/工单取数函数读取的正是该字段，无需再推送给别的窗口。
        /// 换卡成功后清掉旧 token 期间的失败退避与哈希缓存，让下一轮立即用新卡重拉。
        /// </summary>
        private async void RefreshTokenFireAndForget()
        {
            try
            {
                bool ok = await GetTokenAsync();   // 后台换卡，内部成功时更新 _tokenIssuedAt 与凭证字段
                if (ok)
                {
                    // 新卡生效：清掉失败退避，使下个刷新周期（或手动触发）能立即重试
                    lock (_tabDataLastFail) _tabDataLastFail.Clear();
                }
            }
            catch { }
            finally
            {
                lock (_tokenRefreshLock) _tokenRefreshing = false;
            }
        }

        // ==================== 左侧 TabControl 物料/工单自动刷新（缓存比对）====================

        /// <summary>
        /// 启动左侧看板定时刷新：立即拉一次，之后每 5 分钟比对缓存——相同则表格不动，不同才更新。
        /// </summary>
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
            return new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/bd/material");
        }

        /// <summary>工单页签取数：拉最新工单列表（page/page_size），不依赖写死单号。</summary>
        private string FetchWorkOrderForTab()
        {
            var paramLst = new List<LrParam>();
            var p1 = new LrParam(); p1.pKey = "page"; p1.pValue = "1"; paramLst.Add(p1);
            var p2 = new LrParam(); p2.pKey = "page_size"; p2.pValue = "100"; paramLst.Add(p2);
            return new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/pm/mo_taskbill");
        }

        /// <summary>
        /// 将API返回的JSON拍平为DataTable
        /// </summary>
        // 列顺序即输出契约，与精简前逐字一致，勿调整
        private static readonly string[] MaterialColumns =
        {
            "material_id", "material_number", "material_name", "material_full_name",
            "specification", "model", "barcode", "material_group_id", "material_group_name",
            "material_group_number", "measure_unit_id", "measure_unit_name", "is_multi_unit",
            "primary_unit_name", "primary_unit_convert", "alt_units", "units_json",
            "aux_prop_id", "aux_prop_name", "labels", "labels_json", "custom_fields",
            "custom_field_json", "enable", "remark", "create_time", "modify_time", "row_json"
        };

        // 直映射表：目标列 -> 源字段（is_multi_unit / enable 需转换，单独处理）
        private static readonly string[][] MaterialFieldMap =
        {
            new[] { "material_id", "id" },
            new[] { "material_number", "number" },
            new[] { "material_name", "name" },
            new[] { "material_full_name", "full_name" },
            new[] { "specification", "spec" },
            new[] { "model", "model" },
            new[] { "barcode", "barcode" },
            new[] { "material_group_id", "material_group_id" },
            new[] { "material_group_name", "material_group_name" },
            new[] { "material_group_number", "material_group_number" },
            new[] { "measure_unit_id", "measure_unit_id" },
            new[] { "measure_unit_name", "measure_unit_name" },
            new[] { "aux_prop_id", "aux_prop_id" },
            new[] { "aux_prop_name", "aux_prop_name" },
            new[] { "remark", "remark" },
            new[] { "create_time", "create_time" },
            new[] { "modify_time", "modify_time" }
        };

        private DataTable FlattenMaterialJson(string jsonStr)
        {
            DataTable dt = new DataTable();
            foreach (string col in MaterialColumns)
                dt.Columns.Add(col, typeof(string));

            JObject jo = JsonConvert.DeserializeObject<JObject>(jsonStr);
            JArray rows = (JArray)jo["data"]["rows"];

            foreach (JObject row in rows)
            {
                DataRow dr = dt.NewRow();
                foreach (string[] pair in MaterialFieldMap)
                    dr[pair[0]] = ValOrEmpty(row, pair[1]);
                dr["is_multi_unit"] = ValOrEmpty(row, "is_multi_unit") == "1" ? "是" : "否";
                dr["enable"] = ValOrEmpty(row, "enable") == "1" ? "是" : "否";

                // ---------- 拍平 units ----------
                string unitsStr = ValOrEmpty(row, "units");
                dr["units_json"] = unitsStr;
                JArray unitsArr = null;
                if (!string.IsNullOrEmpty(unitsStr))
                {
                    try { unitsArr = JArray.Parse(unitsStr); } catch { }
                }
                string primaryUnit = "";
                List<string> altUnits = new List<string>();
                if (unitsArr != null)
                {
                    foreach (JObject u in unitsArr)
                    {
                        if (ValOrEmpty(u, "unit_type") == "1")
                        {
                            primaryUnit = ValOrEmpty(u, "unit_name");
                            dr["primary_unit_convert"] = ValOrEmpty(u, "unit_convert");
                        }
                        else
                        {
                            altUnits.Add(ValOrEmpty(u, "unit_name") + "(换算率:" + ValOrEmpty(u, "unit_convert") + ")");
                        }
                    }
                }
                dr["primary_unit_name"] = primaryUnit;
                dr["alt_units"] = string.Join(", ", altUnits);

                // ---------- 拍平 labels ----------
                string labelsStr = ValOrEmpty(row, "mul_label");
                dr["labels_json"] = labelsStr;
                JArray labelsArr = null;
                if (!string.IsNullOrEmpty(labelsStr))
                {
                    try { labelsArr = JArray.Parse(labelsStr); } catch { }
                }
                List<string> labelNames = new List<string>();
                if (labelsArr != null)
                {
                    foreach (JObject lb in labelsArr)
                    {
                        labelNames.Add(ValOrEmpty(lb, "label_name"));
                    }
                }
                dr["labels"] = string.Join(", ", labelNames);

                // ---------- 拍平 custom_field ----------
                string cfStr = ValOrEmpty(row, "custom_field");
                dr["custom_field_json"] = cfStr;
                JObject cfObj = null;
                if (!string.IsNullOrEmpty(cfStr))
                {
                    try { cfObj = JObject.Parse(cfStr); } catch { }
                }
                List<string> cfPairs = new List<string>();
                if (cfObj != null)
                {
                    foreach (var prop in cfObj.Properties())
                    {
                        cfPairs.Add(prop.Name + "=" + prop.Value.ToString());
                    }
                }
                dr["custom_fields"] = string.Join("; ", cfPairs);

                dr["row_json"] = row.ToString();
                dt.Rows.Add(dr);
            }
            return dt;
        }

        /// <summary>
        /// 安全获取JObject中的值
        /// </summary>
        private string ValOrEmpty(JObject jo, string key)
        {
            JToken token = null;
            if (jo.TryGetValue(key, out token) && token != null)
                return token.ToString();
            return "";
        }

        /// <summary>列名路径里是否含 name 字段（如 name / material.name / detail[0].unit.name）</summary>
        private bool ContainsName(string keyPath)
        {
            foreach (string seg in keyPath.Split('.'))
            {
                string name = seg;
                int bracket = name.IndexOf('[');
                if (bracket > 0) name = name.Substring(0, bracket);
                if (string.Equals(name, "name", StringComparison.OrdinalIgnoreCase) ||
                    name.IndexOf("name", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        /// <summary>是否为顶层的 name 列（排序时最优先）</summary>
        private bool IsExactName(string keyPath)
        {
            return string.Equals(keyPath, "name", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 将任意API返回的JSON（含data.rows）动态拍平为DataTable，自动识别所有字段并展开嵌套
        /// </summary>
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
                {
                    rows = (JArray)dataObj["rows"];
                }
                else
                {
                    // 单个对象或用其他key包裹的数据
                    rows = new JArray { dataToken };
                }
            }
            else
            {
                // JValue 等其他类型
                rows = new JArray { dataToken };
            }

            if (rows == null || rows.Count == 0)
            {
                dt.Columns.Add("message", typeof(string));
                dt.Rows.Add("无数据或数据格式不支持");
                return dt;
            }

            // 只提取JObject类型的行
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

            // 第一遍扫描：收集所有列名
            HashSet<string> allKeys = new HashSet<string>();
            foreach (JObject row in objRows)
            {
                CollectKeys(row, "", allKeys);
            }
            // 列顺序：所有含 name 的列排最前（顶层 name 优先，其余按含 name 的层级深度），其余字段按字母排序
            List<string> sortedKeys = allKeys
                .OrderBy(k => ContainsName(k) ? 0 : 1)
                .ThenBy(k => IsExactName(k) ? 0 : 1)
                .ThenBy(k => k.Count(c => c == '.'))
                .ThenBy(k => k, StringComparer.Ordinal)
                .ToList();

            // 创建列
            foreach (string key in sortedKeys)
            {
                dt.Columns.Add(key, typeof(string));
            }

            // 填充数据
            foreach (JObject row in objRows)
            {
                DataRow dr = dt.NewRow();
                foreach (string key in sortedKeys)
                {
                    dr[key] = GetFlattenedValue(row, key);
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        /// <summary>
        /// 把"字符串化的 JSON"还原成 JObject/JArray。
        /// 金蝶部分字段（如分录明细）会把对象/数组序列化成字符串返回，
        /// 若不还原，这些字段会被当成普通文本整段塞进一列，里面的 name 字段就无法单独成列。
        /// 只对以 { 或 [ 开头的字符串尝试解析，普通文本（如物料名称）原样返回，行为不变。
        /// </summary>
        private JToken ResolveJsonValue(JToken token)
        {
            if (token is JValue && token.Type == JTokenType.String)
            {
                string s = token.ToString().Trim();
                if (s.StartsWith("{") || s.StartsWith("["))
                {
                    try
                    {
                        JToken parsed = JToken.Parse(s);
                        if (parsed is JObject || parsed is JArray) return parsed;
                    }
                    catch { }
                }
            }
            return token;
        }

        /// <summary>
        /// 递归收集所有键（嵌套对象用点号连接，数组展开为序号；
        /// 字符串化的 JSON 会先还原再展开）
        /// </summary>
        private void CollectKeys(JObject obj, string prefix, HashSet<string> keys)
        {
            foreach (var prop in obj.Properties())
            {
                string fullKey = string.IsNullOrEmpty(prefix) ? prop.Name : prefix + "." + prop.Name;
                JToken value = ResolveJsonValue(prop.Value);
                if (value is JObject)
                {
                    CollectKeys((JObject)value, fullKey, keys);
                }
                else if (value is JArray)
                {
                    JArray arr = (JArray)value;
                    // 数组中只要有对象元素就逐个展开（兼容元素是字符串化 JSON 的情况）
                    bool hasObjectItem = false;
                    foreach (JToken item in arr)
                    {
                        if (ResolveJsonValue(item) is JObject) { hasObjectItem = true; break; }
                    }
                    if (hasObjectItem)
                    {
                        for (int i = 0; i < arr.Count; i++)
                        {
                            JToken item = ResolveJsonValue(arr[i]);
                            if (item is JObject)
                                CollectKeys((JObject)item, fullKey + "[" + i + "]", keys);
                            else
                                keys.Add(fullKey + "[" + i + "]");
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

        /// <summary>
        /// 根据带点号的键路径获取拍平后的值
        /// </summary>
        private string GetFlattenedValue(JObject obj, string keyPath)
        {
            string[] parts = keyPath.Split('.');
            JToken current = obj;
            foreach (string part in parts)
            {
                if (current == null) return "";
                // 处理数组索引，如 "units[0]"
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
                        // 与 CollectKeys 对应：字符串化的 JSON 先还原，才能继续按路径深入取值
                        child = ResolveJsonValue(child);
                        if (arrIndex >= 0 && child is JArray)
                        {
                            JArray arr = (JArray)child;
                            if (arrIndex < arr.Count)
                                current = ResolveJsonValue(arr[arrIndex]);
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

        /// <summary>
        /// 将任意DataTable动态显示到DataGridView（列自适应）
        /// </summary>
        private void PopulateGenericGridView(DataTable dt)
        {
            FillGridView(dgvMaterials, dt);
        }

        /// <summary>
        /// 把 DataTable 填充到指定 DataGridView（主表格与 TabControl 自动刷新表格共用）。
        /// 采用手动填行 + SuspendLayout：先解除任何 DataSource 绑定回到非绑定模式，
        /// 再用一次 Rows.Add(object[]) 批量加行，避免 DataSource 绑定在部分环境下不渲染的问题。
        /// 列宽只按表头估算，去掉 AutoResizeColumns 逐格测量造成的严重卡顿。
        /// </summary>
        private void FillGridView(DataGridView grid, DataTable dt)
        {
            if (grid == null || dt == null) return;
            // 只有句柄已创建且确实跨线程时才 BeginInvoke；窗口未显示时直接同步填充，
            // 避免 InvokeRequired 在句柄未创建时抛异常。
            if (grid.IsHandleCreated && grid.InvokeRequired)
            {
                grid.BeginInvoke(new Action(() => FillGridView(grid, dt)));
                return;
            }

            // 关键防御：DataGridView 一旦处于 DataSource 绑定模式，Rows.Add 会抛
            // "不能将行添加到已绑定数据的 DataGridView"，且绑定模式下本环境可能不自动渲染。
            // 先清空绑定，回到手动填充模式。
            grid.SuspendLayout();
            try
            {
                if (grid.DataSource != null) grid.DataSource = null;
                grid.Columns.Clear();
                grid.Rows.Clear();

                // 一次性创建列
                foreach (DataColumn dc in dt.Columns)
                    grid.Columns.Add(dc.ColumnName, dc.ColumnName);

                // 批量加行（限制最多显示 500 行，避免超大数据卡 UI；完整数据仍在 _lastJson/数据库）
                const int MAX_ROWS = 500;
                int rowCount = Math.Min(dt.Rows.Count, MAX_ROWS);
                int colCount = dt.Columns.Count;
                object[] values = new object[colCount];
                for (int r = 0; r < rowCount; r++)
                {
                    DataRow dr = dt.Rows[r];
                    for (int c = 0; c < colCount; c++)
                        values[c] = dr[c];
                    grid.Rows.Add(values);
                }

                // 列宽只按"表头文字"估算（O(列数)），不再逐格测量（AutoResizeColumns 是主要卡顿源）
                foreach (DataGridViewColumn col in grid.Columns)
                {
                    int w = (col.HeaderText ?? "").Length * 12 + 24;
                    col.Width = Math.Max(72, Math.Min(w, 180));
                }
                if (grid.Rows.Count > 0 && grid.Columns.Count > 0)
                {
                    try { grid.CurrentCell = grid.Rows[0].Cells[0]; } catch { }
                }
            }
            finally
            {
                grid.ResumeLayout();
            }
        }

        /// <summary>
        /// 通用按钮处理：调用API → 拍平JSON → 显示表格 → 更新标题
        /// </summary>
        private void HandleApiResponse(string strJson, string endpointLabel)
        {
            // 性能：大 JSON（几十万字符）整段塞进 TextBox 会明显卡 UI；界面只显示前若干字符，
            // 完整原文仍保存在 _lastJson 供入库使用，不影响功能。
            const int MAX_TEXTBOX_CHARS = 20000;
            if (!string.IsNullOrEmpty(strJson) && strJson.Length > MAX_TEXTBOX_CHARS)
                TextBox1.Text = strJson.Substring(0, MAX_TEXTBOX_CHARS)
                    + "\n…（共 " + strJson.Length + " 字符，此处仅显示前 " + MAX_TEXTBOX_CHARS + "；完整数据不受影响）";
            else
                TextBox1.Text = strJson;
            _lastJson = strJson;
            _lastLabel = endpointLabel;
            try
            {
                DataTable dt = FlattenGenericJson(strJson);
                PopulateGenericGridView(dt);
                string total = "0";
                try
                {
                    JObject jo = JsonConvert.DeserializeObject<JObject>(strJson);
                    JToken dataToken = jo["data"];
                    if (dataToken != null && dataToken.Type != JTokenType.Null)
                    {
                        if (dataToken is JArray)
                            total = ((JArray)dataToken).Count.ToString();
                        else if (dataToken is JObject)
                        {
                            JObject dataObj = (JObject)dataToken;
                            if (dataObj["total"] != null)
                                total = dataObj["total"].ToString();
                            else if (dataObj["count"] != null)
                                total = dataObj["count"].ToString();
                        }
                    }
                }
                catch { }
                this.Text = "星辰API调用 - " + endpointLabel + " [共" + total + "条]";
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据解析失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // 点击按钮输出的JSON自动保存到 SQL Server（相同数据不会重复入库）
            if (chkAutoSave != null && chkAutoSave.Checked)
            {
                SaveJsonToDatabase(false);
            }
        }

        // ==================== SQL Server（EF Core）动态入库 ====================

        /// <summary>
        /// 把最近一次按钮输出的JSON保存到 SQL Server：
        /// 相同数据只存一次，不同数据先比对差异再更新，并写入变更历史与同步日志。
        /// </summary>
        private SaveResult SaveJsonToDatabase(bool showDialog)
        {
            if (!_dbReady)
            {
                if (showDialog)
                    MessageBox.Show("数据库未连接，无法保存。\n请点击【测试数据库】查看具体原因。", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
            if (string.IsNullOrWhiteSpace(_lastJson))
            {
                if (showDialog)
                    MessageBox.Show("当前没有可保存的数据，请先点击任一查询按钮获取JSON。", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                string endpoint = EndpointMap.ResolvePath(_lastLabel);
                DynamicJsonStore store = new DynamicJsonStore();
                SaveResult r = store.Save(endpoint, _lastLabel, _lastJson);

                _dbSavedTotal += r.Inserted + r.Updated;
                // 三态提示：数据一致（无新增无更新）/ 数据有变化（已成功写入）/ 真正异常（catch 分支）
                bool hasChange = (r.Inserted > 0 || r.Updated > 0);
                if (lblDbStatus != null)
                {
                    if (hasChange)
                    {
                        // 数据不一致且已成功写入：绿色“数据库更新成功”
                        lblDbStatus.Text = string.Format("数据库更新成功\n本次{0}\n累计入库{1}条", r.Summary, _dbSavedTotal);
                        lblDbStatus.ForeColor = Color.DarkGreen;
                    }
                    else
                    {
                        // 数据完全一致：非红色提示“未新增数据”，并显式重置颜色，避免历史失败红字残留
                        lblDbStatus.Text = string.Format("数据库未新增数据\n本次共{0}条，数据无变化\n累计入库{1}条", r.Total, _dbSavedTotal);
                        lblDbStatus.ForeColor = Color.DarkGreen;
                    }
                }
                if (showDialog)
                {
                    if (hasChange)
                    {
                        MessageBox.Show("数据库更新成功（SQL Server：XingChenDb）\n接口：" + endpoint +
                            "\n" + r.Summary +
                            "\n\n表：api_record / api_record_field / api_record_change / api_sync_log",
                            "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("数据库未新增数据\n本次共" + r.Total + "条，数据无变化，无需更新数据库。",
                            "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                return r;
            }
            catch (Exception ex)
            {
                // 展开完整异常链（含内层异常），把真实原因暴露出来，避免"入库失败"四个字掩盖问题
                string detail = ex.Message;
                Exception innerEx = ex.InnerException;
                int innerDepth = 0;
                while (innerEx != null && innerDepth < 3)
                {
                    detail += " -> " + innerEx.Message;
                    innerEx = innerEx.InnerException;
                    innerDepth++;
                }
                // 表结构过时是最常见的根因，给出可操作的提示
                if (detail.IndexOf("column", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    detail.IndexOf("字段", StringComparison.Ordinal) >= 0)
                {
                    detail += "【提示：数据库表结构可能与当前代码不一致，可在 SQL Server 中删除 XingChenDb 库后重启程序自动重建】";
                }
                if (lblDbStatus != null)
                {
                    string uiDetail = detail.Length > 160 ? detail.Substring(0, 160) : detail;
                    lblDbStatus.Text = "数据库：入库失败\n" + uiDetail;
                    lblDbStatus.ForeColor = Color.Firebrick;
                }
                // 自动入库（showDialog=false）时也让异常可见：完整写入日志文件
                try
                {
                    string logPath = Path.Combine(Application.StartupPath, "db_error.log");
                    File.AppendAllText(logPath,
                        "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] 入库失败\n" + ex.ToString() + "\n\n",
                        Encoding.UTF8);
                }
                catch { }
                if (showDialog)
                {
                    MessageBox.Show("入库失败：" + detail, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    Trace.WriteLine("入库失败：" + detail);
                }
                return null;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// 在“获取物料”接口返回的JSON数据后追加几条动态测试物料：
        /// 编号带毫秒时间戳+序号，保证每次点击都与库中已有数据不同（哈希不同），
        /// 从而保存到数据库时被识别为“新增”，可在数据库中查询到本次新增的记录。
        /// 若原JSON无 data.rows 结构，则构造一个仅含测试行的标准结构，不破坏显示与入库链路。
        /// </summary>
        private static string AppendDemoMaterialRows(string json, int count)
        {
            JObject root;
            try
            {
                root = string.IsNullOrWhiteSpace(json)
                    ? new JObject()
                    : JObject.Parse(json);
            }
            catch
            {
                root = new JObject();
            }

            JObject data = root["data"] as JObject;
            if (data == null)
            {
                data = new JObject();
                root["data"] = data;
            }
            JArray rows = data["rows"] as JArray;
            if (rows == null)
            {
                rows = new JArray();
                data["rows"] = rows;
            }

            // 毫秒时间戳 + 序号，保证每次追加的数据编号唯一，哈希必然与历史数据不同
            string stamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            for (int i = 1; i <= count; i++)
            {
                string key = "DEMO-" + stamp + "-" + i.ToString();
                JObject row = new JObject();
                row["id"] = key;
                row["number"] = key;
                row["name"] = "新增测试物料-" + stamp + "-" + i.ToString();
                row["full_name"] = "新增测试物料-" + stamp + "-" + i.ToString() + "（获取物料动态追加）";
                row["spec"] = "DEMO-SPEC-" + i.ToString();
                row["measure_unit_name"] = "个";
                row["enable"] = "1";
                row["material_group_name"] = "动态测试分类";
                row["remark"] = "由【获取物料】动态追加，用于验证新增数据入库与查询";
                rows.Add(row);
            }

            if (root["code"] == null) root["code"] = 0;
            JToken oldTotal = data["total"];
            int newTotal;
            if (oldTotal != null && int.TryParse(oldTotal.ToString(), out newTotal))
            {
                data["total"] = newTotal + count;
            }
            else
            {
                data["total"] = rows.Count;
            }

            return root.ToString(Newtonsoft.Json.Formatting.None);
        }

        /// <summary>
        /// 构造内置的标准物料测试数据，结构与“物料管理”接口返回一致，
        /// 便于 100% 复用真实入库链路（同一 endpoint、同一 Save 方法）。
        /// </summary>
        private static string BuildTestMaterialJson()
        {
            JObject row = new JObject();
            row["id"] = "TEST-ITEM-001";
            row["number"] = "TEST-ITEM-001";
            row["name"] = "测试数据-001";
            row["full_name"] = "测试数据-001（标准测试物料）";
            row["spec"] = "TEST-SPEC-10x10";
            row["measure_unit_id"] = "TEST-UNIT-001";
            row["measure_unit_name"] = "个";
            row["enable"] = "1";
            row["material_group_name"] = "测试物料分类";
            row["remark"] = "测试入库内置数据，用于验证数据库写入+读取全链路";

            JArray rows = new JArray();
            rows.Add(row);

            JObject data = new JObject();
            data["total"] = 1;
            data["rows"] = rows;

            JObject root = new JObject();
            root["code"] = 0;
            root["data"] = data;
            return root.ToString(Newtonsoft.Json.Formatting.None);
        }

        /// <summary>
        /// 把从数据库读回的记录转换成DataTable，供 PopulateGenericGridView 显示
        /// </summary>
        private DataTable BuildReadbackTable(List<ApiRecord> records)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Id", typeof(string));
            dt.Columns.Add("接口", typeof(string));
            dt.Columns.Add("业务键", typeof(string));
            dt.Columns.Add("标题", typeof(string));
            dt.Columns.Add("字段数", typeof(string));
            dt.Columns.Add("同步次数", typeof(string));
            dt.Columns.Add("更新时间", typeof(string));

            foreach (ApiRecord r in records)
            {
                DataRow dr = dt.NewRow();
                dr["Id"] = r.Id.ToString();
                dr["接口"] = string.IsNullOrEmpty(r.EndpointName) ? (r.Endpoint ?? "") : r.EndpointName;
                dr["业务键"] = r.BizKey ?? "";
                dr["标题"] = r.Title ?? "";
                dr["字段数"] = r.FieldCount.ToString();
                dr["同步次数"] = r.SyncCount.ToString();
                dr["更新时间"] = r.LastSyncTime.ToString("yyyy-MM-dd HH:mm:ss");
                dt.Rows.Add(dr);
            }
            return dt;
        }

        /// <summary>
        /// 测试入库按钮：内置标准物料数据 → 走真实入库链路写入SQL Server →
        /// 立即从数据库读回同一条记录并显示，验证写入+读取全链路是否正常。
        /// </summary>
        private void btnTestInsert_Click(object sender, EventArgs e)
        {
            const string testLabel = "物料管理";
            const string testKey = "TEST-ITEM-001";

            if (!_dbReady)
            {
                MessageBox.Show("数据库未连接，无法执行测试入库。\n请点击【测试数据库】查看具体原因。",
                    "测试入库", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 【关键修复】测试入库不得破坏“获取物料 → 保存到数据库”的共享数据上下文。
            // 先暂存 _lastJson/_lastLabel，测试数据仅在本次写入期间临时使用，
            // 结束后无条件恢复，保证之后点“保存到数据库”仍保存获取物料得到的全部数据（含新增行）。
            string prevJson = _lastJson;
            string prevLabel = _lastLabel;
            string endpoint = EndpointMap.ResolvePath(testLabel);

            // ① 写入：临时切换到测试数据，复用与“获取物料”完全相同的入库链路
            SaveResult saved;
            try
            {
                _lastJson = BuildTestMaterialJson();
                _lastLabel = testLabel;
                // showDialog=false：写入结果在最后与读回结果合并展示，避免连续弹窗
                saved = SaveJsonToDatabase(false);
            }
            finally
            {
                // 无论成功或失败都恢复，避免测试数据污染“保存到数据库”的数据源
                _lastJson = prevJson;
                _lastLabel = prevLabel;
            }
            if (saved == null || !saved.Success)
            {
                MessageBox.Show("测试数据写入数据库失败。\n请查看界面下方数据库状态提示（红字），或 bin\\Debug\\db_error.log。",
                    "测试入库失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ② 读取：读回该接口的全部记录（含“获取物料”新增的行与本次测试数据），
            //    不再只按 TEST-ITEM-001 过滤，也【不覆盖】主界面表格/文本框/标题。
            List<ApiRecord> records;
            try
            {
                records = new DynamicJsonStore().QueryRecords(endpoint, "", 50);
            }
            catch (Exception ex)
            {
                MessageBox.Show("测试数据写入成功，但从数据库读回失败：\n" + ex.Message,
                    "读取失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ③ 展示：结果只在弹窗呈现；主界面表格继续显示“获取物料”的全部数据（含新增行）。
            string detail = "测试入库完成（写入 → 读取闭环验证）\n\n"
                + "① 测试数据写入结果：" + saved.Summary + "\n"
                + "② 数据库中【" + endpoint + "】接口当前共 " + records.Count + " 条记录"
                + "（含“获取物料”新增的数据与本次测试数据）。\n"
                + "③ 主界面表格保持“获取物料”的全部数据不变；"
                + "点【保存到数据库】仍会保存这些数据的全部新增行。\n\n"
                + "   本次测试业务键：" + testKey + "\n";
            if (records.Count > 0)
            {
                detail += "   最近一条业务键：" + records[0].BizKey + "\n"
                        + "   最近一条标题：" + (records[0].Title ?? "") + "\n";
            }
            MessageBox.Show(detail, "测试入库验证", MessageBoxButtons.OK,
                records.Count > 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private void btnTestDb_Click(object sender, EventArgs e)
        {
            string msg = DynamicJsonStore.TestConnection();
            _dbReady = msg.StartsWith("连接成功");
            if (lblDbStatus != null)
            {
                lblDbStatus.Text = _dbReady ? "数据库：已连接\nXingChenDb" : "数据库：连接失败";
                lblDbStatus.ForeColor = _dbReady ? Color.DarkGreen : Color.Firebrick;
            }
            MessageBox.Show(msg, "SQL Server 连接测试", MessageBoxButtons.OK,
                _dbReady ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }

        // ==================== 数据库操作（DataSet + XML 持久化，旧实现，已被SQL Server取代）====================

        private string GetDbPath()
        {
            return Path.Combine(Application.StartupPath, "MaterialsData.xml");
        }

        private DataSet LoadDatabase()
        {
            DataSet ds = new DataSet("MaterialsDB");
            string dbPath = GetDbPath();

            if (File.Exists(dbPath))
            {
                ds.ReadXml(dbPath);
                return ds;
            }

            // 创建物料主表
            DataTable dtMat = new DataTable("materials");
            dtMat.Columns.Add("material_id", typeof(string));
            dtMat.Columns.Add("material_number", typeof(string));
            dtMat.Columns.Add("material_name", typeof(string));
            dtMat.Columns.Add("material_full_name", typeof(string));
            dtMat.Columns.Add("specification", typeof(string));
            dtMat.Columns.Add("model", typeof(string));
            dtMat.Columns.Add("barcode", typeof(string));
            dtMat.Columns.Add("material_group_id", typeof(string));
            dtMat.Columns.Add("material_group_name", typeof(string));
            dtMat.Columns.Add("material_group_number", typeof(string));
            dtMat.Columns.Add("measure_unit_id", typeof(string));
            dtMat.Columns.Add("measure_unit_name", typeof(string));
            dtMat.Columns.Add("is_multi_unit", typeof(string));
            dtMat.Columns.Add("primary_unit_name", typeof(string));
            dtMat.Columns.Add("primary_unit_convert", typeof(string));
            dtMat.Columns.Add("alt_units", typeof(string));
            dtMat.Columns.Add("aux_prop_id", typeof(string));
            dtMat.Columns.Add("aux_prop_name", typeof(string));
            dtMat.Columns.Add("labels", typeof(string));
            dtMat.Columns.Add("custom_fields", typeof(string));
            dtMat.Columns.Add("enable", typeof(string));
            dtMat.Columns.Add("remark", typeof(string));
            dtMat.Columns.Add("create_time", typeof(string));
            dtMat.Columns.Add("modify_time", typeof(string));
            dtMat.Columns.Add("units_json", typeof(string));
            dtMat.Columns.Add("labels_json", typeof(string));
            dtMat.Columns.Add("custom_field_json", typeof(string));
            dtMat.Columns.Add("row_json", typeof(string));
            dtMat.Columns.Add("sync_time", typeof(string));
            dtMat.PrimaryKey = new DataColumn[] { dtMat.Columns["material_id"] };
            ds.Tables.Add(dtMat);

            // 单位明细表
            DataTable dtUnits = new DataTable("material_units");
            dtUnits.Columns.Add("id", typeof(int));
            dtUnits.Columns.Add("material_id", typeof(string));
            dtUnits.Columns.Add("unit_id", typeof(string));
            dtUnits.Columns.Add("unit_name", typeof(string));
            dtUnits.Columns.Add("unit_type", typeof(string));
            dtUnits.Columns.Add("unit_convert", typeof(string));
            dtUnits.Columns["id"].AutoIncrement = true;
            dtUnits.Columns["id"].AutoIncrementSeed = 1;
            dtUnits.Columns["id"].AutoIncrementStep = 1;
            dtUnits.PrimaryKey = new DataColumn[] { dtUnits.Columns["id"] };
            ds.Tables.Add(dtUnits);

            // 标签表
            DataTable dtLabels = new DataTable("material_labels");
            dtLabels.Columns.Add("id", typeof(int));
            dtLabels.Columns.Add("material_id", typeof(string));
            dtLabels.Columns.Add("label_id", typeof(string));
            dtLabels.Columns.Add("label_name", typeof(string));
            dtLabels.Columns["id"].AutoIncrement = true;
            dtLabels.Columns["id"].AutoIncrementSeed = 1;
            dtLabels.Columns["id"].AutoIncrementStep = 1;
            dtLabels.PrimaryKey = new DataColumn[] { dtLabels.Columns["id"] };
            ds.Tables.Add(dtLabels);

            // 同步日志表
            DataTable dtLog = new DataTable("sync_log");
            dtLog.Columns.Add("id", typeof(int));
            dtLog.Columns.Add("sync_time", typeof(string));
            dtLog.Columns.Add("record_count", typeof(int));
            dtLog.Columns.Add("status", typeof(string));
            dtLog.Columns.Add("error_msg", typeof(string));
            dtLog.Columns.Add("api_endpoint", typeof(string));
            dtLog.Columns["id"].AutoIncrement = true;
            dtLog.Columns["id"].AutoIncrementSeed = 1;
            dtLog.Columns["id"].AutoIncrementStep = 1;
            dtLog.PrimaryKey = new DataColumn[] { dtLog.Columns["id"] };
            ds.Tables.Add(dtLog);

            // 外键关系
            ds.Relations.Add("FK_material_units", dtMat.Columns["material_id"], dtUnits.Columns["material_id"]);
            ds.Relations.Add("FK_material_labels", dtMat.Columns["material_id"], dtLabels.Columns["material_id"]);

            ds.AcceptChanges();
            ds.WriteXml(dbPath, XmlWriteMode.WriteSchema);
            return ds;
        }

        // ==================== 实时数据获取：5 分钟防抖通用入口 ====================

        /// <summary>
        /// 带 5 分钟数据令牌的获取入口。所有"获取类"按钮共用这套 Token 认证流程：
        ///   1) 同一按钮上一次请求未结束 -> 静默拦截连点，避免并发；
        ///   2) 认证 Token：若该按钮已有未过期 Token -> "Token 相同"，直接用它绑定的数据
        ///      重新渲染表格（不发请求），这样"点A→点B→点回A"时 A 仍显示自己的数据；
        ///   3) Token 过期或不存在 -> 生成新 Token，到后台线程抓取一次最新数据；
        ///   4) 抓取成功 -> 新 Token 绑定新数据并替换旧 Token（旧的 Token 与数据一并作废）；
        ///      抓取失败 -> 不生成新 Token，旧 Token 保持，允许下次立即重试。
        /// </summary>
        /// <param name="key">令牌键，每个按钮用各自接口标识，互不干扰</param>
        /// <param name="label">中文名，传给 HandleApiResponse 显示</param>
        /// <param name="fetchFunc">真正发起 API 请求并返回 JSON 的委托（在后台线程执行）</param>
        /// <param name="afterFetch">拿到数据后在 UI 线程的额外处理（如物料追加 DEMO 行），可选</param>
        private async Task<bool> FetchWithThrottleAsync(
            string key,
            string label,
            Func<string> fetchFunc,
            Action<string> afterFetch = null,
            bool silent = false,              // 自动刷新时为 true：不弹框打扰用户
            string guardCurrentKey = null)    // 自动刷新时传入本按钮 key：若期间用户已切到别的按钮，则只更新数据不覆盖表格
        {
            // 1) 并发锁：同一按钮请求进行中，连点直接丢弃
            lock (_fetchingKeys)
            {
                if (_fetchingKeys.Contains(key)) return false;
                _fetchingKeys.Add(key);
            }

            // 渲染守卫：自动刷新（guardCurrentKey 非空）时，只有当前表格仍显示该按钮才允许渲染
            Func<bool> canRender = () => string.IsNullOrEmpty(guardCurrentKey) || IsCurrentButton(guardCurrentKey);

            try
            {
                // 2) 认证 Token：取出该按钮当前令牌，判断是否仍在 5 分钟有效期内
                ButtonDataToken current;
                bool hasValidToken = _buttonTokens.TryGetValue(key, out current)
                    && current != null
                    && (DateTime.Now - current.CreatedAt) < _fetchCooldown
                    && !string.IsNullOrWhiteSpace(current.DataJson);

                if (hasValidToken)
                {
                    // Token 相同（未过期）：只认证、不请求，直接显示该 Token 绑定的数据
                    if (canRender())
                    {
                        HandleApiResponse(current.DataJson, label);
                        if (afterFetch != null) afterFetch(current.DataJson);
                        RememberCurrentButton(key, label, fetchFunc, afterFetch);
                    }
                    return false;
                }

                // 3) Token 过期或不存在：生成新 Token，到后台线程抓取一次最新数据
                if (!silent) Cursor = Cursors.WaitCursor;
                string strJson = await Task.Run(fetchFunc);

                // 4) 抓取成功：新 Token（新值 + 当前时刻）绑定新数据，替换旧 Token —— 旧 Token 与旧数据作废
                if (!string.IsNullOrWhiteSpace(strJson))
                {
                    ButtonDataToken newToken = new ButtonDataToken
                    {
                        Token = Guid.NewGuid().ToString("N"),
                        CreatedAt = DateTime.Now,
                        DataJson = strJson,
                        Label = label
                    };
                    lock (_buttonTokens) _buttonTokens[key] = newToken;

                    if (canRender())
                    {
                        HandleApiResponse(strJson, label);
                        // afterFetch 在 HandleApiResponse 之后：如"网络互斥"需先展示 JSON，再提取 ID 覆盖 TextBox1
                        if (afterFetch != null) afterFetch(strJson);
                        RememberCurrentButton(key, label, fetchFunc, afterFetch);
                    }
                    return true;
                }
                else
                {
                    // 抓取失败：不生成新 Token，旧 Token 保持（下次点击可立即重试）
                    if (!silent)
                        MessageBox.Show("未获取到数据，本次不更新令牌。", "提示",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (!silent)
                    MessageBox.Show("获取实时数据失败: " + ex.Message, "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                lock (_fetchingKeys) _fetchingKeys.Remove(key);
                if (!silent) Cursor = Cursors.Default;
            }
        }

        /// <summary>当前表格是否仍显示指定按钮（自动刷新的渲染守卫）。</summary>
        private bool IsCurrentButton(string key)
        {
            lock (_currentLock) return _currentKey == key;
        }

        /// <summary>记录"当前表格显示的是哪个按钮"，供 token 过期时自动刷新复用其抓取委托。</summary>
        private void RememberCurrentButton(string key, string label, Func<string> fetchFunc, Action<string> afterFetch)
        {
            lock (_currentLock)
            {
                _currentKey = key;
                _currentLabel = label;
                _currentFetchFunc = fetchFunc;
                _currentAfterFetch = afterFetch;
            }
        }

        // ===== 自动刷新：当前按钮 token 满 5 分钟失效时，自动重抓并刷新表格（无需手动点击）=====
        private const int AUTO_REFRESH_CHECK_MS = 15000;             // 每 15 秒检查一次当前按钮 token 是否过期
        private DateTime _lastAutoRefreshFail = DateTime.MinValue;   // 上次自动刷新失败时间（失败后 1 分钟内不重试，避免持续请求）

        /// <summary>启动自动刷新定时器：只在表格显示的是某按钮数据（有 token）时才检查。</summary>
        private void StartAutoRefreshTimer()
        {
            if (_autoRefreshTimer != null) return;   // 防止重复启动
            _autoRefreshTimer = new System.Windows.Forms.Timer();
            _autoRefreshTimer.Interval = AUTO_REFRESH_CHECK_MS;
            _autoRefreshTimer.Tick += AutoRefreshTimer_Tick;
            _autoRefreshTimer.Start();
        }

        /// <summary>
        /// 定时器回调：若当前按钮的 token 已满 5 分钟失效，则静默重新抓取一次最新数据并刷新表格。
        /// 只作用于"当前正在显示的按钮"，不会覆盖用户已切换到的其他按钮。
        /// </summary>
        private void AutoRefreshTimer_Tick(object sender, EventArgs e)
        {
            // 失败退避：上次自动刷新失败后 1 分钟内不再重试
            if ((DateTime.Now - _lastAutoRefreshFail) < TimeSpan.FromMinutes(1)) return;

            string key, label;
            Func<string> fetch;
            Action<string> after;
            lock (_currentLock)
            {
                // 当前显示的是启动默认数据 / 尚未点击任何按钮 => 不自动刷新
                if (string.IsNullOrEmpty(_currentKey) || _currentFetchFunc == null) return;
                key = _currentKey;
                label = _currentLabel;
                fetch = _currentFetchFunc;
                after = _currentAfterFetch;
            }

            // 检查该按钮 token 是否仍在有效期内；未满 5 分钟则什么都不做
            ButtonDataToken token;
            lock (_buttonTokens)
            {
                if (!_buttonTokens.TryGetValue(key, out token) || token == null
                    || string.IsNullOrEmpty(token.DataJson)) return;
                if ((DateTime.Now - token.CreatedAt) < _fetchCooldown) return;
            }

            // token 已过期：异步静默重抓（silent 不弹框；guardCurrentKey 防止覆盖用户已切换的按钮）
            AutoRefreshCurrentAsync(key, label, fetch, after);
        }

        /// <summary>fire-and-forget 包装：异步执行当前按钮自动刷新，记录失败时间用于退避。</summary>
        private async void AutoRefreshCurrentAsync(string key, string label, Func<string> fetch, Action<string> after)
        {
            bool ok = await FetchWithThrottleAsync(key, label, fetch, after,
                silent: true, guardCurrentKey: key);
            if (!ok) _lastAutoRefreshFail = DateTime.Now;
        }

        /// <summary>
        /// fire-and-forget 包装：启动时异步加载数据库实时数据作为默认展示，不阻塞调用方。
        /// 注意：这里只加载一次，不做周期刷新——5 分钟 TTL 由各按钮点击时按缓存判断。
        /// </summary>
        private async void RefreshRealtimeTable()
        {
            await RefreshRealtimeTableAsync();
        }

        /// <summary>
        /// 启动时默认展示：AsNoTracking() 异步查询数据库最新 N 条 → 统一走 PopulateGenericGridView 填充表格。
        /// 纯静默：不弹框、不写 _lastJson/_lastLabel、不写 _buttonTokens、不触发自动入库；
        /// 因此不会影响各按钮自己的 5 分钟 TTL 缓存与"点哪个显示哪个"的逻辑。
        /// </summary>
        private async Task<bool> RefreshRealtimeTableAsync()
        {
            const string key = "db_realtime";

            lock (_fetchingKeys)
            {
                if (_fetchingKeys.Contains(key)) return false;
                _fetchingKeys.Add(key);
            }

            try
            {
                // 真异步只读查询：AsNoTracking() + ToListAsync()
                List<ApiRecord> records = await new DynamicJsonStore().QueryRecordsAsync(_realtimeEndpoint, 50);

                // 用与 API 按钮相同的方式填充表格（不用 DataSource），保证点击任意按钮后表格持续可见
                PopulateGenericGridView(BuildReadbackTable(records));
                string scope = string.IsNullOrEmpty(_realtimeEndpoint) ? "全部接口" : _realtimeEndpoint;
                this.Text = "星辰API调用 - 数据库实时数据(" + scope + ") [最新" + records.Count + "条]";
                return true;
            }
            catch
            {
                // 静默失败：启动默认加载不应弹框打扰用户（数据库未连接时保持现有表格不变）
                return false;
            }
            finally
            {
                lock (_fetchingKeys) _fetchingKeys.Remove(key);
            }
        }

        // ==================== 按钮事件 ====================

        private async void btnGetMtrl_Click(object sender, EventArgs e)
        {
            var paramLst = new List<LrParam>();
            var param = new LrParam();
            param.pKey = "page"; param.pValue = "1";
            paramLst.Add(param);
            param = new LrParam();
            param.pKey = "page_size"; param.pValue = "100";
            paramLst.Add(param);
            param = new LrParam();
            param.pKey = "show_units"; param.pValue = "true";
            paramLst.Add(param);

            // fetchFunc 在后台线程：取数 + 追加动态 DEMO 行
            await FetchWithThrottleAsync("material", "物料管理",
                () =>
                {
                    string json = new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/bd/material");
                    // 追加几条动态测试数据，便于验证"获取物料→保存到数据库→数据库查询"全链路。
                    // 每条编号由毫秒时间戳+序号组成，保证唯一 => 每次保存都会被识别为新增(Inserted)。
                    return AppendDemoMaterialRows(json, 3);
                },
                json => { lastMaterialTable = FlattenMaterialJson(json); });
        }

        private void btnSaveToDb_Click(object sender, EventArgs e)
        {
            // 保存到 SQL Server（EF Core 动态入库：相同数据仅存一次，不同数据先比对再更新）
            SaveJsonToDatabase(true);
        }

        private async void btnGetWh_Click(object sender, EventArgs e)
        {
            await FetchWithThrottleAsync("store", "仓库",
                () => new LrGetMtrlDatum().GetDatum(clientId, clientSecret, AppKey, strAppSecret, strAppToken, "", "", "jdy/v2/bd/store", "1", "100"));
        }

        private async void btnGetDept_Click(object sender, EventArgs e)
        {
            await FetchWithThrottleAsync("department", "部门",
                () => new LrGetMtrlDatum().GetDatum(clientId, clientSecret, AppKey, strAppSecret, strAppToken, "", "", "jdy/v2/bd/department", "1", "100"));
        }

        private async void btnGetEmp_Click(object sender, EventArgs e)
        {
            await FetchWithThrottleAsync("emp", "人员",
                () => new LrGetMtrlDatum().GetDatum(clientId, clientSecret, AppKey, strAppSecret, strAppToken, "", "", "jdy/v2/bd/emp", "1", "100"));
        }

        private async void btnGetCust_Click(object sender, EventArgs e)
        {
            var paramLst = new List<LrParam>();
            var param = new LrParam();
            param.pKey = "page_size"; param.pValue = "100";
            paramLst.Add(param);
            await FetchWithThrottleAsync("customer", "客户",
                () => new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/bd/customer"));
        }

        private async void btnGetUnits_Click(object sender, EventArgs e)
        {
            await FetchWithThrottleAsync("measure_unit", "单位",
                () => new LrGetMtrlDatum().GetDatum(clientId, clientSecret, AppKey, strAppSecret, strAppToken, "", "", "jdy/v2/bd/measure_unit", "1", "100"));
        }

        private async void btnGetLoc_Click(object sender, EventArgs e)
        {
            await FetchWithThrottleAsync("spaces", "库位",
                () => new LrGetMtrlDatum().GetDatum(clientId, clientSecret, AppKey, strAppSecret, strAppToken, "", "", "jdy/v2/bd/spaces", "1", "100"));
        }

        private void btnSaveInStk_Click(object sender, EventArgs e)
        {
            var clsX = new clsGetToken();
            string strJson = clsX.SaveBill(clientId, clientSecret, strAppToken);
            HandleApiResponse(strJson, "保存入库");
        }

        private async void btnGetMtrlDetail_Click(object sender, EventArgs e)
        {
            // 一键智能取号：右侧结果表选中行 → 左侧物料 Tab 选中行 → 自动拉物料列表第一条
            string billNo = await ResolveDetailKeyAsync(
                new[] { "number", "material_number", "bill_no" }, FetchMaterialForTab, dgvAutoTabMaterial);
            if (string.IsNullOrWhiteSpace(billNo))
            {
                SetStatusHint("未找到可用物料编码：请先在物料列表选中一行，或稍后重试");
                return;
            }
            var paramLst = new List<LrParam>();
            var param = new LrParam();
            param.pKey = "number"; param.pValue = billNo;   // 物料编码来自选中行，不再写死
            paramLst.Add(param);
            await FetchWithThrottleAsync("material_detail", "物料详情",
                () => new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/bd/material_detail"));
        }

        private async void btnIcMoBill_Click(object sender, EventArgs e)
        {
            var paramLst = new List<LrParam>();
            var p1 = new LrParam(); p1.pKey = "page"; p1.pValue = "1"; paramLst.Add(p1);
            var p2 = new LrParam(); p2.pKey = "page_size"; p2.pValue = "100"; paramLst.Add(p2);
            await FetchWithThrottleAsync("mo_taskbill", "生产工单",
                () => new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/pm/mo_taskbill"));
        }

        /// <summary>
        /// 通用：从右侧结果表当前选中行取指定列名的单元格值；未选中则取第一行；表为空则 hasRows=false。
        /// 供"一键智能取号"优先从表格取号使用。
        /// </summary>
        private string GetSelectedValue(out bool hasRows, params string[] candidateNames)
        {
            hasRows = false;
            if (dgvMaterials == null) return null;
            string no = null;
            if (dgvMaterials.CurrentRow != null)
                no = FindCellValue(dgvMaterials.CurrentRow, candidateNames);
            if (string.IsNullOrWhiteSpace(no) && dgvMaterials.Rows.Count > 0)
            {
                hasRows = true;
                no = FindCellValue(dgvMaterials.Rows[0], candidateNames);
            }
            else if (!string.IsNullOrWhiteSpace(no))
            {
                hasRows = true;
            }
            return string.IsNullOrWhiteSpace(no) ? null : no;
        }

        /// <summary>
        /// 详情取号"一键智能"：优先取表格选中行的指定字段（点哪行看哪行）；
        /// 若表格没有可用数据，则自动调用对应列表接口拉一次数据，从第一条提取同名字段。
        /// 这样无需先手动查列表、也无需选中行，点详情按钮即可出结果。
        /// 返回 null 表示确实取不到号（列表也无数据）。
        /// 取号顺序：右侧结果表选中行 → 左侧对应 Tab 表格选中行 → 后台自动拉列表第一条。
        /// </summary>
        /// <param name="candidateNames">要取的字段候选名，如 number/bill_no 或 id/material_id</param>
        /// <param name="listFetcher">对应列表接口的取数委托（在后台线程执行）</param>
        /// <param name="extraGrid">额外的取号来源（如左侧工单/物料 Tab 表格），可为 null</param>
        private async Task<string> ResolveDetailKeyAsync(
            string[] candidateNames, Func<string> listFetcher, DataGridView extraGrid = null)
        {
            // 1) 先查右侧结果表当前行/第一行
            bool hasRows;
            string key = GetSelectedValue(out hasRows, candidateNames);
            if (!string.IsNullOrWhiteSpace(key)) return key;

            // 2) 右表取不到：查左侧对应 Tab 表格（用户看到的往往就是这里的数据）
            key = GetValueFromGrid(extraGrid, candidateNames);
            if (!string.IsNullOrWhiteSpace(key)) return key;

            // 3) 仍然取不到：后台自动拉一次列表，从第一条提取号（不卡 UI）
            if (listFetcher == null) return null;
            try
            {
                string listJson = await Task.Run(listFetcher);
                return ExtractKeyFromFirstRow(listJson, candidateNames);
            }
            catch { return null; }
        }

        /// <summary>从任意 DataGridView 的当前行（或第一行）按候选列名取值，取不到返回 null。</summary>
        private string GetValueFromGrid(DataGridView grid, params string[] candidateNames)
        {
            if (grid == null) return null;
            string v = null;
            if (grid.CurrentRow != null) v = FindCellValue(grid.CurrentRow, candidateNames);
            if (string.IsNullOrWhiteSpace(v) && grid.Rows.Count > 0) v = FindCellValue(grid.Rows[0], candidateNames);
            return string.IsNullOrWhiteSpace(v) ? null : v;
        }

        /// <summary>轻量状态提示：只改标题栏，不弹框打断操作。</summary>
        private void SetStatusHint(string hint)
        {
            if (!IsDisposed && IsHandleCreated)
            {
                try { this.Text = "星辰API调用 - " + hint; } catch { }
            }
        }

        /// <summary>
        /// 从列表接口返回的 JSON 中，取数据第一条记录的指定字段值。
        /// 兼容 data 为数组、data.rows 为数组、data 为单个对象三种结构；字段名忽略大小写。
        /// </summary>
        private string ExtractKeyFromFirstRow(string json, params string[] candidateNames)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                JObject jo = JsonConvert.DeserializeObject<JObject>(json);
                if (jo == null) return null;
                JToken data = jo["data"];
                if (data == null || data.Type == JTokenType.Null) return null;

                JObject first = null;
                if (data is JArray)
                {
                    if (((JArray)data).Count > 0) first = ((JArray)data)[0] as JObject;
                }
                else if (data is JObject)
                {
                    JObject dobj = (JObject)data;
                    JToken rows = dobj["rows"];
                    if (rows is JArray && ((JArray)rows).Count > 0)
                        first = ((JArray)rows)[0] as JObject;
                    else
                        first = dobj;   // data 本身就是单条对象
                }
                if (first == null) return null;

                foreach (string name in candidateNames)
                {
                    foreach (JProperty prop in first.Properties())
                    {
                        if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase)
                            && prop.Value != null && prop.Value.Type != JTokenType.Null)
                        {
                            string v = prop.Value.ToString().Trim();
                            if (v.Length > 0) return v;
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        /// <summary>在一行里按多个候选列名找第一个非空单元格值（列名忽略大小写）。</summary>
        private string FindCellValue(DataGridViewRow row, params string[] candidateNames)
        {
            if (row == null) return null;
            foreach (string name in candidateNames)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.OwningColumn != null
                        && string.Equals(cell.OwningColumn.Name, name, StringComparison.OrdinalIgnoreCase)
                        && cell.Value != null)
                    {
                        string v = cell.Value.ToString().Trim();
                        if (v.Length > 0) return v;
                    }
                }
            }
            return null;
        }

        private async void btnGetMoLst_Click(object sender, EventArgs e)
        {
            // 一键智能取号：右侧结果表选中行 → 左侧工单 Tab 选中行 → 自动拉工单列表第一条
            string billNo = await ResolveDetailKeyAsync(
                new[] { "bill_no", "number", "billNo", "FBillNo", "bill_number", "material_number" },
                FetchWorkOrderForTab, dgvAutoTabWorkOrder);
            if (string.IsNullOrWhiteSpace(billNo))
            {
                SetStatusHint("未找到可用工单号：请先在工单列表选中一行，或稍后重试");
                return;
            }
            var paramLst = new List<LrParam>();
            var param = new LrParam();
            param.pKey = "number"; param.pValue = billNo;   // 单号来自选中行，不再写死
            paramLst.Add(param);
            await FetchWithThrottleAsync("mo_taskbill_detail", "工单详情",
                () => new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/pm/mo_taskbill_detail"));
        }

        private async void btnGetInStkLst_Click(object sender, EventArgs e)
        {
            var paramLst = new List<LrParam>();
            var param = new LrParam();
            param.pKey = "bill_status"; param.pValue = "Z";
            paramLst.Add(param);
            param = new LrParam();
            param.pKey = "end_date"; param.pValue = "2024-08-21";
            paramLst.Add(param);
            param = new LrParam();
            param.pKey = "page"; param.pValue = "1";
            paramLst.Add(param);
            param = new LrParam();
            param.pKey = "page_size"; param.pValue = "100";
            paramLst.Add(param);
            param = new LrParam();
            param.pKey = "start_date"; param.pValue = "2024-08-21";
            paramLst.Add(param);
            await FetchWithThrottleAsync("inv_product", "入库列表",
                () => new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/scm/inv_product"));
        }

        private async void btnGetInStkDetail_Click(object sender, EventArgs e)
        {
            // 一键智能取号：优先用表格选中行，取不到自动拉入库列表第一条
            string id = await ResolveDetailKeyAsync(
                new[] { "id", "material_id", "billid", "FBillID", "FID" },
                () =>
                {
                    var lp = new List<LrParam>();
                    var q = new LrParam(); q.pKey = "bill_status"; q.pValue = "Z"; lp.Add(q);
                    q = new LrParam(); q.pKey = "page"; q.pValue = "1"; lp.Add(q);
                    q = new LrParam(); q.pKey = "page_size"; q.pValue = "100"; lp.Add(q);
                    return new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, lp, "jdy/v2/scm/inv_product");
                });
            if (string.IsNullOrWhiteSpace(id))
            {
                SetStatusHint("未找到可用入库单 id：请先在入库列表选中一行，或稍后重试");
                return;
            }
            var paramLst = new List<LrParam>();
            var param = new LrParam();
            param.pKey = "id"; param.pValue = id;   // id 来自选中行，不再写死
            paramLst.Add(param);
            await FetchWithThrottleAsync("inv_product_detail", "入库明细",
                () => new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/scm/inv_product_detail"));
        }

        private async void btnGetMtrlSort_Click(object sender, EventArgs e)
        {
            await FetchWithThrottleAsync("material_group", "物料分类",
                () => new LrGetMtrlDatum().GetDatum(clientId, clientSecret, AppKey, strAppSecret, strAppToken, "", "", "jdy/v2/bd/material_group", "1", "1000"));
        }

        private async void btnGetMtrlUnit_Click(object sender, EventArgs e)
        {
            await FetchWithThrottleAsync("material_unit", "物料单位",
                () => new LrGetMtrlDatum().GetDatum(clientId, clientSecret, AppKey, strAppSecret, strAppToken, "", "", "jdy/v2/bd/material_unit", "1", "1000"));
        }

        private async void btnGetAux_Click(object sender, EventArgs e)
        {
            var paramLst = new List<LrParam>();
            var param = new LrParam();
            param.pKey = "enable"; param.pValue = "1";
            paramLst.Add(param);
            param = new LrParam();
            param.pKey = "group_id"; param.pValue = "2";
            paramLst.Add(param);
            await FetchWithThrottleAsync("aux_attribute", "辅助属性",
                () => new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/bd/aux_attribute"));
        }

        private async void btnGetPurBill_Click(object sender, EventArgs e)
        {
            var paramLst = new List<LrParam>();
            var p1 = new LrParam(); p1.pKey = "page"; p1.pValue = "1"; paramLst.Add(p1);
            var p2 = new LrParam(); p2.pKey = "page_size"; p2.pValue = "100"; paramLst.Add(p2);
            await FetchWithThrottleAsync("pur_order", "采购订单",
                () => new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/scm/pur_order"));
        }

        private async void btnGetPurOrderLst_Click(object sender, EventArgs e)
        {
            // 一键智能取号：优先用表格选中行，取不到自动拉采购订单列表第一条
            string billNo = await ResolveDetailKeyAsync(
                new[] { "number", "bill_no", "material_number" },
                () =>
                {
                    var lp = new List<LrParam>();
                    var q = new LrParam(); q.pKey = "page"; q.pValue = "1"; lp.Add(q);
                    q = new LrParam(); q.pKey = "page_size"; q.pValue = "100"; lp.Add(q);
                    return new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, lp, "jdy/v2/scm/pur_order");
                });
            if (string.IsNullOrWhiteSpace(billNo))
            {
                SetStatusHint("未找到可用采购订单号：请先在采购订单列表选中一行，或稍后重试");
                return;
            }
            var paramLst = new List<LrParam>();
            var param = new LrParam();
            param.pKey = "number"; param.pValue = billNo;   // 单号来自选中行，不再写死
            paramLst.Add(param);
            await FetchWithThrottleAsync("pur_order_detail", "采购订单详情",
                () => new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/scm/pur_order_detail"));
        }

        private async void btnQueryLock_Click(object sender, EventArgs e)
        {
            var paramLst = new List<LrParam>();
            var param = new LrParam();
            param.pKey = "bill_type"; param.pValue = "mo_task_bill";
            paramLst.Add(param);
            param = new LrParam();
            param.pKey = "page"; param.pValue = "1";
            paramLst.Add(param);
            param = new LrParam();
            param.pKey = "page_size"; param.pValue = "100";
            paramLst.Add(param);

            await FetchWithThrottleAsync("mutex_list", "网络互斥",
                () => new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/sys/mutex_list"),
                strJson =>
                {
                    // 提取所有ID到TextBox供清除操作使用（HandleApiResponse 之后覆盖）
                    try
                    {
                        JObject jo = JsonConvert.DeserializeObject<JObject>(strJson);
                        if (jo["data"]["count"].ToString().CompareTo("0") > 0)
                        {
                            JArray jrLst = (JArray)jo["data"]["rows"];
                            string strId = "";
                            foreach (JObject joX in jrLst)
                            {
                                if (strId == "")
                                    strId = joX["id"].ToString();
                                else
                                    strId = strId + "," + joX["id"].ToString();
                            }
                            TextBox1.Text = strId;
                        }
                    }
                    catch { }
                });
        }

        private void btnClearLock_Click(object sender, EventArgs e)
        {
            if (TextBox1.Text == "")
            {
                MessageBox.Show("请先获取网络互斥信息！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show("确定清除吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                return;
            string[] idLst = TextBox1.Text.Split(',');
            var client = new APIClient(clientId, clientSecret);
            string lastResult = "";
            for (int i = 0; i < idLst.Length; i++)
            {
                JObject jo = new JObject();
                jo.Add("bill_type", "mo_task_bill");
                jo.Add("id", idLst[i]);
                jo.Add("is_force", true);
                jo.Add("sign", "1");
                string api_nonce = new Random().Next(100000000, 999999999).ToString();
                int timeStamp = (int)(DateTime.Now.ToLocalTime() - DateTime.Parse("1970-01-01").ToLocalTime()).TotalSeconds;
                lastResult = client.HttpPost("jdy/v2/sys/mutex_operate", clientId, strAppToken, api_nonce, timeStamp.ToString(), jo.ToString());
                TextBox1.Text = TextBox1.Text + "\r\n" + lastResult;
            }
            if (!string.IsNullOrEmpty(lastResult))
                HandleApiResponse(lastResult, "清除互斥结果");
        }

        private async void btnGetSalOrder_Click(object sender, EventArgs e)
        {
            var paramLst = new List<LrParam>();
            var p1 = new LrParam(); p1.pKey = "page"; p1.pValue = "1"; paramLst.Add(p1);
            var p2 = new LrParam(); p2.pKey = "page_size"; p2.pValue = "100"; paramLst.Add(p2);
            await FetchWithThrottleAsync("sal_order", "销售订单",
                () => new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/scm/sal_order"));
        }

        private async void btnSalOrderLst_Click(object sender, EventArgs e)
        {
            // 一键智能取号：优先用表格选中行，取不到自动拉销售订单列表第一条
            string id = await ResolveDetailKeyAsync(
                new[] { "id", "material_id", "billid", "FBillID", "FID" },
                () =>
                {
                    var lp = new List<LrParam>();
                    var q = new LrParam(); q.pKey = "page"; q.pValue = "1"; lp.Add(q);
                    q = new LrParam(); q.pKey = "page_size"; q.pValue = "100"; lp.Add(q);
                    return new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, lp, "jdy/v2/scm/sal_order");
                });
            if (string.IsNullOrWhiteSpace(id))
            {
                SetStatusHint("未找到可用销售订单 id：请先在销售订单列表选中一行，或稍后重试");
                return;
            }
            var paramLst = new List<LrParam>();
            var param = new LrParam();
            param.pKey = "id"; param.pValue = id;   // 内码来自选中行，不再写死
            paramLst.Add(param);
            await FetchWithThrottleAsync("sal_order_detail", "销售订单明细",
                () => new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/scm/sal_order_detail"));
        }

        private async void btnSalOutStk_Click(object sender, EventArgs e)
        {
            var paramLst = new List<LrParam>();
            var param = new LrParam();
            param.pKey = "end_bill_date"; param.pValue = DateTime.Today.ToString("yyyy-MM-dd");
            paramLst.Add(param);
            param = new LrParam();
            param.pKey = "filter_zero"; param.pValue = "true";
            paramLst.Add(param);
            param = new LrParam();
            param.pKey = "page_size"; param.pValue = "100";
            paramLst.Add(param);
            param = new LrParam();
            param.pKey = "start_bill_date"; param.pValue = DateTime.Today.ToString("yyyy-MM-dd");
            paramLst.Add(param);
            param = new LrParam();
            param.pKey = "total_fields"; param.pValue = "allamount";
            paramLst.Add(param);
            await FetchWithThrottleAsync("sal_out_bound", "销售出库单列表",
                () => new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/scm/sal_out_bound"));
        }

        private async void btnSalOutStkLst_Click(object sender, EventArgs e)
        {
            // 一键智能取号：优先用表格选中行，取不到自动拉销售出库单列表第一条
            string id = await ResolveDetailKeyAsync(
                new[] { "id", "material_id", "billid", "FBillID", "FID" },
                () =>
                {
                    var lp = new List<LrParam>();
                    var q = new LrParam(); q.pKey = "end_bill_date"; q.pValue = DateTime.Today.ToString("yyyy-MM-dd"); lp.Add(q);
                    q = new LrParam(); q.pKey = "start_bill_date"; q.pValue = DateTime.Today.ToString("yyyy-MM-dd"); lp.Add(q);
                    q = new LrParam(); q.pKey = "page_size"; q.pValue = "100"; lp.Add(q);
                    return new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, lp, "jdy/v2/scm/sal_out_bound");
                });
            if (string.IsNullOrWhiteSpace(id))
            {
                SetStatusHint("未找到可用销售出库单 id：请先在销售出库列表选中一行，或稍后重试");
                return;
            }
            var paramLst = new List<LrParam>();
            var param = new LrParam();
            param.pKey = "id"; param.pValue = id;   // 内码来自选中行，不再写死
            paramLst.Add(param);
            await FetchWithThrottleAsync("sal_out_bound_detail", "销售出库明细",
                () => new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/scm/sal_out_bound_detail"));
        }

        private async void btnSalTotalRpt_Click(object sender, EventArgs e)
        {
            var paramLst = new List<LrParam>();
            var param = new LrParam();
            param.pKey = "end_date"; param.pValue = DateTime.Today.ToString("yyyy-MM-dd");
            paramLst.Add(param);
            param = new LrParam();
            param.pKey = "filter_summary_field"; param.pValue = "customerid";
            paramLst.Add(param);
            param = new LrParam();
            param.pKey = "page"; param.pValue = "1";
            paramLst.Add(param);
            param = new LrParam();
            param.pKey = "page_size"; param.pValue = "100";
            paramLst.Add(param);
            param = new LrParam();
            param.pKey = "start_date"; param.pValue = "2026-05-01";
            paramLst.Add(param);
            await FetchWithThrottleAsync("sal_summary_rpt_ims", "销售汇总表",
                () => new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/sal/sal_summary_rpt_ims"));
        }

        private async void btnQueryStk_Click(object sender, EventArgs e)
        {
            var paramLst = new List<LrParam>();
            // 去掉写死的 filter_material_id：原来永远只查某一个物料的库存，
            // 现在不带该过滤条件，查全部物料库存。
            var param = new LrParam();
            param.pKey = "opt_group_inv_query_type"; param.pValue = "1";
            paramLst.Add(param);
            param = new LrParam();
            param.pKey = "type"; param.pValue = "1";
            paramLst.Add(param);
            await FetchWithThrottleAsync("inv_inventory_ims", "库存查询",
                () => new LrGetMtrlDatum().GetDatumX(clientId, clientSecret, strAppToken, paramLst, "jdy/v2/inv/inv_inventory_ims"));
        }
    }
}