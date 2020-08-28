﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.生产管理.资材部
{
    /// <summary>
    /// 治具管理读数据 的摘要说明
    /// </summary>
    public class 治具管理读数据 : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var page = int.Parse(context.Request["page"]);
            var limit = int.Parse(context.Request["limit"]);

            {
                List<FixtureRead> fixtureReads = new List<FixtureRead>();
                using (JDJS_WMS_DB_USEREntities wms = new JDJS_WMS_DB_USEREntities())
                {
                    var orders = wms.JDJS_WMS_Order_Entry_Table.Where(r => r.Intention == 4| r.Intention == 2| r.Intention == 3);
                    foreach (var order in orders)
                    {
                        var processInfos = wms.JDJS_WMS_Order_Process_Info_Table.Where(r => r.OrderID == order.Order_ID && r.sign != 0);
                        foreach (var item in processInfos)
                        {
                            if (!item.JigSpecification.Contains("#1#"))
                            {
                                FixtureRead fixture = new FixtureRead();
                                fixture.projectName = order.ProjectName == null ? "" : order.ProjectName;
                                fixture.orderName = order.Product_Name;
                                fixture.order = order.Order_Number.ToString();
                                fixture.process = item.ProcessID.ToString();
                                fixture.processId = item.ID.ToString();
                                fixture.processId1 = item.ID.ToString();
                                fixture.Jig_Expected_Completion_Time = item.Jig_Expected_Completion_Time.ToString();


                                int fixID = Convert.ToInt32(item.JigType);
                                string fixName = "";
                                var fixTable = wms.JDJS_WMS_Device_Status_Table.Where(r => r.ID == fixID).FirstOrDefault();
                                if (fixTable != null)
                                {
                                    fixName = fixTable.Status + "(" + fixTable.explain + ")";
                                    fixture.Jia_System_Id = fixTable.SystemId == null ? 0 : Convert.ToInt32(fixTable.SystemId);
                                }
                                fixture.jigType = fixName;
                                fixture.jigSpecification = item.JigSpecification;
                                var times = wms.JDJS_WMS_Order_DelayTime_Table.Where(r => r.OrderID == item.OrderID);
                                if (times.Count() > 0)
                                {
                                    fixture.time = times.FirstOrDefault().Fixture.ToString();
                                }

                                var fixtureInfo = wms.JDJS_WMS_Order_Fixture_Manager_Table.Where(r => r.ProcessID == item.ID);
                                if (fixtureInfo.Count() > 0)
                                {
                                    int DeviceNum = 0;
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
                                    fixture.jigDemandNumber = DeviceNum.ToString();
                                    fixture.jigPreparedNumber = fixtureInfo.First().FixtureFinishPerpareNumber.ToString();
                                    if (Convert.ToInt32(fixtureInfo.First().FixtureNumber) > Convert.ToInt32(fixtureInfo.First().FixtureFinishPerpareNumber))
                                    {
                                        fixture.jigPrepareState = "待备";
                                    }
                                    else
                                    {
                                        fixture.jigPrepareState = "已备";
                                    }
                                    fixture.jigAppendNumber = fixtureInfo.First().FixtureAdditionNumber.ToString();
                                }
                                fixtureReads.Add(fixture);
                            }
                        }

                    }
                }
                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                //var layPage = fixtureReads.Skip((page - 1) * limit).Take(limit);
                var model = new { code = 0, data = fixtureReads, count = fixtureReads.Count };

                var json = serializer.Serialize(model);
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
    public class FixtureRead
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public string order;
        /// <summary>
        /// 项目名称
        /// </summary>
        public string projectName;
        /// <summary>
        /// 产品名称
        /// </summary>
        public string orderName;
        /// <summary>
        /// 工序号
        /// </summary>
        public string process;
        /// <summary>
        /// 治具种类
        /// </summary>
        public string jigType;
        /// <summary>
        /// 治具规格
        /// </summary>
        public string jigSpecification;
        /// <summary>
        /// 治具需求数量
        /// </summary>
        public string jigDemandNumber;
        /// <summary>
        /// 治具准备状态
        /// </summary>
        public string jigPrepareState;
        /// <summary>
        /// 治具已准备量
        /// </summary>
        public string jigPreparedNumber;
        /// <summary>
        /// 治具追加量
        /// </summary>
        public string jigAppendNumber;
        /// <summary>
        /// 工序主键ID
        /// </summary>
        public string processId;
        public string processId1;
        public string time;
        public string Jig_Expected_Completion_Time;
        public int Jia_System_Id;
    }
}