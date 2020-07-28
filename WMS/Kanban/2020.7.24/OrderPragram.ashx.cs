﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.Kanban._2020._7._24
{
    /// <summary>
    /// OrderPragram 的摘要说明
    /// </summary>
    public class OrderPragram : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            List<Info> infos = new List<Info>();
            using (JDJS_WMS_DB_USEREntities wms = new JDJS_WMS_DB_USEREntities())
            {
                var orders = wms.JDJS_WMS_Order_Entry_Table.Where(r => r.Intention == 3 && r.AuditResult == "审核通过");
                foreach (var order in orders)
                {
                    Info info = new Info();
                    info.OrderNum = order.Order_Number;
                    info.ProjectName = order.ProjectName;
                    info.Time = "-";
                    info.time = DateTime.Now.AddYears(-1);
                    var guide = wms.JDJS_WMS_Order_Guide_Schedu_Table.Where(r => r.OrderID == order.Order_ID).FirstOrDefault();
                    if (guide != null)
                    {
                        info.Time = guide.ExpectEndTime==null?"-": "预计:"+guide.ExpectEndTime.ToString();
                        info.time = guide.ExpectEndTime == null ? DateTime.Now.AddYears(-1) : Convert.ToDateTime ( guide.ExpectEndTime);
                    }
                    
                    var processes = wms.JDJS_WMS_Order_Process_Info_Table.Where(r => r.OrderID == order.Order_ID&&r.sign !=0).FirstOrDefault ();
                    if (processes != null)
                    {
                        if (processes.program_audit_sign == 1)
                        {
                            if (processes.ProgramePassTime != null)
                            {
                                info.Time = processes.ProgramePassTime == null ? "-" : "实际:" + processes.ProgramePassTime.ToString();
                                info.time = processes.ProgramePassTime == null ? DateTime.Now.AddYears(-1) : Convert.ToDateTime(processes.ProgramePassTime);
                            }
                            var sche = wms.JDJS_WMS_Order_Process_Scheduling_Table.Where(r => r.ProcessID == processes.ID).FirstOrDefault();
                            if (sche != null)
                            {
                                continue;
                            }
                        }
                    }
                    infos.Add(info);

                }
            }
            infos = infos.OrderBy(r => r.time).ToList ();
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var json = serializer.Serialize(infos);
            context.Response.ContentType = "text/event-stream";
            context.Response.Write("data:" + json + "\n\n");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }

    public struct Info
    { 
        public string OrderNum { get; set; }
        public string ProjectName { get; set; }
        public string Time { get; set; }
        public DateTime time { get; set; }
    }
}