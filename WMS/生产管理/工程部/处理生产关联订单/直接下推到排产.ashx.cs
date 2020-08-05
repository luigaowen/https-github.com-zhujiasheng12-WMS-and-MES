﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.生产管理.工程部.处理生产关联订单
{
    /// <summary>
    /// 直接下推到排产 的摘要说明
    /// </summary>
    public class 直接下推到排产 : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var orderId = int.Parse(context.Request["orderId"]);
            int oldOrderId = 0;
            using (JDJS_WMS_DB_USEREntities wms = new JDJS_WMS_DB_USEREntities())
            {
                var order = wms.JDJS_WMS_Order_Entry_Table.Where(r => r.Order_ID == orderId).FirstOrDefault();
                if (order == null)
                {
                    context.Response.Write("该订单不存在，请确认后再试！");
                    return;
                }
                if (order.ParentId == null)
                {
                    context.Response.Write("该订单无关联订单，请确认后再试！");
                    return;
                }
                using (System.Data.Entity.DbContextTransaction mytran = wms.Database.BeginTransaction())
                {
                    try
                    {
                        oldOrderId = int.Parse(order.ParentId.ToString());
                        var blankInfo = wms.JDJS_WMS_Order_Blank_Table.Where(r => r.OrderID == oldOrderId).FirstOrDefault();
                        if (blankInfo != null)
                        {
                            JDJS_WMS_Order_Blank_Table jdBlank = new JDJS_WMS_Order_Blank_Table()
                            {
                                OrderID = orderId,
                                BlackNumber = order.Product_Output,
                                BlankAddition = 0,
                                BlankSpecification = blankInfo.BlankSpecification.Contains ("#1#")? blankInfo.BlankSpecification .Replace  ("#1#","") : blankInfo.BlankSpecification,
                                BlankState = "待备料",
                                BlanktotalPreparedNumber = 0,
                                BlankType = blankInfo.BlankType,
                                Expected_Completion_Time = DateTime.Now
                            };
                            wms.JDJS_WMS_Order_Blank_Table.Add(jdBlank);
                        }

                        JDJS_WMS_Quality_Confirmation_Table jdQuality = new JDJS_WMS_Quality_Confirmation_Table()
                        {
                            OrderID = orderId,
                            DetectionNumber = 0,
                            CurrFinishedProductNumber = 0,
                            PassRate = 0,
                            PefectiveProductNumber = 0,
                            PendingNumber = 0,
                            QualifiedProductNumber = 0

                        };
                        wms.JDJS_WMS_Quality_Confirmation_Table.Add(jdQuality);
                        wms.SaveChanges();
                        JDJS_WMS_Finished_Product_Manager jdFinish = new JDJS_WMS_Finished_Product_Manager()
                        {
                            outputNumber = 0,
                            DefectiveProductNumber = 0,
                            OrderID = orderId,
                            stock = 0,
                            waitForWarehousing = 0,
                            warehousingNumber = 0,
                        };
                        wms.JDJS_WMS_Finished_Product_Manager.Add(jdFinish);
                        wms.SaveChanges();
                        var guide = wms.JDJS_WMS_Order_Guide_Schedu_Table.Where(r => r.OrderID == orderId).FirstOrDefault();
                        if (guide != null)
                        {
                            guide.ExpectEndTime = DateTime.Now;
                            guide.EndTime = DateTime.Now;
                            wms.SaveChanges();
                        }
                        var processes = wms.JDJS_WMS_Order_Process_Info_Table.Where(r => r.OrderID == oldOrderId && r.sign != 0);
                        JDJS_WMS_Order_DelayTime_Table jdDelay = new JDJS_WMS_Order_DelayTime_Table()
                        {
                            OrderID = orderId,

                        };
                        wms.JDJS_WMS_Order_DelayTime_Table.Add(jdDelay);
                        foreach (var item in processes)
                        {
                            JDJS_WMS_Order_Process_Info_Table jd = new JDJS_WMS_Order_Process_Info_Table()
                            {
                                DeviceType = item.DeviceType,
                                ProcessID = item.ProcessID,
                                JigSpecification = item.JigSpecification.Contains ("#1#")? item.JigSpecification .Replace ("#1#",""): item.JigSpecification,
                                JigType = item.JigType,
                                Jig_Expected_Completion_Time = DateTime.Now,
                                BlankNumber = order.Product_Output,
                                sign = 1,
                                program_audit_sign = 1,
                                programName = order.Order_Number + "-" + item.programName.Split('-')[1],
                                ProcessTime = item.ProcessTime,
                                BlankSpecification = item.BlankSpecification.Contains("#1#")?item.BlankSpecification.Replace ("#1#",""):item.BlankSpecification,
                                BlankType = item.BlankType,
                                MachNumber = item.MachNumber,
                                Modulus = item.Modulus,
                                NonCuttingTime = item.NonCuttingTime,
                                toolChartName = "T-" + order.Order_Number + "-" + item.toolChartName.Split('-')[2],
                                toolPreparation = 0,
                                ProgramePassTime = DateTime.Now,
                                OrderID = orderId
                            };
                            wms.JDJS_WMS_Order_Process_Info_Table.Add(jd);
                            order.audit_Result = "。" + DateTime.Now.ToString() + ":审核通过";
                            order.program_audit_result = "。" + DateTime.Now.ToString() + ":审核通过";
                            wms.SaveChanges();
                            int processId = jd.ID;
                            var processInfo = wms.JDJS_WMS_Order_Process_Information_Table.Where(r => r.ProcessID == item.ID).FirstOrDefault();
                            if (processInfo != null)
                            {
                                JDJS_WMS_Order_Process_Information_Table jdInfo = new JDJS_WMS_Order_Process_Information_Table()
                                {
                                    Note = processInfo.Note,
                                    ProcessID = processId,
                                    WorkMaterial = processInfo.WorkMaterial,
                                    XPoint = processInfo.XPoint,
                                    YPoint = processInfo.YPoint,
                                    ZPoint = processInfo.ZPoint
                                };
                                wms.JDJS_WMS_Order_Process_Information_Table.Add(jdInfo);
                                wms.SaveChanges();
                            }

                            var tools = wms.JDJS_WMS_Order_Process_Tool_Info_Table.Where(r => r.ProcessID == item.ID);
                            foreach (var tool in tools)
                            {
                                JDJS_WMS_Order_Process_Tool_Info_Table jdTool = new JDJS_WMS_Order_Process_Tool_Info_Table()
                                {
                                    BladeLenth = tool.BladeLenth,
                                    EffectiveLength = tool.EffectiveLength,
                                    ProcessID = processId,
                                    ToolDiameter = tool.ToolDiameter,
                                    isFine = tool.isFine,
                                    Shank = tool.Shank,
                                    PathName = tool.PathName,
                                    ToolAroidance = tool.ToolAroidance,
                                    ToolLength = tool.ToolLength,
                                    ToolName = tool.ToolName,
                                    ToolNO = tool.ToolNO

                                };
                                wms.JDJS_WMS_Order_Process_Tool_Info_Table.Add(jdTool);
                            }
                            wms.SaveChanges();
                            JDJS_WMS_Order_Fixture_Manager_Table jdFix = new JDJS_WMS_Order_Fixture_Manager_Table()
                            {
                                ProcessID = processId,
                                FixtureAdditionNumber = 0,
                                FixtureFinishPerpareNumber = 0,
                                FixtureNumber = 0
                            };
                            wms.JDJS_WMS_Order_Fixture_Manager_Table.Add(jdFix);
                            JDJS_WMS_Warehouse_InOut_History_Table jdInout = new JDJS_WMS_Warehouse_InOut_History_Table()
                            {
                                InNum = 0,
                                OutNum = 0,
                                ProcessId = processId
                            };
                            wms.JDJS_WMS_Warehouse_InOut_History_Table.Add(jdInout);

                        }
                        order.Intention = 3;
                        wms.SaveChanges();
                        mytran.Commit();
                        context.Response.Write("ok");
                        return;
                    }
                    catch (Exception ex)
                    {
                        mytran.Rollback();
                        context.Response.Write(ex.Message);
                        return;
                    }
                }
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