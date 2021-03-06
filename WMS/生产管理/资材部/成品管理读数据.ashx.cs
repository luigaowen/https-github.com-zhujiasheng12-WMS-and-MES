﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.生产管理.资材部
{
    /// <summary>
    /// 成品管理读数据 的摘要说明
    /// </summary>
    public class 成品管理读数据 : IHttpHandler
    {
        System.Web.Script.Serialization.JavaScriptSerializer Serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

        public void ProcessRequest(HttpContext context)
        {
            var page = int.Parse(context.Request["page"]);
            var limit = int.Parse(context.Request["limit"]);
            {
                List<FinishedProductManagement> products = new List<FinishedProductManagement>();
                using (JDJS_WMS_DB_USEREntities  wms = new JDJS_WMS_DB_USEREntities ())
                {
                    var orders = wms.JDJS_WMS_Order_Entry_Table.Where(r => r.Intention == 4 | r.Intention == 2 | r.Intention == 3);
                    foreach (var order in orders)
                    {
                        FinishedProductManagement finishedProductManagement = new FinishedProductManagement();
                        finishedProductManagement.projectName = order.ProjectName == null ? "" : order.ProjectName;
                        finishedProductManagement.orderName = order.Product_Name;
                        finishedProductManagement.order = order.Order_Number;
                        finishedProductManagement.orderId = order.Order_ID.ToString();
                        finishedProductManagement.orderDemandNumber = order.Product_Output.ToString();
                        var product = wms.JDJS_WMS_Finished_Product_Manager.Where(r => r.OrderID == order.Order_ID);
                        var wait = wms.JDJS_WMS_Quality_Confirmation_Table.Where(r => r.OrderID == order.Order_ID);
                        if (product.Count() < 1 || wait.Count() < 1)
                        {

                            if (wait.Count() < 1 && product.Count() > 0)//有成品表没有品质确认表
                            {
                                int OverNum = 0;
                                List<int> processID = new List<int>();
                                var orderprocess = wms.JDJS_WMS_Order_Process_Info_Table.Where(r => r.OrderID == order.Order_ID && r.sign != 0);
                                if (orderprocess.Count() > 0)
                                {
                                    foreach (var item in orderprocess)
                                    {
                                        int id = Convert.ToInt32(item.ProcessID);
                                        if (!processID.Contains(id))
                                        {
                                            processID.Add(id);
                                        }
                                    }
                                    int max = processID.Max();
                                    var MaxProcess = wms.JDJS_WMS_Order_Process_Info_Table.Where(r => r.OrderID == order.Order_ID && r.sign != 0 && r.ProcessID == max).First();
                                    var over = wms.JDJS_WMS_Order_Process_Scheduling_Table.Where(r => r.ProcessID == MaxProcess.ID && r.isFlag == 3);
                                    OverNum = over.Count();
                                }
                                finishedProductManagement.outputNumber = product.First().outputNumber.ToString();
                                finishedProductManagement.outputTime = product.First().outputTime.ToString();
                                finishedProductManagement.qualifiedProductNumber = "0";
                                finishedProductManagement.rejectNumber = "0";
                                finishedProductManagement.stock = product.First().stock.ToString();
                                finishedProductManagement.waitForWarehousing = OverNum.ToString();
                                finishedProductManagement.warehousingNumber = product.First().warehousingNumber.ToString();
                                finishedProductManagement.warehousingTime = product.First().warehousingTime.ToString();


                            }
                            if (product.Count() < 1 && wait.Count() > 0)//有品质确认表无成品表
                            {
                                finishedProductManagement.outputNumber = "0";
                                finishedProductManagement.outputTime = "0";
                                finishedProductManagement.qualifiedProductNumber = wait.First().QualifiedProductNumber.ToString();
                                finishedProductManagement.rejectNumber = wait.First().PefectiveProductNumber.ToString();
                                finishedProductManagement.stock = "0";
                                finishedProductManagement.waitForWarehousing = wait.First().QualifiedProductNumber.ToString();
                                finishedProductManagement.warehousingNumber = "0";
                                finishedProductManagement.warehousingTime = "0";

                            }
                            if (product.Count() < 1 && wait.Count() < 1)
                            {
                                finishedProductManagement.outputNumber = "0";
                                finishedProductManagement.outputTime = "0";
                                finishedProductManagement.qualifiedProductNumber = "0";
                                finishedProductManagement.rejectNumber = "0";
                                finishedProductManagement.stock = "0";
                                finishedProductManagement.waitForWarehousing = "0";
                                finishedProductManagement.warehousingNumber = "0";
                                finishedProductManagement.warehousingTime = "0";
                            }


                        }
                        else
                        {
                            var prodect =
                                          from confim in wms.JDJS_WMS_Quality_Confirmation_Table
                                          from manage in wms.JDJS_WMS_Finished_Product_Manager
                                          where confim.OrderID == order.Order_ID && manage.OrderID == order.Order_ID
                                          select new
                                          {
                                              confim.QualifiedProductNumber,
                                              confim.PefectiveProductNumber,
                                              manage.waitForWarehousing,
                                              manage.warehousingNumber,
                                              manage.warehousingTime,
                                              manage.outputNumber,
                                              manage.outputTime,
                                              manage.stock
                                          };
                            if (prodect.Count() > 0)
                            {
                                finishedProductManagement.outputNumber = prodect.First().outputNumber.ToString();
                                finishedProductManagement.outputTime = prodect.First().outputTime.ToString();
                                finishedProductManagement.qualifiedProductNumber = prodect.First().QualifiedProductNumber.ToString();
                                finishedProductManagement.rejectNumber = prodect.First().PefectiveProductNumber.ToString();
                                finishedProductManagement.stock = prodect.First().stock.ToString();
                                finishedProductManagement.waitForWarehousing = prodect.First().waitForWarehousing.ToString();
                                finishedProductManagement.warehousingNumber = prodect.First().warehousingNumber.ToString();
                                finishedProductManagement.warehousingTime = prodect.First().warehousingTime.ToString();
                            }
                        }
                        products.Add(finishedProductManagement);
                    }
                }
                //var layPage = products.Skip((page - 1) * limit).Take(limit);
                var model = new { code = 0, data = products, count = products.Count };
                var json = Serializer.Serialize(model);
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
    class FinishedProductManagement
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
        /// 订单需求量
        /// </summary>
        public string orderDemandNumber;
        /// <summary>
        /// 当前合格品数
        /// </summary>
        public string qualifiedProductNumber;
        /// <summary>
        /// 当前不良品数
        /// </summary>
        public string rejectNumber;
        /// <summary>
        /// 待入库
        /// </summary>
        public string waitForWarehousing;
        /// <summary>
        /// 入库数
        /// </summary>
        public string warehousingNumber;
        /// <summary>
        /// 入库时间
        /// </summary>
        public string warehousingTime;
        /// <summary>
        /// 出库数
        /// </summary>
        public string outputNumber;
        /// <summary>
        /// 库存
        /// </summary>
        public string stock;

        /// <summary>
        /// 出库时间
        /// </summary>
        public string outputTime;
        /// <summary>
        /// 订单Id（主键）
        /// </summary>
            public string orderId;
    }
}