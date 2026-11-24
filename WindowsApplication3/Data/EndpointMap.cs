using System.Collections.Generic;

namespace WindowsApplication3.Data
{
    /// <summary>
    /// 按钮中文名 -> 金蝶星辰接口路径 的映射，用于入库时记录数据来源接口。
    /// 未登记的标签会以 api/标签 形式保存，保证不会丢数据。
    /// </summary>
    public static class EndpointMap
    {
        private static readonly Dictionary<string, string> Map = new Dictionary<string, string>(System.StringComparer.Ordinal)
        {
            { "物料管理", "jdy/v2/bd/material" },
            { "物料详情", "jdy/v2/bd/material_detail" },
            { "物料分类", "jdy/v2/bd/material_group" },
            { "物料单位", "jdy/v2/bd/material_unit" },
            { "仓库", "jdy/v2/bd/store" },
            { "部门", "jdy/v2/bd/department" },
            { "人员", "jdy/v2/bd/emp" },
            { "客户", "jdy/v2/bd/customer" },
            { "单位", "jdy/v2/bd/measure_unit" },
            { "库位", "jdy/v2/bd/spaces" },
            { "辅助属性", "jdy/v2/bd/aux_attribute" },
            { "生产工单", "jdy/v2/pm/mo_taskbill" },
            { "工单详情", "jdy/v2/pm/mo_taskbill_detail" },
            { "保存入库", "jdy/v2/scm/inv_product" },
            { "入库列表", "jdy/v2/scm/inv_product" },
            { "入库明细", "jdy/v2/scm/inv_product_detail" },
            { "采购订单", "jdy/v2/scm/pur_order" },
            { "采购订单详情", "jdy/v2/scm/pur_order_detail" },
            { "网络互斥", "jdy/v2/sys/mutex_list" },
            { "清除互斥结果", "jdy/v2/sys/mutex_operate" },
            { "销售订单", "jdy/v2/scm/sal_order" },
            { "销售订单明细", "jdy/v2/scm/sal_order_detail" },
            { "销售出库单列表", "jdy/v2/scm/sal_out_bound" },
            { "销售出库明细", "jdy/v2/scm/sal_out_bound_detail" },
            { "销售汇总表", "jdy/v2/sal/sal_summary_rpt_ims" },
            { "库存查询", "jdy/v2/inv/inv_inventory_ims" }
        };

        public static string ResolvePath(string label)
        {
            if (string.IsNullOrEmpty(label)) return "api/unknown";
            string path;
            if (Map.TryGetValue(label, out path)) return path;
            return "api/" + label;
        }

        /// <summary>
        /// 接口路径 -> 按钮中文名（请求日志用）。同一路径对应多个按钮时取登记的第一个；
        /// 未登记的路径返回空串，日志里会退化显示路径本身。
        /// </summary>
        public static string ResolveLabel(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            foreach (KeyValuePair<string, string> kv in Map)
            {
                if (string.Equals(kv.Value, path, System.StringComparison.Ordinal))
                    return kv.Key;
            }
            return "";
        }
    }
}
