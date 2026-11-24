using System;

namespace WindowsApplication3.Data
{
    /// <summary>
    /// 同步日志表：每点击一次按钮（每一次API响应入库）记一条，便于核对入库结果。
    /// </summary>
    public class ApiSyncLog
    {
        public int Id { get; set; }

        public string Endpoint { get; set; }

        public string EndpointName { get; set; }

        public DateTime SyncTime { get; set; }

        /// <summary>本次响应解析出的数据行数</summary>
        public int TotalRows { get; set; }

        /// <summary>新增入库行数（库中原来没有）</summary>
        public int InsertedRows { get; set; }

        /// <summary>比对后内容有变化并更新的行数</summary>
        public int UpdatedRows { get; set; }

        /// <summary>内容完全相同、未重复存储的行数</summary>
        public int UnchangedRows { get; set; }

        /// <summary>成功/失败</summary>
        public string Status { get; set; }

        public string Message { get; set; }

        /// <summary>本次入库耗时（毫秒）</summary>
        public int DurationMs { get; set; }
    }
}
