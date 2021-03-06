﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace WebApplication2.Model.生产管理.工程部
{
    /// <summary>
    /// createWorkNumber 的摘要说明
    /// </summary>
    public class editWorkNumber : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            try
            {

           
            var form = context.Request.Form;
           
            
            var workNumber = int.Parse(form["workNumber"]);
                var coefficient = int.Parse(form["coefficient"]);
                var blankType = int.Parse(form["blankType"]);
            var blankSpecification = form["blankSpecification"];
            var blankNumber =int.Parse(form["blankNumber"]);
            var fixtureType = int.Parse(form["fixtureType"]);
            var jigSpecification = form["jigSpecification"];
            var jigNumber = int.Parse(form["jigNumber"]);
            var machType = int.Parse(form["machType"]);
            var processId = int.Parse(form["processId"]);
            
            using (JDJS_WMS_DB_USEREntities entities=new JDJS_WMS_DB_USEREntities())
            {
                var orderId = entities.JDJS_WMS_Order_Process_Info_Table.Where(r => r.ID == processId).First().OrderID;
                var rows = entities.JDJS_WMS_Order_Process_Info_Table.Where(r => r.ProcessID == workNumber&r.OrderID==orderId&r.sign==1);
                if (rows.Count() > 0)
                {
                    foreach (var item in rows)
                    {
                        if (item.ID != processId)
                        {
                            context.Response.Write("该工序号已存在");
                            return;
                        }
                    }
                }
                var row = entities.JDJS_WMS_Order_Process_Info_Table.Where(r => r.ID == processId).First();
                if (row.ProcessID != workNumber)
                {
                    context.Response.Write("工序号不可更改");
                    return;
                    var orderNumber = entities.JDJS_WMS_Order_Entry_Table.Where(r => r.Order_ID == row.OrderID).First().Order_Number;
                    var extensionP = Path.GetExtension(row.programName);
                    var newPName= orderNumber + "-P" + workNumber + extensionP;
                    PathInfo pathInfo = new PathInfo();
                    var pathP = Path.Combine(pathInfo.upLoadPath(), orderNumber,"加工文件", row.programName);
                    var pathPLater= Path.Combine(pathInfo.upLoadPath(), orderNumber, "加工文件",newPName);
                    
                    File.Move(pathP, pathPLater);
                    row.programName = newPName;

                    var extensionT = Path.GetExtension(row.toolChartName);
                    var newTName = orderNumber + "-T" + workNumber + extensionT;
                   
                    var pathT = Path.Combine(pathInfo.upLoadPath(), orderNumber,"刀具表", row.toolChartName);
                    var pathTLater = Path.Combine(pathInfo.upLoadPath(), orderNumber, "刀具表", newTName);
                    FileInfo fileT = new FileInfo(pathT);
                    fileT.MoveTo(pathTLater);

                    row.toolChartName = newTName;
                }
                    if (row.sign == 1)
                    {
                        context.Response.Write("工艺审核已通过，不可修改！");
                        return;
                    }
                row.ProcessID = workNumber;
              
                row.BlankType = blankType;
                row.BlankSpecification = blankSpecification+"#1#";
                    row.Modulus = coefficient;
                row.DeviceType = machType;
                row.BlankNumber = blankNumber;
                entities.JDJS_WMS_Order_Blank_Table.Where(r => r.OrderID == orderId).First().BlackNumber = blankNumber;
                row.JigType = fixtureType;
                row.JigSpecification = jigSpecification + "#1#";
                    var blank = entities.JDJS_WMS_Order_Blank_Table.Where(r => r.OrderID == orderId);
                    if (blank.Count() > 0)
                    {
                        foreach (var item in blank)
                        {
                            item.BlackNumber = blankNumber;
                            item.BlankSpecification = blankSpecification + "#1#";
                            item.BlankState = "待备料";
                            item.BlankType = blankType;
                            
                        }
                    }
                    else
                    {
                        JDJS_WMS_Order_Blank_Table jDJS_WMS_Order_Blank_Table = new JDJS_WMS_Order_Blank_Table()
                        {
                            BlackNumber =blankNumber ,
                            BlankAddition =0,
                            BlankSpecification =blankSpecification +"#1#",
                            BlankState ="待备料",
                            BlanktotalPreparedNumber=0,
                            BlankType =blankType ,
                            OrderID =orderId 
                        };
                        entities.JDJS_WMS_Order_Blank_Table.Add(jDJS_WMS_Order_Blank_Table);
                    }
                var fixs = entities.JDJS_WMS_Order_Fixture_Manager_Table.Where(r => r.ProcessID == processId).FirstOrDefault();
                if (fixs != null)
                {
                    entities.JDJS_WMS_Order_Fixture_Manager_Table.Where(r => r.ProcessID == processId).FirstOrDefault().FixtureNumber = jigNumber;
                }
                else
                {
                    
                    int id = processId;
                    var orderNum = entities.JDJS_WMS_Order_Entry_Table.Where(r => r.Order_ID == orderId);
                    JDJS_WMS_Order_Fixture_Manager_Table fix = new JDJS_WMS_Order_Fixture_Manager_Table()
                    {
                        ProcessID = id,
                        FixtureNumber = jigNumber,
                        FixtureFinishPerpareNumber = 0,
                        FixtureAdditionNumber = 0

                    };
                    entities.JDJS_WMS_Order_Fixture_Manager_Table.Add(fix);
                    entities.SaveChanges();
                }

               
                entities.SaveChanges();
                context.Response.Write("ok");
            }
            }
            catch (Exception ex)
            {
                context.Response.Write(ex.Message);
                return;
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