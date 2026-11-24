using System;

namespace WindowsApplication3.Data
{
    /// <summary>
    /// 数据变更比对历史表：当同一业务键的数据内容与库中不同（DataHash 不一致）时，
    /// 记录“旧值/新值/差异字段”，实现“不同数据比对后再保存”。
    /// </summary>
    public class ApiRecordChange
    {
        public int Id { get; set; }

        public string Endpoint { get; set; }

        /// <summary>接口中文名（按钮名称）</summary>
        public string EndpointName { get; set; }

        public string BizKey { get; set; }

        /// <summary>对应主表记录id（主表记录被删除时可为空）</summary>
        public int? RecordId { get; set; }

        /// <summary>变更前数据哈希</summary>
        public string OldHash { get; set; }

        /// <summary>变更后数据哈希</summary>
        public string NewHash { get; set; }

        /// <summary>差异字段明细 JSON：[{"field":"","old":"","new":""}]</summary>
        public string ChangedFields { get; set; }

        /// <summary>差异字段数量</summary>
        public int ChangedCount { get; set; }

        /// <summary>变更前完整JSON</summary>
        public string OldJson { get; set; }

        /// <summary>变更后完整JSON</summary>
        public string NewJson { get; set; }

        public DateTime ChangeTime { get; set; }
    }
}
