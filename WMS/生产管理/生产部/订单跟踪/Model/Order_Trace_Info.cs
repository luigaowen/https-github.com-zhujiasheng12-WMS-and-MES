﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.生产管理.生产部.订单跟踪.Model
{
    public class Order_Trace_Info
    {
        public string Name { get; set; }
        public List<Order_Trace_Content_Info> ContentList { get; set; }
        public bool IsOver { get; set; }
    }

    public struct Order_Trace_Content_Info
    {
        public string Content { get; set; }
        public string Person { get; set; }
        public string PlanEndTime { get; set; }
        public string EndTime { get; set; }
        public bool IsOver { get; set; }
    }

    public class DataManage
    {
        public static List<Order_Trace_Info> GetInfo(int orderId)
        {
            List<Order_Trace_Info> infos = new List<Order_Trace_Info>();

            using (JDJS_WMS_DB_USEREntities wms = new JDJS_WMS_DB_USEREntities())
            {

                var order = wms.JDJS_WMS_Order_Entry_Table.Where(r => r.Order_ID == orderId).FirstOrDefault();
                #region 市场下单
                Order_Trace_Info order_Trace_Info = new Order_Trace_Info();
                order_Trace_Info.ContentList = new List<Order_Trace_Content_Info>();
                order_Trace_Info.Name = "市场下单";
                Order_Trace_Content_Info info = new Order_Trace_Content_Info();
                info.Content = "市场下单";
                order_Trace_Info.IsOver = true;
                info.Person = order.Order_Leader;
                info.PlanEndTime = "-";
                switch (order.Intention)
                {
                    case 2:
                        if (order.AuditResult == "审核通过")
                        {
                            info.IsOver = true;
                            info.EndTime = "-";
                            var str = order.AuditAdvice.Split('\n').ToList();
                            str.Remove("");
                            if (str.Count > 0)
                            {
                                int index = str.Last().IndexOf("：");
                                info.EndTime = str.Last().Substring(0, index);

                            }

                        }
                        else
                        {
                            info.IsOver = false;

                            info.EndTime = "-";
                        }
                        break;
                    case 3:
                        if (order.AuditResult == "审核通过")
                        {
                            info.IsOver = true;

                            info.EndTime = "-";
                            var str = order.AuditAdvice.Split('\n').ToList();
                            str.Remove("");
                            if (str.Count > 0)
                            {
                                int index = str.Last().IndexOf("：");
                                info.EndTime = str.Last().Substring(0, index);

                            }
                        }
                        else
                        {
                            info.IsOver = false;

                            info.EndTime = "-";
                        }
                        break;
                    case 4:
                        if (order.AuditResult == "审核通过")
                        {
                            info.IsOver = true;

                            info.EndTime = "-";
                            var str = order.AuditAdvice.Split('\n').ToList();
                            str.Remove("");
                            if (str.Count > 0)
                            {
                                int index = str.Last().IndexOf("：");
                                info.EndTime = str.Last().Substring(0, index);

                            }
                        }
                        else
                        {
                            info.IsOver = false;

                            info.EndTime = "-";
                        }
                        break;
                    default:
                        info.IsOver = false;
                        info.EndTime = "-";
                        break;
                }
                order_Trace_Info.ContentList.Add(info);
                foreach (var item in order_Trace_Info.ContentList)
                {
                    if (!item.IsOver)
                    {
                        order_Trace_Info.IsOver = false;
                    }
                }
                infos.Add(order_Trace_Info);
                #endregion
                #region 工程处理
                Order_Trace_Info order_Trace_Info_Process = new Order_Trace_Info();
                order_Trace_Info_Process.ContentList = new List<Order_Trace_Content_Info>();
                order_Trace_Info_Process.Name = "工程处理";
                order_Trace_Info_Process.IsOver = true;
                #region 工艺规划
                Order_Trace_Content_Info info_Plan = new Order_Trace_Content_Info();
                info_Plan.Content = "工艺规划";
                info_Plan.EndTime = "-";
                info_Plan.PlanEndTime = "-";
                info_Plan.Person = order.Engine_Program_Manager == null ? "-" : order.Engine_Program_Manager;
                info_Plan.IsOver = false;
                var process = wms.JDJS_WMS_Order_Process_Info_Table.Where(r => r.OrderID == order.Order_ID && r.sign != 0).FirstOrDefault();
                if (process != null)
                {
                    if (process.sign == 1)
                    {
                        info_Plan.IsOver = true;
                    }
                }
                order_Trace_Info_Process.ContentList.Add(info_Plan);
                #endregion
                #region 工程编程
                Order_Trace_Content_Info info_Pram = new Order_Trace_Content_Info();
                info_Pram.Content = "工程编程";
                info_Pram.EndTime = "-";
                info_Pram.PlanEndTime = "-";
                info_Pram.Person = order.Engine_Program_Manager == null ? "-" : order.Engine_Program_Manager;
                info_Pram.IsOver = false;
                var guide = wms.JDJS_WMS_Order_Guide_Schedu_Table.Where(r => r.OrderID == orderId).FirstOrDefault();
                if (process != null)
                {
                    if (process.program_audit_sign == 1)
                    {
                        info_Pram.IsOver = true;
                        info_Pram.EndTime = process.ProgramePassTime == null ? "-" : process.ProgramePassTime.ToString();
                    }
                }
                if (guide != null)
                {
                    if (guide.ExpectEndTime != null)
                    {
                        info_Pram.PlanEndTime = guide.ExpectEndTime.ToString();
                    }
                }
                order_Trace_Info_Process.ContentList.Add(info_Pram);
                #endregion
                foreach (var item in order_Trace_Info_Process.ContentList)
                {
                    if (!item.IsOver)
                    {
                        order_Trace_Info_Process.IsOver = false;
                    }
                }
                infos.Add(order_Trace_Info_Process);
                #endregion  
                #region 生产准备  机台，毛坯，夹具，刀具
                Order_Trace_Info order_Trace_Prod = new Order_Trace_Info();
                order_Trace_Prod.ContentList = new List<Order_Trace_Content_Info>();
                order_Trace_Prod.Name = "生产准备";
                order_Trace_Prod.IsOver = true;
                #region 机台排产
                Order_Trace_Content_Info mach = new Order_Trace_Content_Info();
                mach.Content = "机台排产";
                mach.EndTime = "-";
                mach.PlanEndTime = "-";
                mach.IsOver = false;
                mach.Person = "-";
                var sche = wms.JDJS_WMS_Order_Process_Scheduling_Table.Where(r => r.OrderID == order.Order_ID && r.isFlag != 0);
                if (sche.Count() > 0)
                {
                    mach.IsOver = true;
                }
                order_Trace_Prod.ContentList.Add(mach);
                #endregion
                #region 毛坯准备
                Order_Trace_Content_Info blank = new Order_Trace_Content_Info();
                blank.Content = "毛坯准备";
                blank.EndTime = "";
                blank.IsOver = false;
                blank.Person = "";
                blank.PlanEndTime = "-";
                var blankDelay = wms.JDJS_WMS_Order_DelayTime_Table.Where(r => r.OrderID == order.Order_ID).FirstOrDefault();
                if (blankDelay != null)
                {
                    if (blankDelay.BlankTime != null)
                    {
                        blank.PlanEndTime = Convert.ToDateTime(blankDelay.BlankTime).ToString();
                    }
                }
                var row = wms.JDJS_WMS_Order_Blank_Table.Where(r => r.OrderID == order.Order_ID).FirstOrDefault();
                if (row != null)
                {
                    if (row.BlankState == "已完成")
                    {
                        blank.IsOver = true;
                    }
                    if (row.Expected_Completion_Time != null)
                    {
                        blank.PlanEndTime = Convert.ToDateTime(row.Expected_Completion_Time).ToString();
                    }
                }
                order_Trace_Prod.ContentList.Add(blank);

                #endregion
                #region 夹具准备
                Order_Trace_Content_Info jia = new Order_Trace_Content_Info();
                jia.Content = "夹具准备";
                jia.EndTime = "-";
                jia.IsOver = true;
                jia.Person = "-";
                jia.PlanEndTime = "-";
                if (process != null)
                {
                    if (process.Jig_Expected_Completion_Time != null)
                    {
                        jia.PlanEndTime = process.Jig_Expected_Completion_Time.ToString();
                    }
                }
                else
                {
                    jia.IsOver = false;
                }
                var processes = wms.JDJS_WMS_Order_Process_Info_Table.Where(r => r.OrderID == order.Order_ID && r.sign != 0);
                foreach (var item in processes)
                {
                    var fixtureInfo = wms.JDJS_WMS_Order_Fixture_Manager_Table.Where(r => r.ProcessID == item.ID);
                    if (fixtureInfo.Count() > 0)
                    {
                        int DeviceNum = 0;
                        int preparenum = 0;
                        if (fixtureInfo.First().FixtureNumber == null)
                        {

                            var cnc = wms.JDJS_WMS_Order_Process_Scheduling_Table.Where(r => r.ProcessID == item.ID && r.isFlag != 0);
                            List<int> cncid = new List<int>();
                            foreach (var real in cnc)
                            {
                                if (!cncid.Contains(Convert.ToInt32(real.CncID)))
                                {
                                    cncid.Add(Convert.ToInt32(real.CncID));
                                }
                            }
                            DeviceNum = cncid.Count();
                        }
                        else
                        {
                            DeviceNum = Convert.ToInt32(fixtureInfo.First().FixtureNumber);
                        }
                        var over = wms.JDJS_WMS_Fixture_Additional_History_Table.Where(r => r.ProcessID == item.ID);
                        foreach (var real in over)
                        {
                            preparenum += Convert.ToInt32(real.AddNum);
                        }
                        if (DeviceNum > preparenum)
                        {
                            jia.IsOver = false;
                        }


                    }
                }
                if (!order_Trace_Info.IsOver)
                {
                    jia.IsOver = false;
                }
                if (!order_Trace_Info_Process.IsOver)
                {
                    jia.IsOver = false;
                }
                order_Trace_Prod.ContentList.Add(jia);

                #endregion
                #region 刀具准备
                Order_Trace_Content_Info tool = new Order_Trace_Content_Info();
                tool.Content = "刀具准备";
                tool.EndTime = "-";
                tool.IsOver = true;
                tool.Person = "-";
                tool.PlanEndTime = "-";
                var toolDelay = wms.JDJS_WMS_Order_DelayTime_Table.Where(r => r.OrderID == order.Order_ID).FirstOrDefault();
                if (toolDelay != null)
                {
                    if (toolDelay.ToolTime != null)
                    {
                        tool.PlanEndTime = Convert.ToDateTime(toolDelay.ToolTime).ToString();
                    }
                }
                List<int> cncIds = new List<int>();
                foreach (var item in processes)
                {
                    if (item.toolPreparation != 1)
                    {
                        tool.IsOver = false;
                        break;
                    }
                    var cnces = wms.JDJS_WMS_Order_Process_Scheduling_Table.Where(r => r.ProcessID == item.ID && (r.isFlag == 1 || r.isFlag == 2));
                    foreach (var real in cnces)
                    {
                        if (!cncIds.Contains(Convert.ToInt32(real.CncID)))
                        {
                            cncIds.Add(Convert.ToInt32(real.CncID));
                        }
                    }
                }
                foreach (var cnc in cncIds)
                {

                    int cncID = cnc;
                    #region 是否完成
                    List<int> shankToolNums = new List<int>();
                    List<int> ToolStandInfos = new List<int>();
                    List<int> ProcessToolInfos = new List<int>();
                    Dictionary<int, string> shanlToolInfo = new Dictionary<int, string>();
                    Dictionary<int, string> ProcessNeedToolInfo = new Dictionary<int, string>();
                    {
                        var cncs = wms.JDJS_WMS_Device_Info.Where(r => r.ID == cncID);
                        var shangs = wms.JDJS_WMS_Tool_Shank_Table.Where(r => r.CncID == cncID);
                        foreach (var item in shangs)
                        {
                            var toolID = item.ToolID;
                            var sp = wms.JDJS_WMS_ToolHolder_Tool_Table.Where(r => r.ID == toolID).FirstOrDefault();
                            if (sp != null)
                            {
                                var spID = sp.ToolSpecifications;
                                var spinfo = wms.JDJS_WMS_Tool_Stock_History.Where(r => r.Id == spID).FirstOrDefault();
                                if (spinfo != null)
                                {
                                    if (!shanlToolInfo.ContainsKey(Convert.ToInt32(item.ToolNum)))
                                    {
                                        shanlToolInfo.Add(Convert.ToInt32(item.ToolNum), spinfo.KnifeName);
                                    }
                                }
                            }
                            shankToolNums.Add(Convert.ToInt32(item.ToolNum));

                        }
                        var process1 = wms.JDJS_WMS_Order_Process_Scheduling_Table.Where(r => r.CncID == cncID && r.OrderID == order.Order_ID && (r.isFlag == 1 || r.isFlag == 2)).OrderBy(r => r.StartTime).FirstOrDefault();
                        if (process1 != null)
                        {
                            int processID = Convert.ToInt32(process1.ProcessID);
                            var cncTypeID = cncs.FirstOrDefault().MachType;
                            var standTool = wms.JDJS_WMS_Tool_Standard_Table.Where(r => r.MachTypeID == cncTypeID);
                            foreach (var item in standTool)
                            {
                                var toolnumstr = item.ToolID;
                                if (toolnumstr.Length > 1)
                                {
                                    int toolNum = Convert.ToInt32(toolnumstr.Substring(1));

                                    ToolStandInfos.Add(toolNum);
                                }
                            }
                            var processTools = wms.JDJS_WMS_Order_Process_Tool_Info_Table.Where(r => r.ProcessID == processID);
                            foreach (var item in processTools)
                            {
                                if (!ToolStandInfos.Contains(Convert.ToInt32(item.ToolNO)))
                                {
                                    ProcessToolInfos.Add(Convert.ToInt32(item.ToolNO));
                                    var ToolSTR = item.ToolName;
                                    int index0 = ToolSTR.IndexOf("[");
                                    int index1 = ToolSTR.IndexOf("]");
                                    if (index1 > index0)
                                    {
                                        ToolSTR = ToolSTR.Substring(index0 + 1, index1 - index0 - 1);
                                        if (!ProcessNeedToolInfo.ContainsKey(Convert.ToInt32(item.ToolNO)))
                                        {
                                            ProcessNeedToolInfo.Add(Convert.ToInt32(item.ToolNO), ToolSTR);
                                        }
                                    }
                                }
                            }
                            foreach (var item in ProcessToolInfos)
                            {
                                if (!shankToolNums.Contains(item))
                                {
                                    tool.IsOver = false;
                                }
                            }

                        }
                    }
                    if (tool.IsOver)
                    {
                        foreach (var item in ProcessNeedToolInfo)
                        {
                            if (shanlToolInfo.ContainsKey(item.Key))
                            {
                                if (!(shanlToolInfo[item.Key].Contains(item.Value) || item.Value.Contains(shanlToolInfo[item.Key])))
                                {
                                    tool.IsOver = false;
                                    break;
                                }
                            }
                            else
                            {
                                tool.IsOver = false;
                                break;
                            }
                        }
                    }
                    #endregion


                }

                if (!order_Trace_Info.IsOver)
                {
                    tool.IsOver = false;
                }
                if (!order_Trace_Info_Process.IsOver)
                {
                    tool.IsOver = false;
                }
                order_Trace_Prod.ContentList.Add(tool);
                #endregion

                foreach (var item in order_Trace_Prod.ContentList)
                {
                    if (!item.IsOver)
                    {
                        order_Trace_Prod.IsOver = false;
                    }
                }
                infos.Add(order_Trace_Prod);
                #endregion

            }
            return infos;
        }

        public static Order_Trace_Work_Sche GetWorkInfo(int orderId)
        {
            Order_Trace_Work_Sche info = new Order_Trace_Work_Sche();
            using (JDJS_WMS_DB_USEREntities wms = new JDJS_WMS_DB_USEREntities())
            {
                info.Name = "生产进度";
                int allBlankNum = 0;
                var blank = wms.JDJS_WMS_Order_Blank_Table.Where(r => r.OrderID == orderId).FirstOrDefault();
                if (blank != null)
                {
                    allBlankNum = blank.BlanktotalPreparedNumber == null ? 0 : Convert.ToInt32(blank.BlanktotalPreparedNumber);
                }
                #region 已加工数
                int OverNum = 0;
                List<int> processID = new List<int>();
                var orderprocess = wms.JDJS_WMS_Order_Process_Info_Table.Where(r => r.OrderID == orderId && r.sign != 0);
                if (orderprocess.Count() > 0)
                {
                    foreach (var item in orderprocess)
                    {
                        allBlankNum = allBlankNum * Convert.ToInt32(item.Modulus);
                        int id = Convert.ToInt32(item.ProcessID);
                        if (!processID.Contains(id))
                        {
                            processID.Add(id);
                        }
                    }
                    int max = processID.Max();
                    var MaxProcess = wms.JDJS_WMS_Order_Process_Info_Table.Where(r => r.OrderID == orderId && r.sign != 0 && r.ProcessID == max).First();
                    var over = wms.JDJS_WMS_Order_Process_Scheduling_Table.Where(r => r.ProcessID == MaxProcess.ID && r.isFlag == 3);
                    var doing = wms.JDJS_WMS_Order_Process_Scheduling_Table.Where(r => r.ProcessID == MaxProcess.ID && r.isFlag == 2);
                    var not = wms.JDJS_WMS_Order_Process_Scheduling_Table.Where(r => r.ProcessID == MaxProcess.ID && r.isFlag == 1);
                    int doCount = 0;
                    foreach (var item in over)
                    {
                        doCount += (item.WorkCount == null ? 0 : Convert.ToInt32(item.WorkCount));

                    }
                    foreach (var item in doing)
                    {
                        doCount += (item.WorkCount == null ? 0 : Convert.ToInt32(item.WorkCount));

                    }
                    foreach (var item in not)
                    {
                        doCount += (item.WorkCount == null ? 0 : Convert.ToInt32(item.WorkCount));

                    }


                    OverNum = doCount;
                }
                info.Finish = OverNum;
                #endregion
                #region 待加工数
                info.waitNum = allBlankNum - info.Finish;
                if (info.waitNum < 0)
                {
                    info.waitNum = 0;
                }
                #endregion
                #region 良品数
                var confirm = wms.JDJS_WMS_Quality_Confirmation_Table.Where(r => r.OrderID == orderId);
                if (confirm.Count() > 0)
                {

                    if (OverNum < 1)
                    {
                        info.Good = 0;
                    }
                    else
                    {
                        info.Good = confirm.First().QualifiedProductNumber == null ? 0 : Convert.ToInt32(confirm.First().QualifiedProductNumber);
                    }

                }
                else
                {
                    info.Good = 0;
                }
                #endregion
                #region 入库数
                var product = wms.JDJS_WMS_Finished_Product_Manager.Where(r => r.OrderID == orderId).FirstOrDefault();
                if (product != null)
                {
                    info.Storage = product.warehousingNumber == null ? 0 : Convert.ToInt32(product.warehousingNumber);
                }
                #endregion 
            }
            return info;
        }
    }
}