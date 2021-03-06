﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace WebApplication2.生产管理.资材部
{
    /// <summary>
    /// 备料提交 的摘要说明
    /// </summary>
    public class 备料提交 : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)

        {
            var orderId = int.Parse(context.Request["orderId"]);
            var number = int.Parse(context.Request["number"]);
            using (JDJS_WMS_DB_USEREntities entities = new JDJS_WMS_DB_USEREntities())
            {
                var order = entities.JDJS_WMS_Order_Entry_Table.Where(r => r.Order_ID == orderId).FirstOrDefault();
                if (order.ProofingORProduct != -1)
                {

                    int allblankNum = 0;
                    var process = entities.JDJS_WMS_Order_Process_Info_Table.Where(r => r.OrderID == orderId && r.ProcessID == 1 && r.sign != 0).FirstOrDefault();
                    if (process != null)
                    {
                        allblankNum = Convert.ToInt32(process.BlankNumber);
                    }
                    else
                    {
                        var rowblank = entities.JDJS_WMS_Order_Blank_Table.Where(r => r.OrderID == orderId);
                        if (rowblank.Count() > 0)
                        {
                            allblankNum = Convert.ToInt32(rowblank.First().BlackNumber);
                        }

                    }

                    var row = entities.JDJS_WMS_Order_Blank_Table.Where(r => r.OrderID == orderId);
                    if (row.Count() > 0)
                    {

                        if (row.First().BlanktotalPreparedNumber != null)
                        {
                            row.First().BlanktotalPreparedNumber += number;
                            if (row.First().BlanktotalPreparedNumber + number >= allblankNum)
                            {
                                row.First().BlankState = "已完成";
                            }
                        }
                        else
                        {

                            row.First().BlanktotalPreparedNumber = number;
                            if (number >= allblankNum)
                            {
                                row.First().BlankState = "已完成";
                            }
                        }
                    }
                    entities.SaveChanges();
                    JDJS_WMS_Blank_Additional_History_Table blankAdd = new JDJS_WMS_Blank_Additional_History_Table()
                    {
                        OrderID = orderId,
                        BlankAddNum = number,
                        AddTime = DateTime.Now
                    };
                    entities.JDJS_WMS_Blank_Additional_History_Table.Add(blankAdd);//添加到毛坯添加历史记录表中
                    entities.SaveChanges();
                    PathInfo path = new PathInfo();
                    string damaMachName = @path.DaMaMachName();
                    damaMachName = damaMachName.Replace("\\\\", "++++");
                    damaMachName = damaMachName.Replace("\\", @"\");
                    damaMachName = damaMachName.Replace("++++", "\\");
                    using (System.Data.Entity.DbContextTransaction mytran = entities.Database.BeginTransaction())
                    {
                        try
                        {
                            var orderInfo = entities.JDJS_WMS_Order_Entry_Table.Where(r => r.Order_ID == orderId).FirstOrDefault();
                            if (orderInfo != null)
                            {
                                string orderNum = orderInfo.Order_Number;
                                var blankInfo = entities.JDJS_WMS_Blank_Table.Where(r => r.OrderID == orderId);
                                int blankCount = blankInfo.Count();
                                for (int i = 0; i < number; i++)
                                {
                                    JDJS_WMS_Blank_Table blank = new JDJS_WMS_Blank_Table()
                                    {
                                        OrderID = orderId,
                                        flag = 0,
                                        Num = blankCount + i + 1,
                                    };
                                    entities.JDJS_WMS_Blank_Table.Add(blank);

                                    string num = (blankCount + i + 1).ToString();
                                    while (num.Length < 4)
                                    {
                                        num = num.Insert(0, "0");
                                    }
                                    //Show the DLL version
                                    TSCLIB_DLL.openport(damaMachName); //注意修改打印机名称，需要和资材部电脑连接的打码机一致                                          //Open specified printer driver
                                    TSCLIB_DLL.setup("40", "10", "4", "8", "0", "2", "0");        //设置标签大小格式                   //Setup the media size and sensor type info
                                    TSCLIB_DLL.clearbuffer();                                                           //Clear image buffer
                                    TSCLIB_DLL.windowsfont(15, 5, 20, 0, 0, 0, "宋体", "订单号:" + orderNum + " " + "状态:毛坯");  //Draw windows font
                                    TSCLIB_DLL.barcode("20", "30", "128", "45", "1", "0", "2", "2", orderNum + num + "00"); //Drawing barcode
                                    TSCLIB_DLL.printlabel("1", "1");                                                    //Print labels
                                    TSCLIB_DLL.closeport();


                                }
                            }
                            entities.SaveChanges();
                            mytran.Commit();
                        }
                        catch (Exception ex)
                        {
                            mytran.Rollback();
                            context.Response.Write(ex.Message);
                            return;
                        }
                    }
                }
                else
                {
                    int allblankNum = 0;
                    var process = entities.JDJS_WMS_Order_Process_Info_Table.Where(r => r.OrderID == orderId && r.ProcessID == 1 && r.sign != 0).FirstOrDefault();
                    if (process != null)
                    {
                        allblankNum = Convert.ToInt32(process.BlankNumber);
                    }
                    else
                    {
                        var rowblank = entities.JDJS_WMS_Order_Blank_Table.Where(r => r.OrderID == orderId);
                        if (rowblank.Count() > 0)
                        {
                            allblankNum = Convert.ToInt32(rowblank.First().BlackNumber);
                        }

                    }

                    var row = entities.JDJS_WMS_Order_Blank_Table.Where(r => r.OrderID == orderId);
                    if (row.Count() > 0)
                    {

                        if (row.First().BlanktotalPreparedNumber != null)
                        {
                            row.First().BlanktotalPreparedNumber += number;
                            if (row.First().BlanktotalPreparedNumber + number >= allblankNum)
                            {
                                row.First().BlankState = "已完成";
                            }
                        }
                        else
                        {

                            row.First().BlanktotalPreparedNumber = number;
                            if (number >= allblankNum)
                            {
                                row.First().BlankState = "已完成";
                            }
                        }
                    }
                    entities.SaveChanges();
                    JDJS_WMS_Blank_Additional_History_Table blankAdd = new JDJS_WMS_Blank_Additional_History_Table()
                    {
                        OrderID = orderId,
                        BlankAddNum = number,
                        AddTime = DateTime.Now
                    };
                    entities.JDJS_WMS_Blank_Additional_History_Table.Add(blankAdd);//添加到毛坯添加历史记录表中
                    entities.SaveChanges();
                    PathInfo path = new PathInfo();
                    string damaMachName = @path.DaMaMachName();
                    damaMachName = damaMachName.Replace("\\\\", "++++");
                    damaMachName = damaMachName.Replace("\\", @"\");
                    damaMachName = damaMachName.Replace("++++", "\\");
                    using (System.Data.Entity.DbContextTransaction mytran = entities.Database.BeginTransaction())
                    {
                        try
                        {
                            var orderInfo = entities.JDJS_WMS_Order_Entry_Table.Where(r => r.Order_ID == orderId).FirstOrDefault();
                            if (orderInfo != null)
                            {
                                string orderNum = orderInfo.Order_Number;
                                var blankInfo = entities.JDJS_WMS_Blank_Table.Where(r => r.OrderID == orderId);
                                int blankCount = blankInfo.Count();
                                if (blankCount > 0)
                                { }
                                else
                                {
                                    for (int i = 0; i < 1; i++)
                                    {
                                        JDJS_WMS_Blank_Table blank = new JDJS_WMS_Blank_Table()
                                        {
                                            OrderID = orderId,
                                            flag = 0,
                                            Num = blankCount + i + 1,

                                        };
                                        entities.JDJS_WMS_Blank_Table.Add(blank);

                                        //string num = (blankCount + i + 1).ToString();
                                        //while (num.Length < 4)
                                        //{
                                        //    num = num.Insert(0, "0");
                                        //}
                                        ////Show the DLL version
                                        //TSCLIB_DLL.openport(damaMachName); //注意修改打印机名称，需要和资材部电脑连接的打码机一致                                          //Open specified printer driver
                                        //TSCLIB_DLL.setup("40", "10", "4", "8", "0", "2", "0");        //设置标签大小格式                   //Setup the media size and sensor type info
                                        //TSCLIB_DLL.clearbuffer();                                                           //Clear image buffer
                                        //TSCLIB_DLL.windowsfont(15, 5, 20, 0, 0, 0, "宋体", "订单号:" + orderNum + " " + "状态:毛坯");  //Draw windows font
                                        //TSCLIB_DLL.barcode("20", "30", "128", "45", "1", "0", "2", "2", orderNum + num + "00"); //Drawing barcode
                                        //TSCLIB_DLL.printlabel("1", "1");                                                    //Print labels
                                        //TSCLIB_DLL.closeport();


                                    }
                                }
                            }
                            entities.SaveChanges();
                            mytran.Commit();
                        }
                        catch (Exception ex)
                        {
                            mytran.Rollback();
                            context.Response.Write(ex.Message);
                            return;
                        }
                    }
                }

                context.Response.Write("ok");
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

    public class TSCLIB_DLL
    {
        [DllImport("TSCLIB.dll", EntryPoint = "about")]
        public static extern int about();

        [DllImport("TSCLIB.dll", EntryPoint = "openport")]
        public static extern int openport(string printername);

        [DllImport("TSCLIB.dll", EntryPoint = "barcode")]
        public static extern int barcode(string x, string y, string type,
                    string height, string readable, string rotation,
                    string narrow, string wide, string code);

        [DllImport("TSCLIB.dll", EntryPoint = "clearbuffer")]
        public static extern int clearbuffer();

        [DllImport("TSCLIB.dll", EntryPoint = "closeport")]
        public static extern int closeport();

        [DllImport("TSCLIB.dll", EntryPoint = "downloadpcx")]
        public static extern int downloadpcx(string filename, string image_name);

        [DllImport("TSCLIB.dll", EntryPoint = "formfeed")]
        public static extern int formfeed();

        [DllImport("TSCLIB.dll", EntryPoint = "nobackfeed")]
        public static extern int nobackfeed();

        [DllImport("TSCLIB.dll", EntryPoint = "printerfont")]
        public static extern int printerfont(string x, string y, string fonttype,
                        string rotation, string xmul, string ymul,
                        string text);

        [DllImport("TSCLIB.dll", EntryPoint = "printlabel")]
        public static extern int printlabel(string set, string copy);

        [DllImport("TSCLIB.dll", EntryPoint = "sendcommand")]
        public static extern int sendcommand(string printercommand);

        [DllImport("TSCLIB.dll", EntryPoint = "setup")]
        public static extern int setup(string width, string height,
                  string speed, string density,
                  string sensor, string vertical,
                  string offset);

        [DllImport("TSCLIB.dll", EntryPoint = "windowsfont")]
        public static extern int windowsfont(int x, int y, int fontheight,
                        int rotation, int fontstyle, int fontunderline,
                        string szFaceName, string content);

    }
}