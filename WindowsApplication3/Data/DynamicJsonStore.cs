using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace WindowsApplication3.Data
{
    /// <summary>
    /// 一次入库的结果统计
    /// </summary>
    public class SaveResult
    {
        public bool Success;
        public int Total;
        public int Inserted;
        public int Updated;
        public int Unchanged;
        public string Message;

        public string Summary
        {
            get
            {
                return string.Format("共{0}条：新增{1}，比对更新{2}，相同未重复存{3}",
                    Total, Inserted, Updated, Unchanged);
            }
        }
    }

    /// <summary>
    /// 动态JSON入库服务（EF Core）：
    /// 1) 任意接口返回的JSON自动拆行、拍平字段，无需为每个接口建表（动态保存）；
    /// 2) 以 接口+业务键 唯一定位记录，内容哈希相同 => 只更新同步次数，不重复存（相同数据仅存一次）；
    /// 3) 内容哈希不同 => 先逐字段比对，把差异写入变更历史表，再更新主表与字段表（不同数据比对后保存）。
    /// </summary>
    public class DynamicJsonStore
    {
        private class Prepared
        {
            public string BizKey;
            public string Title;
            public string Hash;
            public string Json;
            public List<FlatField> Fields;
        }

        /// <summary>
        /// 把一次API响应JSON保存到SQL Server
        /// </summary>
        public SaveResult Save(string endpoint, string endpointName, string json)
        {
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            SaveResult result = new SaveResult();
            result.Total = 0;

            List<JObject> rows = JsonFlattener.ExtractRows(json);
            result.Total = rows.Count;

            List<Prepared> prepared = new List<Prepared>();
            foreach (JObject row in rows)
            {
                Prepared item = new Prepared();
                item.Hash = JsonFlattener.ComputeHash(row);
                item.BizKey = JsonFlattener.PickBizKey(row, item.Hash);
                item.Title = JsonFlattener.PickTitle(row);
                item.Json = row.ToString(Newtonsoft.Json.Formatting.None);
                item.Fields = JsonFlattener.Flatten(row);
                prepared.Add(item);
            }

            DateTime now = DateTime.Now;
            using (XcApiDbContext db = new XcApiDbContext())
            {
                using (var tx = db.Database.BeginTransaction())
                {
                    try
                    {
                        List<string> bizKeys = prepared.Select(p => p.BizKey).Distinct().ToList();
                        List<ApiRecord> found = db.ApiRecords
                            .Include(r => r.Fields)
                            .Where(r => r.Endpoint == endpoint && bizKeys.Contains(r.BizKey))
                            .ToList();

                        Dictionary<string, ApiRecord> map = new Dictionary<string, ApiRecord>(StringComparer.Ordinal);
                        foreach (ApiRecord r in found)
                        {
                            if (!map.ContainsKey(r.BizKey)) map[r.BizKey] = r;
                        }

                        foreach (Prepared item in prepared)
                        {
                            ApiRecord rec;
                            if (map.TryGetValue(item.BizKey, out rec))
                            {
                                bool hashSame = string.Equals(rec.DataHash, item.Hash, StringComparison.OrdinalIgnoreCase);
                                // 自愈：旧版本把嵌套字段展开成 a.b / a[0].b 碎行。若检测到旧格式，
                                // 即使内容哈希相同也要重写字段表，把碎行合并成"嵌套对象整体一行"。
                                bool legacyFields = HasLegacyFieldNames(rec.Fields);
                                if (hashSame && !legacyFields)
                                {
                                    // 相同数据且字段表已是新格式：不再重复保存，只累计同步次数与时间
                                    rec.SyncCount += 1;
                                    rec.LastSyncTime = now;
                                    result.Unchanged++;
                                }
                                else
                                {
                                    bool dataChanged = !hashSame;   // 内容是否真的变了（旧格式重写不算内容变化）
                                    if (dataChanged)
                                    {
                                        // 数据真变了：先比对差异，写入变更历史
                                        Dictionary<string, string> oldMap = EntityFieldMap.ToMap(rec.Fields);
                                        string diffJson = BuildDiffJson(oldMap, item.Fields);
                                        int diffCount = CountDiff(oldMap, item.Fields);

                                        db.ApiRecordChanges.Add(new ApiRecordChange
                                        {
                                            Endpoint = endpoint,
                                            EndpointName = endpointName,
                                            BizKey = item.BizKey,
                                            RecordId = rec.Id > 0 ? (int?)rec.Id : null,
                                            OldHash = rec.DataHash,
                                            NewHash = item.Hash,
                                            ChangedFields = diffJson,
                                            ChangedCount = diffCount,
                                            OldJson = rec.Json,
                                            NewJson = item.Json,
                                            ChangeTime = now
                                        });
                                    }

                                    if (rec.Fields != null && rec.Fields.Count > 0)
                                    {
                                        db.ApiRecordFields.RemoveRange(rec.Fields);
                                        rec.Fields.Clear();
                                        // 关键修复：先提交删除，再插入新字段。
                                        // 否则 EF Core 在同一次 SaveChanges 中对相同 (RecordId, FieldName)
                                        // 可能先 INSERT 后 DELETE，违反唯一索引 ux_api_record_field_record_field
                                        // 抛" Violation of UNIQUE KEY constraint"异常，导致入库失败。
                                        db.SaveChanges();
                                    }
                                    rec.Json = item.Json;
                                    rec.DataHash = item.Hash;
                                    rec.Title = item.Title ?? rec.Title;
                                    rec.FieldCount = item.Fields.Count;
                                    rec.EndpointName = endpointName ?? rec.EndpointName;
                                    rec.LastSyncTime = now;
                                    rec.SyncCount += 1;
                                    if (dataChanged) rec.ChangeCount += 1;
                                    AddFields(db, rec, item.Fields);
                                    result.Updated++;
                                }
                            }
                            else
                            {
                                rec = new ApiRecord
                                {
                                    Endpoint = endpoint,
                                    EndpointName = endpointName,
                                    BizKey = item.BizKey,
                                    Title = item.Title,
                                    DataHash = item.Hash,
                                    Json = item.Json,
                                    FieldCount = item.Fields.Count,
                                    FirstSyncTime = now,
                                    LastSyncTime = now,
                                    SyncCount = 1,
                                    ChangeCount = 0,
                                    Fields = new List<ApiRecordField>()
                                };
                                AddFields(db, rec, item.Fields);
                                db.ApiRecords.Add(rec);
                                map[item.BizKey] = rec;   // 同一批内重复键也不会重复入库
                                result.Inserted++;
                            }
                        }

                        db.ApiSyncLogs.Add(new ApiSyncLog
                        {
                            Endpoint = endpoint,
                            EndpointName = endpointName,
                            SyncTime = now,
                            TotalRows = result.Total,
                            InsertedRows = result.Inserted,
                            UpdatedRows = result.Updated,
                            UnchangedRows = result.Unchanged,
                            Status = "成功",
                            Message = result.Summary,
                            DurationMs = (int)watch.ElapsedMilliseconds
                        });

                        db.SaveChanges();
                        tx.Commit();
                        result.Success = true;
                        result.Message = result.Summary;
                    }
                    catch (Exception)
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 检测一条记录是否还是旧版本"展开式"字段格式。
        /// 旧格式会把嵌套字段拍成 primary_unit.unit_id、alt_units[0].unit_id 这种碎行，
        /// 字段名里带 '.'（嵌套）或 '['（数组下标）；新格式嵌套对象/数组整体存一行，字段名不含这两者。
        /// 检测到即触发字段表重写（自愈），无需删库重建。
        /// </summary>
        private static bool HasLegacyFieldNames(List<ApiRecordField> fields)
        {
            if (fields == null) return false;
            foreach (ApiRecordField f in fields)
            {
                if (string.IsNullOrEmpty(f.FieldName)) continue;
                if (f.FieldName.IndexOf('.') >= 0 || f.FieldName.IndexOf('[') >= 0) return true;
            }
            return false;
        }

        private static void AddFields(XcApiDbContext db, ApiRecord rec, List<FlatField> fields)
        {
            foreach (FlatField f in fields)
            {
                ApiRecordField ef = new ApiRecordField
                {
                    FieldName = f.Name,
                    FieldValue = f.Value,
                    FieldType = f.Type,
                    RecordId = rec.Id
                };
                rec.Fields.Add(ef);
                db.ApiRecordFields.Add(ef);
            }
        }

        private static string BuildDiffJson(Dictionary<string, string> oldMap, List<FlatField> newFields)
        {
            Dictionary<string, string> newMap = ToMap(newFields);
            JArray arr = new JArray();

            foreach (KeyValuePair<string, string> kv in newMap)
            {
                string old;
                if (!oldMap.TryGetValue(kv.Key, out old))
                {
                    arr.Add(new JObject { { "type", "added" }, { "field", kv.Key }, { "new", Cut(kv.Value) } });
                }
                else if (!string.Equals(old, kv.Value, StringComparison.Ordinal))
                {
                    arr.Add(new JObject { { "type", "changed" }, { "field", kv.Key }, { "old", Cut(old) }, { "new", Cut(kv.Value) } });
                }
            }
            foreach (KeyValuePair<string, string> kv in oldMap)
            {
                string neu;
                if (!newMap.TryGetValue(kv.Key, out neu))
                {
                    arr.Add(new JObject { { "type", "removed" }, { "field", kv.Key }, { "old", Cut(kv.Value) } });
                }
            }
            if (arr.Count > 200)
            {
                // 差异过多时只保留前200条，避免单行过大
                JArray trimmed = new JArray();
                for (int i = 0; i < 200; i++) trimmed.Add(arr[i]);
                trimmed.Add(new JObject { { "type", "more" }, { "field", "差异过多已截断" } });
                return trimmed.ToString(Newtonsoft.Json.Formatting.None);
            }
            return arr.ToString(Newtonsoft.Json.Formatting.None);
        }

        private static int CountDiff(Dictionary<string, string> oldMap, List<FlatField> newFields)
        {
            Dictionary<string, string> newMap = ToMap(newFields);
            int count = 0;
            foreach (KeyValuePair<string, string> kv in newMap)
            {
                string old;
                if (!oldMap.TryGetValue(kv.Key, out old) || !string.Equals(old, kv.Value, StringComparison.Ordinal)) count++;
            }
            foreach (string key in oldMap.Keys)
            {
                if (!newMap.ContainsKey(key)) count++;
            }
            return count;
        }

        private static Dictionary<string, string> ToMap(List<FlatField> fields)
        {
            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.Ordinal);
            if (fields == null) return map;
            foreach (FlatField f in fields)
            {
                map[f.Name] = f.Value;
            }
            return map;
        }

        /// <summary>
        /// 把已入库的字段实体转成“字段名 -> 值”的字典，便于与新数据逐字段比对
        /// </summary>
        private static class EntityFieldMap
        {
            public static Dictionary<string, string> ToMap(List<ApiRecordField> fields)
            {
                Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.Ordinal);
                if (fields == null) return map;
                foreach (ApiRecordField f in fields)
                {
                    map[f.FieldName] = f.FieldValue;
                }
                return map;
            }
        }

        private static string Cut(string value)
        {
            if (value == null) return null;
            return value.Length <= 500 ? value : value.Substring(0, 500) + "...";
        }

        /// <summary>
        /// 从SQL Server读回已入库记录，用于验证“写入 → 读取”闭环。
        /// 按最后同步时间倒序返回，支持按接口路径和业务键关键字过滤。
        /// </summary>
        /// <param name="endpoint">接口路径；为空则不限制接口</param>
        /// <param name="bizKeyContains">业务键包含的关键字；为空则不限制</param>
        /// <param name="topN">最多返回条数；小于等于0时按10条处理</param>
        /// <returns>记录列表；无数据时返回空列表，读取异常由界面层捕获并明确提示</returns>
        public List<ApiRecord> QueryRecords(string endpoint, string bizKeyContains, int topN)
        {
            List<ApiRecord> list = new List<ApiRecord>();
            if (topN <= 0) topN = 10;
            using (XcApiDbContext db = new XcApiDbContext())
            {
                // 幂等调用：若库/表被手工删除，这里会自动重建，避免查询直接抛“无效对象名”
                db.Database.EnsureCreated();

                IQueryable<ApiRecord> query = db.ApiRecords;
                if (!string.IsNullOrEmpty(endpoint))
                {
                    query = query.Where(r => r.Endpoint == endpoint);
                }
                if (!string.IsNullOrEmpty(bizKeyContains))
                {
                    query = query.Where(r => r.BizKey.Contains(bizKeyContains));
                }
                list = query.OrderByDescending(r => r.LastSyncTime)
                            .Take(topN)
                            .ToList();
            }
            return list;
        }

        /// <summary>
        /// 【只读/高并发场景】异步读取指定接口的最新 N 条记录。
        /// 关键：AsNoTracking() 让 EF Core 不把实体放入变更跟踪器——
        /// 不建快照、不做身份解析，查询更快、每个客户端的 DbContext 内存更低；
        /// 多台设备同时只读不会产生锁冲突（SQL Server 默认 READ COMMITTED 下共享锁兼容）。
        /// 注意：只读查询专用；若要修改实体再保存，不能用 AsNoTracking()。
        /// </summary>
        /// <param name="endpoint">接口路径；为空则不限制接口</param>
        /// <param name="topN">最多返回条数；小于等于0时按10条处理</param>
        /// <returns>只读记录列表（无变更跟踪）</returns>
        public async Task<List<ApiRecord>> QueryRecordsAsync(string endpoint, int topN)
        {
            if (topN <= 0) topN = 10;
            using (XcApiDbContext db = new XcApiDbContext())
            {
                await db.Database.EnsureCreatedAsync();

                IQueryable<ApiRecord> query = db.ApiRecords.AsNoTracking();
                if (!string.IsNullOrEmpty(endpoint))
                {
                    query = query.Where(r => r.Endpoint == endpoint);
                }
                return await query.OrderByDescending(r => r.LastSyncTime)
                                  .Take(topN)
                                  .ToListAsync();
            }
        }

        /// <summary>
        /// 测试数据库连通性并返回统计信息
        /// </summary>
        public static string TestConnection()
        {
            bool ok;
            string initMsg = XcApiDbContext.EnsureDatabase(out ok);
            if (!ok) return initMsg;

            using (XcApiDbContext db = new XcApiDbContext())
            {
                int records = db.ApiRecords.Count();
                int fields = db.ApiRecordFields.Count();
                int changes = db.ApiRecordChanges.Count();
                int logs = db.ApiSyncLogs.Count();
                return string.Format("连接成功！\n库：XingChenDb（SQL Server）\n记录 {0} 条 / 动态字段 {1} 行 / 变更 {2} 次 / 同步日志 {3} 条",
                    records, fields, changes, logs);
            }
        }
    }
}
