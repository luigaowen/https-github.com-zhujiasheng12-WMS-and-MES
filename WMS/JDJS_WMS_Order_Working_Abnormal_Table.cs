//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebApplication2
{
    using System;
    using System.Collections.Generic;
    
    public partial class JDJS_WMS_Order_Working_Abnormal_Table
    {
        public int ID { get; set; }
        public Nullable<int> OrderId { get; set; }
        public Nullable<int> ProcessId { get; set; }
        public string Type { get; set; }
        public string Remark { get; set; }
        public Nullable<int> AbnormalPersonId { get; set; }
        public string AbnormalPersonName { get; set; }
        public string AuditState { get; set; }
        public string AuditRemark { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public Nullable<System.DateTime> AlterTime { get; set; }
        public Nullable<System.DateTime> AuditTime { get; set; }
    }
}
