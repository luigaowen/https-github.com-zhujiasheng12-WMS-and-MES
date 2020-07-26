﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.Kanban._2020._7._24
{
    /// <summary>
    /// DownLoadTool 的摘要说明
    /// </summary>
    public class DownLoadTool : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            List<ToolInfos> infos = new List<ToolInfos>();
            using (JDJS_WMS_DB_USEREntities wms = new JDJS_WMS_DB_USEREntities())
            {
                var devcices = wms.JDJS_WMS_Device_Info;
                foreach (var cnc in devcices)
                {
                    var work = wms.JDJS_WMS_Order_Process_Scheduling_Table.Where(r => r.CncID == cnc.ID && r.isFlag == 3).OrderByDescending(r => r.EndTime).FirstOrDefault();
                    if (work != null)
                    {
                        var order = wms.JDJS_WMS_Order_Entry_Table.Where(r => r.Order_ID == work.OrderID).FirstOrDefault();
                        if (order != null)
                        {
                            if (order.Intention != 4)
                            {
                                ToolInfos info = new ToolInfos();
                                info.IsDownTool = true;
                                info.IsOver = true;
                                info.MachNum = cnc.MachNum;
                                info.OrderName = order.Product_Name;
                                info.OrderNum = order.Order_Number;
                                infos.Add(info);
                            }
                        }
                    }
                }
            }
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var json = serializer.Serialize(infos);
            context.Response.ContentType = "text/event-stream";
            context.Response.Write("data:"+json+"\n\n");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
    public struct ToolInfos
    { 
        public string MachNum { get; set; }
        public string OrderNum { get; set; }
        public string OrderName { get; set; }
        public bool IsOver { get; set; }
        public bool IsDownTool { get; set; }
    }
}