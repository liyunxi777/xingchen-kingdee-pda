using System;
using System.Collections.Generic;

namespace WindowsApplication3.Data
{
    /// <summary>
    /// 动态JSON记录主表：一条API数据行 = 一条记录。
    /// 以 (Endpoint + BizKey) 作为业务唯一键，DataHash 用于判断内容是否发生变化，
    /// 从而实现“相同的数据仅存一次，不同的数据比对后更新”。
    /// </summary>
    public class ApiRecord
    {
        public int Id { get; set; }

        /// <summary>接口路径，如 jdy/v2/bd/material</summary>
        public string Endpoint { get; set; }

        /// <summary>接口中文名，如 物料管理</summary>
        public string EndpointName { get; set; }

        /// <summary>业务主键（自动从 id/number/bill_no 等字段识别，识别不到时用内容哈希）</summary>
        public string BizKey { get; set; }

        /// <summary>显示名称（自动从 name/full_number/bill_no 等字段识别）</summary>
        public string Title { get; set; }

        /// <summary>规范化JSON的SHA256哈希，用于数据比对去重</summary>
        public string DataHash { get; set; }

        /// <summary>该条数据的原始JSON</summary>
        public string Json { get; set; }

        /// <summary>拍平后的字段数量</summary>
        public int FieldCount { get; set; }

        /// <summary>首次同步时间</summary>
        public DateTime FirstSyncTime { get; set; }

        /// <summary>最后同步时间</summary>
        public DateTime LastSyncTime { get; set; }

        /// <summary>同步命中次数（数据相同也会累加，用于观察重复拉取次数）</summary>
        public int SyncCount { get; set; }

        /// <summary>数据发生变化并入库的次数</summary>
        public int ChangeCount { get; set; }

        /// <summary>动态字段明细</summary>
        public virtual List<ApiRecordField> Fields { get; set; }

        public ApiRecord()
        {
            Fields = new List<ApiRecordField>();
        }
    }
}
