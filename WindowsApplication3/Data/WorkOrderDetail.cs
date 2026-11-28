using System;

namespace WindowsApplication3.Data
{
    /// <summary>
    /// 生产任务单（工单）专用表：把「工单详情」按钮请求到的工单信息结构化保存。
    /// 与通用动态表 api_record 并存、互不干扰——本表是强类型业务表，便于按工单号/日期查询。
    /// 一条金蝶工单 = 一条记录，以 BillNo（单据编号）为业务唯一键。
    /// 产品明细/用料明细的完整结构尚未确认，先原样存 RawJson，后续需要再拆列。
    /// </summary>
    public class WorkOrderDetail
    {
        public int Id { get; set; }

        /// <summary>单据编号（bill_no），业务唯一键</summary>
        public string BillNo { get; set; }

        /// <summary>金蝶内码（id）</summary>
        public string InnerId { get; set; }

        /// <summary>单据日期（bill_date）</summary>
        public string BillDate { get; set; }

        /// <summary>单据状态（bill_status）</summary>
        public string BillStatus { get; set; }

        /// <summary>生产车间（dept_name）</summary>
        public string DeptName { get; set; }

        /// <summary>计划员/制单人（creator_name）</summary>
        public string CreatorName { get; set; }

        /// <summary>审核人（auditor_name）</summary>
        public string AuditorName { get; set; }

        /// <summary>事务类型（trans_type_name）</summary>
        public string TransTypeName { get; set; }

        /// <summary>备注（remark）</summary>
        public string Remark { get; set; }

        /// <summary>规范化 JSON 的 SHA256，用于判断是否变化</summary>
        public string DataHash { get; set; }

        /// <summary>工单详情接口返回的完整原始 JSON（含产品明细/用料明细）</summary>
        public string RawJson { get; set; }

        /// <summary>首次保存时间</summary>
        public DateTime FirstSyncTime { get; set; }

        /// <summary>最后保存时间</summary>
        public DateTime LastSyncTime { get; set; }

        /// <summary>保存命中次数（数据相同也会累加）</summary>
        public int SyncCount { get; set; }

        /// <summary>内容发生变化并更新的次数</summary>
        public int ChangeCount { get; set; }
    }
}
