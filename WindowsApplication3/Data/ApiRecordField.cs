namespace WindowsApplication3.Data
{
    /// <summary>
    /// 动态字段明细表（EAV）：把任意接口返回的JSON字段逐个拍平存行，
    /// 新增接口/新增字段都无需改表结构，实现“动态保存”。
    /// </summary>
    public class ApiRecordField
    {
        public int Id { get; set; }

        /// <summary>所属记录（api_record.id）</summary>
        public int RecordId { get; set; }

        /// <summary>字段名，嵌套字段用 a.b、数组用 a[0].b 形式</summary>
        public string FieldName { get; set; }

        /// <summary>字段值（统一转字符串保存；对象/数组保存其JSON文本）</summary>
        public string FieldValue { get; set; }

        /// <summary>值类型：string/number/bool/null/object/array</summary>
        public string FieldType { get; set; }

        public virtual ApiRecord Record { get; set; }
    }
}
