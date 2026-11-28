using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace WindowsApplication3.Data
{
    /// <summary>工单详情入库结果统计</summary>
    public class WorkOrderSaveResult
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
                return string.Format("共{0}张工单：新增{1}，更新{2}，无变化{3}",
                    Total, Inserted, Updated, Unchanged);
            }
        }
    }

    /// <summary>
    /// 工单详情入库服务：把「工单详情」按钮请求到的 JSON 结构化保存到 work_order_detail 表。
    /// 以 BillNo（单据编号）为唯一键：
    ///   · 新工单 => 新增；
    ///   · 已存在且内容哈希相同 => 只累加同步次数；
    ///   · 已存在但内容不同 => 更新字段并覆盖原始 JSON。
    /// 详情接口返回结构可能随金蝶版本变化，表头字段用候选名兜底提取，
    /// 无论如何都把完整原文存进 RawJson，保证不丢数据。
    /// </summary>
    public class WorkOrderStore
    {
        // 表头字段候选名（按优先级，不区分大小写）
        private static readonly string[] BillNoKeys = { "bill_no", "billNo", "FBillNo", "bill_number", "number" };
        private static readonly string[] InnerIdKeys = { "id", "inner_id", "FID", "billid", "bill_id" };
        private static readonly string[] BillDateKeys = { "bill_date", "billDate", "FDate", "date" };
        private static readonly string[] BillStatusKeys = { "bill_status", "billStatus", "FBillStatus", "status" };
        private static readonly string[] DeptNameKeys = { "dept_name", "deptName", "FDeptName", "department_name", "workshop" };
        private static readonly string[] CreatorKeys = { "creator_name", "creatorName", "FCreatorName", "creator", "planner" };
        private static readonly string[] AuditorKeys = { "auditor_name", "auditorName", "FAuditorName", "auditor" };
        private static readonly string[] TransTypeKeys = { "trans_type_name", "transTypeName", "FTransTypeName", "trans_type" };
        private static readonly string[] RemarkKeys = { "remark", "Remark", "FRemark", "note" };

        /// <summary>
        /// 把工单详情 JSON 保存到数据库。
        /// </summary>
        /// <param name="json">工单详情接口返回的完整 JSON</param>
        /// <param name="fallbackBillNo">兜底单据编号：从列表选中行取到的工单号，当详情 JSON 里识别不到编号时使用</param>
        public WorkOrderSaveResult Save(string json, string fallbackBillNo)
        {
            WorkOrderSaveResult result = new WorkOrderSaveResult();
            result.Total = 0;

            List<JObject> rows = JsonFlattener.ExtractRows(json);
            result.Total = rows.Count;
            if (rows.Count == 0)
            {
                result.Success = false;
                result.Message = "JSON 中未解析出工单数据";
                return result;
            }

            DateTime now = DateTime.Now;
            using (XcApiDbContext db = new XcApiDbContext())
            {
                // 幂等补表：老库 EnsureCreated 不会建新表，这里兜底保证表存在
                XcApiDbContext.EnsureWorkOrderDetailTable(db);

                using (var tx = db.Database.BeginTransaction())
                {
                    try
                    {
                        // 详情接口通常只返回一张工单；若返回多条，逐条处理
                        foreach (JObject row in rows)
                        {
                            UpsertOne(db, row, json, fallbackBillNo, now, result);
                        }

                        db.SaveChanges();
                        tx.Commit();
                        result.Success = true;
                        result.Message = result.Summary;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
            return result;
        }

        /// <summary>单张工单的新增/更新/未变处理</summary>
        private static void UpsertOne(XcApiDbContext db, JObject row, string rawJson,
            string fallbackBillNo, DateTime now, WorkOrderSaveResult result)
        {
            string hash = JsonFlattener.ComputeHash(row);
            string billNo = PickFirst(row, BillNoKeys);
            if (string.IsNullOrWhiteSpace(billNo)) billNo = fallbackBillNo;
            if (string.IsNullOrWhiteSpace(billNo)) billNo = "hash=" + hash.Substring(0, Math.Min(32, hash.Length));
            billNo = Truncate(billNo, 200);

            WorkOrderDetail existing = null;
            // 先物化再查找：避免遍历 DbSet 过程中 Add 实体带来的枚举不确定性
            List<WorkOrderDetail> all = db.WorkOrderDetails.ToList();
            foreach (WorkOrderDetail w in all)
            {
                if (string.Equals(w.BillNo, billNo, StringComparison.Ordinal))
                {
                    existing = w;
                    break;
                }
            }

            if (existing == null)
            {
                db.WorkOrderDetails.Add(new WorkOrderDetail
                {
                    BillNo = billNo,
                    InnerId = Truncate(PickFirst(row, InnerIdKeys), 100),
                    BillDate = Truncate(PickFirst(row, BillDateKeys), 50),
                    BillStatus = Truncate(PickFirst(row, BillStatusKeys), 50),
                    DeptName = Truncate(PickFirst(row, DeptNameKeys), 200),
                    CreatorName = Truncate(PickFirst(row, CreatorKeys), 200),
                    AuditorName = Truncate(PickFirst(row, AuditorKeys), 200),
                    TransTypeName = Truncate(PickFirst(row, TransTypeKeys), 200),
                    Remark = Truncate(PickFirst(row, RemarkKeys), 1000),
                    DataHash = hash,
                    RawJson = rawJson,
                    FirstSyncTime = now,
                    LastSyncTime = now,
                    SyncCount = 1,
                    ChangeCount = 0
                });
                result.Inserted++;
                return;
            }

            existing.SyncCount += 1;
            existing.LastSyncTime = now;
            if (string.Equals(existing.DataHash, hash, StringComparison.OrdinalIgnoreCase))
            {
                result.Unchanged++;
                return;
            }

            // 内容变了：更新表头字段、哈希与原文
            existing.InnerId = Truncate(PickFirst(row, InnerIdKeys), 100);
            existing.BillDate = Truncate(PickFirst(row, BillDateKeys), 50);
            existing.BillStatus = Truncate(PickFirst(row, BillStatusKeys), 50);
            existing.DeptName = Truncate(PickFirst(row, DeptNameKeys), 200);
            existing.CreatorName = Truncate(PickFirst(row, CreatorKeys), 200);
            existing.AuditorName = Truncate(PickFirst(row, AuditorKeys), 200);
            existing.TransTypeName = Truncate(PickFirst(row, TransTypeKeys), 200);
            existing.Remark = Truncate(PickFirst(row, RemarkKeys), 1000);
            existing.DataHash = hash;
            existing.RawJson = rawJson;
            existing.ChangeCount += 1;
            result.Updated++;
        }

        /// <summary>按候选名取第一个非空标量值（不区分大小写）</summary>
        private static string PickFirst(JObject row, string[] keys)
        {
            if (row == null) return null;
            foreach (string key in keys)
            {
                JToken t = null;
                foreach (JProperty prop in row.Properties())
                {
                    if (string.Equals(prop.Name, key, StringComparison.OrdinalIgnoreCase))
                    {
                        t = prop.Value;
                        break;
                    }
                }
                if (t == null || t.Type == JTokenType.Null ||
                    t.Type == JTokenType.Object || t.Type == JTokenType.Array) continue;
                string v = t.ToString().Trim();
                if (v.Length > 0) return v;
            }
            return null;
        }

        private static string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return value.Length <= max ? value : value.Substring(0, max);
        }

        /// <summary>
        /// 从数据库读回工单详情，用于验证"写入 → 读取"闭环。
        /// </summary>
        /// <param name="billNoContains">工单号包含的关键字，为空则不限制</param>
        /// <param name="topN">最多返回条数</param>
        public static List<WorkOrderDetail> Query(string billNoContains, int topN)
        {
            if (topN <= 0) topN = 10;
            List<WorkOrderDetail> list = new List<WorkOrderDetail>();
            using (XcApiDbContext db = new XcApiDbContext())
            {
                XcApiDbContext.EnsureWorkOrderDetailTable(db);
                System.Linq.IQueryable<WorkOrderDetail> query = db.WorkOrderDetails.AsNoTracking();
                if (!string.IsNullOrEmpty(billNoContains))
                    query = query.Where(w => w.BillNo.Contains(billNoContains));
                list = query.OrderByDescending(w => w.LastSyncTime).Take(topN).ToList();
            }
            return list;
        }
    }
}
