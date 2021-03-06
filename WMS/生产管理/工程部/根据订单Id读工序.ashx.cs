﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.生产管理.工程部
{
    /// <summary>
    /// 根据订单Id读工序 的摘要说明
    /// </summary>
    public class 根据订单Id读工序 : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var orderId = int.Parse(context.Request["orderId"]);
            using(JDJS_WMS_DB_USEREntities entities=new JDJS_WMS_DB_USEREntities())
            {
                var rows = entities.JDJS_WMS_Order_Process_Info_Table.Where(r => r.OrderID == orderId&r.sign !=0);
                List<ProcessInfo> processInfos = new List<ProcessInfo>();

                foreach (var item in rows)
                {
                    processInfos.Add(new ProcessInfo { processId = item.ID.ToString(), processNum = item.ProcessID.ToString() });
                }
                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var json = serializer.Serialize(processInfos);
                context.Response.Write(json);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
    class ProcessInfo
    {
        public string processId;
        public string processNum;
    }
}