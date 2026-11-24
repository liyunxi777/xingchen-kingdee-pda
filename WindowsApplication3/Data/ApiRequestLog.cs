using System;

namespace WindowsApplication3.Data
{
    /// <summary>
    /// API 请求日志表：每一次真实发出的金蝶请求记一条，用于追溯"哪台设备/用户、什么时间、
    /// 请求了哪个接口、传了什么参数、返回了多少条、成功还是失败"。
    /// 与 api_sync_log（入库结果）区别：本表记录"请求动作"本身，与是否入库无关。
    /// </summary>
    public class ApiRequestLog
    {
        public int Id { get; set; }

        /// <summary>设备编号（PDA 编号 / 机器名），回答"是谁请求的"</summary>
        public string DeviceId { get; set; }

        /// <summary>接口路径，如 jdy/v2/bd/material</summary>
        public string Endpoint { get; set; }

        /// <summary>按钮/接口中文名，如 物料管理</summary>
        public string EndpointName { get; set; }

        /// <summary>本次请求参数，序列化后的 key=value 文本</summary>
        public string RequestParams { get; set; }

        /// <summary>请求发生时间</summary>
        public DateTime RequestTime { get; set; }

        /// <summary>返回数据条数（解析失败记 -1）</summary>
        public int ResponseRows { get; set; }

        /// <summary>成功 / 失败</summary>
        public string Status { get; set; }

        /// <summary>失败原因 / 备注</summary>
        public string Message { get; set; }
    }
}
