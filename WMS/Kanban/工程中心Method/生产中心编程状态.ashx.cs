﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.Kanban.工程中心Method
{
    /// <summary>
    /// 生产中心编程状态 的摘要说明
    /// </summary>
    public class 生产中心编程状态 : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            int noAssessment = 0;
            int hasAssessed = 0;
            int underWay = 0;
            try
            {
                using (JDJS_WMS_DB_USEREntities wms = new JDJS_WMS_DB_USEREntities())
                {
                    var noOrders = wms.JDJS_WMS_Order_Entry_Table.Where(r => r.Engine_Program_ManagerId == null && (r.Intention == 3 || r.Intention == 2));
                    noAssessment = noOrders.Count();
                    var orders = wms.JDJS_WMS_Order_Entry_Table.Where(r => r.Engine_Program_ManagerId != null && (r.Intention == 3 || r.Intention == 2));
                    foreach (var order in orders)
                    {
                        int orderID = order.Order_ID;
                        //var processes = wms.JDJS_WMS_Order_Process_Info_Table.Where(r => r.OrderID == orderID && r.sign != 0);
                        var processing = wms.JDJS_WMS_Order_Process_Info_Table.Where(r => r.OrderID == orderID && r.sign != 0 && r.ProgramePassTime ==null);
                        //if (processes.Count() < 1)
                        //{
                        //    underWay++;
                        //    continue;
                        //}
                        
                        if (processing.Count() > 0)
                        {
                            underWay++;
                            continue;
                        }
                        var processed = wms.JDJS_WMS_Order_Process_Info_Table.Where(r => r.OrderID == orderID && r.sign == 1);
                        if (processing.Count() < 1 && processed.Count() > 0)
                        {
                            if (processed.First().ProgramePassTime != null && Convert.ToDateTime(processed.First().ProgramePassTime)>DateTime .Now.AddDays (-7))
                            hasAssessed++;
                            continue;
                        }
                        if (processing.Count() < 1 && processed.Count() < 1)
                        {
                            underWay++;
                            continue;
                        }
                    }
                }
                var mode = new
                {
                    ///未编程
                    noAssessment = noAssessment,
                    ///编程中
                    underWay = underWay,
                    ///已编程
                    hasAssessed = hasAssessed
                };
                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var json = serializer.Serialize(mode);
                context.Response.Write("data:"+json+"\n\n");
                context.Response.ContentType = "text/event-stream";
            }
            catch (Exception ex)
            {
                context.Response.Write(ex.Message );
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
}