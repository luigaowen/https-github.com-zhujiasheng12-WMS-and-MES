﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.OleDb;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
//using Microsoft.Office.Interop.Excel;

namespace WebApplication2
{
    class Function
    {
         ////<summary>
         ////解析固定刀具表
         ////</summary>
         ////<param name="args"></param>
      //public string ImportExcel(int cncTypeId,string path1)
      //  {

      //      //解析excel文件
      //      int MachTypeID = cncTypeId;
      //      string path = path1;
      //      string fileSuffix = Path.GetExtension(path);
      //      if (fileSuffix != ".xls" && fileSuffix != ".xlsx")
      //      {
      //          return("请输入xls或xlsx格式文件！");
               
      //      }
      //      var data = (path, 1);
      //      var x = Convert.ToInt32(data.GetLongLength(0));
      //      var y = Convert.ToInt32(data.GetLongLength(1));
      //      int arrayx = 0;
      //      int arrayy = 0;
      //      var dataArray = new string[x, y];
      //      foreach (var item in data)
      //      {
      //          if (item != null)
      //          {
      //              dataArray[arrayx, arrayy] = item.ToString();
      //          }
      //          arrayy++;
      //          if (arrayy >= y)
      //          {
      //              arrayx++;
      //              arrayy = 0;
      //          }
      //      }
      //      using (JDJS_WMS_DB_USEREntities wms = new JDJS_WMS_DB_USEREntities())
      //      {
      //          using (System.Data.Entity.DbContextTransaction mytran = wms.Database.BeginTransaction())
      //          {
      //              try
      //              {
      //                  List<ToolInfo> toolInfos = new List<ToolInfo>();
      //                  for (int i = 2; i < x; i++)
      //                  {
      //                      ToolInfo toolInfo = new ToolInfo();
      //                      double a = 0;
      //                      if (dataArray[i, 6] != null && isNumberic(dataArray[i, 6], out a))
      //                      {
      //                          toolInfo.Feed = Convert.ToDouble(dataArray[i, 6]);
      //                      }
      //                      toolInfo.Knife = 0;
      //                      toolInfo.MachTypeID = MachTypeID;
      //                      toolInfo.ProcessStage = "精";
      //                      if (dataArray[i, 5] != null && isNumberic(dataArray[i, 5], out a))
      //                      {
      //                          toolInfo.RotatingSpeed = Convert.ToDouble(dataArray[i, 5]);
      //                      }
      //                      toolInfo.Shank = dataArray[i, 1];
      //                      toolInfo.Sort = "常规";
      //                      toolInfo.Specification = dataArray[i, 4];
      //                      toolInfo.ToolID = "T" + dataArray[i, 2];
      //                      if (dataArray[i, 3] != null && isNumberic(dataArray[i, 3], out a))
      //                      {
      //                          toolInfo.ToolLength = Convert.ToDouble(dataArray[i, 3]);
      //                      }
      //                      toolInfo.ToolName = dataArray[i, 0];
      //                      toolInfos.Add(toolInfo);
      //                  }
      //                  var tool = wms.JDJS_WMS_Tool_Standard_Table.Where(r => r.MachTypeID == MachTypeID);
      //                  foreach (var real in tool)
      //                  {
      //                      wms.JDJS_WMS_Tool_Standard_Table.Remove(real);
      //                  }
      //                  foreach (var item in toolInfos)
      //                  {
                            
      //                      JDJS_WMS_Tool_Standard_Table jDJS_WMS_Tool_Standard_Table = new JDJS_WMS_Tool_Standard_Table()
      //                      {
      //                          Feed = item.Feed,
      //                          Knife = item.Knife,
      //                          MachTypeID = item.MachTypeID,
      //                          Name = item.ToolName,
      //                          ProcessStage = item.ProcessStage,
      //                          RazorDiameter = item.RazorDiameter,
      //                          RotatingSpeed = item.RotatingSpeed,
      //                          Shank = item.Shank,
      //                          Sort = item.Sort,
      //                          Specification = item.Specification,
      //                          ToolDiameter = item.ToolDiameter,
      //                          ToolID = item.ToolID,
      //                          ToolLength = item.ToolLength
      //                      };
      //                      wms.JDJS_WMS_Tool_Standard_Table.Add(jDJS_WMS_Tool_Standard_Table);
      //                  }
      //                  wms.SaveChanges();
      //                  mytran.Commit();
      //                  return "ok";
      //              }
      //              catch (Exception ex)
      //              {
      //                  mytran.Rollback();
                     
      //                  return ex.Message;

      //              }
      //          }

      //      }

      //  }

        /// <summary>
        /// 解析刀具表文件
        /// </summary>
        /// <param name="filePath">刀具表存放路径，此处需要保存后调用</param>
        /// <param name="processId">工序主键ID</param>
        /// <param name="time">探测点个数</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>是否成功</returns>
        public static bool ParsingTExcel(string filePath, int processId, int time,double nonCuttingTime,double  timeProportionalityCoefficient,   ref string errMsg)
        {
            try
            {
                double ToolTime = 0.1666666666;//换刀时间
                double ProcessTime = 0;//工序时间
                double OnMachMea = 0.033333333333;//探测时间
                int toolNum = 0;//换刀次数
                string ToolNo = "0";
                int toolFlag = 1;
                string workMential = "";
                string XPoint = "";
                string YPoint = "";
                string ZPoint = "";
                string note = "";
                FileInfo file = new FileInfo(filePath);
                using (JDJS_WMS_DB_USEREntities entities = new JDJS_WMS_DB_USEREntities())
                {
                    using (System.Data.Entity.DbContextTransaction mytran = entities.Database.BeginTransaction())
                    {
                        try
                        {
                            var row = entities.JDJS_WMS_Order_Process_Info_Table.Where(r => r.ID == processId).First();
                            var orderNumber = entities.JDJS_WMS_Order_Entry_Table.Where(r => r.Order_ID == row.OrderID).First().Order_Number;
                            int cncType = Convert.ToInt32(row.DeviceType);

                            var fileName = "T-" + orderNumber + "-P" + row.ProcessID + file.Extension;
                            PathInfo pathInfo = new PathInfo();
                            var paths = Path.Combine(pathInfo.upLoadPath(), orderNumber, "刀具表", fileName);
                            //此处未保存文件。需要自己处理
                            //file.SaveAs(paths);

                            row.toolChartName = fileName;
                            entities.SaveChanges();



                            string FilePath = paths;
                            string path = Path.GetFileNameWithoutExtension(FilePath);
                            int ProcessID = processId;
                            string[] str = path.Split('-');
                            string OrderNum = str[1];
                            string Process = str[2].Substring(1);
                            List<int> ToolNum = new List<int>();
                            List<ToolInfon> toolInfos = new List<ToolInfon>();
                            string fileSuffix = System.IO.Path.GetExtension(FilePath);
                            string err = "";
                            bool isOK = false;
                            #region
                            {
                                string[,] dt = GetExcelData(FilePath, ref err);
                                //dt.Columns[0].ColumnName = "序号";
                                //dt.Columns[1].ColumnName = "路径名";
                                //dt.Columns[2].ColumnName = "刀号";
                                //dt.Columns[3].ColumnName = "刀具";
                                //dt.Columns[4].ColumnName = "刀柄";
                                //dt.Columns[5].ColumnName = "刀具伸出长度";
                                //dt.Columns[6].ColumnName = "刃长";
                                //dt.Columns[7].ColumnName = "有效长度";
                                //dt.Columns[8].ColumnName = "粗中光";
                                //dt.Columns[9].ColumnName = "加工时间";
                                for (int i = 0; i < dt.GetLength(0); i++)
                                {

                                    if (i == 7)
                                    {

                                        ProcessTime = 0;
                                    }
                                    else if (i == 4)
                                    {

                                        workMential = dt[i, 1];
                                    }
                                    else if (i == 8)
                                    {

                                        XPoint = dt[i, 1];
                                        YPoint = dt[i, 3];
                                        ZPoint = dt[i, 5];
                                        note = dt[i, 7];
                                    }
                                    else
                                    {

                                        if (isOK)
                                        {
                                            string toolnum = dt[i, 2].ToString();
                                            double _toolnum = 0;
                                            if (isNumberic(toolnum, out _toolnum))
                                            {
                                                if (toolnum != ToolNo)
                                                {
                                                    toolNum++;
                                                    ToolNo = toolnum;
                                                }

                                                if (dt[i, 9].ToString() != "" && dt[i, 9] != null)
                                                {
                                                    if (dt[i, 9].ToString().Contains(':'))
                                                    {
                                                        var timeInfo = dt[i, 9].ToString().Split(':');
                                                        int hT = Convert.ToInt32(timeInfo[0]);
                                                        int minT = Convert.ToInt32(timeInfo[1]);
                                                        int secT = Convert.ToInt32(timeInfo[2]);
                                                        ProcessTime += (hT * 60 + minT + ((float)secT / 60));
                                                    }
                                                    else if (dt[i, 9].ToString() == "---")
                                                    {
                                                        ProcessTime += 5;
                                                    }
                                                    else
                                                    {
                                                        ProcessTime += (Convert.ToDouble(dt[i, 9].ToString()) * 1440);
                                                    }
                                                }
                                                if (!ToolNum.Contains(Convert.ToInt32(dt[i, 2].ToString())))
                                                {
                                                    ToolNum.Add(Convert.ToInt32(dt[i, 2].ToString()));
                                                    ToolInfon tool = new ToolInfon();
                                                    tool.PathName = dt[i, 1].ToString();
                                                    string toolno = dt[i, 2].ToString();
                                                    double _toolno = 0;
                                                    if (isNumberic(toolno, out _toolno))
                                                    {
                                                        tool.ToolNO = Convert.ToInt32(dt[i, 2].ToString());
                                                    }
                                                    tool.ToolName = dt[i, 3].ToString();
                                                    string tooll = dt[i, 5].ToString();
                                                    double _tooll = 0;
                                                    if (isNumberic(tooll, out _tooll))
                                                    {
                                                        tool.ToolLength = Convert.ToDouble(dt[i, 5].ToString());
                                                    }
                                                    string ToolD = dt[i, 6].ToString();
                                                    double _ToolD = 0;
                                                    if (isNumberic(ToolD, out _ToolD))
                                                    {
                                                        tool.BladeLenth = Convert.ToDouble(dt[i, 6].ToString());
                                                    }

                                                    string Toole = dt[i, 7].ToString();
                                                    if (Toole == "")
                                                    {
                                                        tool.EffectiveLength = tool.BladeLenth;
                                                    }
                                                    else
                                                    {
                                                        double _Toole = 0;
                                                        if (isNumberic(ToolD, out _ToolD))
                                                        {
                                                            tool.EffectiveLength = Convert.ToDouble(dt[i, 7].ToString());
                                                        }
                                                    }

                                                    tool.Shank = dt[i, 4].ToString();
                                                    toolInfos.Add(tool);
                                                }
                                            }
                                        }
                                    }

                                    if (dt[i, 0] == "序号")
                                    {
                                        isOK = true;
                                    }

                                }

                            #endregion

                                var toolInfo = entities.JDJS_WMS_Order_Process_Tool_Info_Table.Where(r => r.ProcessID == ProcessID);
                                foreach (var item in toolInfo)
                                {
                                    entities.JDJS_WMS_Order_Process_Tool_Info_Table.Remove(item);

                                }

                                var toolInformition = entities.JDJS_WMS_Order_Process_Information_Table.Where(r => r.ProcessID == ProcessID);
                                foreach (var item in toolInformition)
                                {
                                    entities.JDJS_WMS_Order_Process_Information_Table.Remove(item);

                                }
                                JDJS_WMS_Order_Process_Information_Table jd = new JDJS_WMS_Order_Process_Information_Table()
                                {
                                    ProcessID = ProcessID,
                                    Note = note,
                                    WorkMaterial = workMential,
                                    XPoint = XPoint,
                                    YPoint = YPoint,
                                    ZPoint = ZPoint
                                };
                                entities.JDJS_WMS_Order_Process_Information_Table.Add(jd);
                                var toolStand = entities.JDJS_WMS_Tool_Standard_Table.Where(r => r.MachTypeID == cncType).ToList();
                                var CETOU = toolStand.Where(r => r.Name == "测头");
                                int cetouID = -1;
                                if (CETOU.Count() > 0)
                                {
                                    cetouID = Convert.ToInt32(CETOU.First().ToolID.Substring(1));
                                }
                                List<string> toolStandNo = new List<string>();
                                foreach (var real in toolStand)
                                {
                                    string strT = real.ToolID;
                                    toolStandNo.Add(strT);
                                }
                                foreach (var item in toolInfos)
                                {
                                    //判断是否有特殊刀具，判断刀具是否在刀具标准表是否存在

                                    if (!toolStandNo.Contains("T" + item.ToolNO.ToString()))
                                    {
                                        toolFlag = -1;
                                        break;
                                    }
                                }

                                entities.SaveChanges();
                                foreach (var item in toolInfos)
                                {

                                    if (item.ToolNO != cetouID)
                                    {
                                        JDJS_WMS_Order_Process_Tool_Info_Table tool = new JDJS_WMS_Order_Process_Tool_Info_Table()
                                        {
                                            ProcessID = ProcessID,
                                            PathName = item.PathName,
                                            ToolNO = item.ToolNO,
                                            ToolName = item.ToolName,
                                            ToolLength = item.ToolLength,
                                            ToolDiameter = 0,
                                            ToolAroidance = item.BladeLenth,
                                            BladeLenth = item.BladeLenth,
                                            EffectiveLength = item.EffectiveLength,
                                            Shank = item.Shank
                                        };
                                        entities.JDJS_WMS_Order_Process_Tool_Info_Table.Add(tool);
                                    }
                                }

                            }


                            double times = (Math.Ceiling(OnMachMea * time + ToolTime * toolNum + ProcessTime) + nonCuttingTime) * timeProportionalityCoefficient;
                            var order = entities.JDJS_WMS_Order_Entry_Table.Where(r => r.Order_Number == OrderNum);
                            if (order.Count() > 0)
                            {
                                int orderid = order.First().Order_ID;
                                double pros = 0;
                                if (isNumberic(Process, out pros))
                                {
                                    int prose = Convert.ToInt32(Process);
                                    var pro = entities.JDJS_WMS_Order_Process_Info_Table.Where(r => r.OrderID == orderid && r.ProcessID == prose);

                                    foreach (var item in pro)
                                    {
                                        item.ProcessTime = times;
                                        item.toolPreparation = toolFlag;
                                        item.NonCuttingTime = nonCuttingTime;
                                    }

                                }
                            }



                            entities.SaveChanges();
                            mytran.Commit();
                        }
                        catch (Exception ex)
                        {
                            mytran.Rollback();
                            errMsg = (ex.Message);
                            return false; ;
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }


        }

        /// <summary>
        /// 获取excel表格数据
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="strMsg">错误信息</param>
        /// <returns>返回二维数组</returns>
        public static string[,] GetExcelData(string path, ref  string strMsg)
        {
            string[,] dataExcel;
            var DATA = ExcelToDataSet(path, out strMsg);
            if (DATA == null)
            {
                return null;
            }
            dataExcel = new string[DATA.Tables[0].Rows.Count, DATA.Tables[0].Columns.Count];
            int i = 0;

            foreach (DataRow mDr in DATA.Tables[0].Rows)
            {
                int j = 0;
                foreach (DataColumn mDc in DATA.Tables[0].Columns)
                {
                    dataExcel[i, j] = mDr[mDc].ToString();
                    j++;
                }
                i++;

            }
            return dataExcel;

        }

        /// <summary>
        /// Excel转换成DataSet（.xlsx/.xls）
        /// </summary>
        /// <param name="filePath">Excel文件路径</param>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        public static DataSet ExcelToDataSet(string filePath, out string strMsg)
        {
            strMsg = "";
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            string fileType = Path.GetExtension(filePath).ToLower();
            string fileName = Path.GetFileName(filePath).ToLower();
            try
            {
                ISheet sheet = null;
                int sheetNumber = 0;
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                if (fileType == ".xlsx")
                {
                    // 2007版本
                    XSSFWorkbook workbook = new XSSFWorkbook(fs);
                    sheetNumber = workbook.NumberOfSheets;
                    for (int i = 0; i < sheetNumber; i++)
                    {
                        string sheetName = workbook.GetSheetName(i);
                        sheet = workbook.GetSheet(sheetName);
                        if (sheet != null)
                        {
                            dt = GetSheetDataTable(sheet, out strMsg);
                            if (dt != null)
                            {
                                dt.TableName = sheetName.Trim();
                                ds.Tables.Add(dt);
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
                else if (fileType == ".xls")
                {
                    // 2003版本
                    HSSFWorkbook workbook = new HSSFWorkbook(fs);
                    sheetNumber = workbook.NumberOfSheets;
                    for (int i = 0; i < sheetNumber; i++)
                    {
                        string sheetName = workbook.GetSheetName(i);
                        sheet = workbook.GetSheet(sheetName);
                        if (sheet != null)
                        {
                            dt = GetSheetDataTable(sheet, out strMsg);
                            if (dt != null)
                            {
                                dt.TableName = sheetName.Trim();
                                ds.Tables.Add(dt);
                            }
                            else
                            {
                                return null;
                                //messageBox.Show("Sheet数据获取失败，原因：" + strMsg);
                            }
                        }
                    }
                }
                return ds;
            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 获取sheet表对应的DataTable
        /// </summary>
        /// <param name="sheet">Excel工作表</param>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        private static DataTable GetSheetDataTable(ISheet sheet, out string strMsg)
        {
            strMsg = "";
            DataTable dt = new DataTable();
            string sheetName = sheet.SheetName;
            int startIndex = 0;// sheet.FirstRowNum;
            int lastIndex = sheet.LastRowNum;
            //最大列数
            int cellCount = 0;
            IRow maxRow = sheet.GetRow(0);
            for (int i = startIndex; i <= lastIndex; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row != null && cellCount < row.LastCellNum)
                {
                    cellCount = row.LastCellNum;
                    maxRow = row;
                }
            }
            //列名设置
            try
            {
                for (int i = 0; i < maxRow.LastCellNum; i++)//maxRow.FirstCellNum
                {
                    dt.Columns.Add(Convert.ToChar(((int)'A') + i).ToString());
                    //DataColumn column = new DataColumn("Column" + (i + 1).ToString());
                    //dt.Columns.Add(column);
                }
            }
            catch
            {
                strMsg = "工作表" + sheetName + "中无数据";
                return null;
            }
            //数据填充
            for (int i = startIndex; i <= lastIndex; i++)
            {
                IRow row = sheet.GetRow(i);
                DataRow drNew = dt.NewRow();
                if (row != null)
                {
                    for (int j = row.FirstCellNum; j < row.LastCellNum; ++j)
                    {
                        if (row.GetCell(j) != null)
                        {
                            ICell cell = row.GetCell(j);
                            switch (cell.CellType)
                            {
                                case CellType.Blank:
                                    drNew[j] = "";
                                    break;
                                case CellType.Numeric:
                                    short format = cell.CellStyle.DataFormat;
                                    //对时间格式（2015.12.5、2015/12/5、2015-12-5等）的处理
                                    if (format == 14 || format == 31 || format == 57 || format == 58)
                                        drNew[j] = cell.DateCellValue;
                                    else
                                        drNew[j] = cell.NumericCellValue;
                                    if (cell.CellStyle.DataFormat == 177 || cell.CellStyle.DataFormat == 178 || cell.CellStyle.DataFormat == 188)
                                        drNew[j] = cell.NumericCellValue.ToString("#0.00");
                                    break;
                                case CellType.String:
                                    drNew[j] = cell.StringCellValue;
                                    break;
                                case CellType.Formula:
                                    try
                                    {
                                        drNew[j] = cell.NumericCellValue;
                                        if (cell.CellStyle.DataFormat == 177 || cell.CellStyle.DataFormat == 178 || cell.CellStyle.DataFormat == 188)
                                            drNew[j] = cell.NumericCellValue.ToString("#0.00");
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            drNew[j] = cell.StringCellValue;
                                        }
                                        catch { }
                                    }
                                    break;
                                default:
                                    drNew[j] = cell.StringCellValue;
                                    break;
                            }
                        }
                    }
                }
                dt.Rows.Add(drNew);
            }
            return dt;
        }

        protected static bool isNumberic(string message, out double result)
        {
            //判断是否为整数字符串
            //是的话则将其转换为数字并将其设为out类型的输出值、返回true, 否则为false
            result = -1;   //result 定义为out 用来输出值
            try
            {
                //当数字字符串的为是少于4时，以下三种都可以转换，任选一种
                //如果位数超过4的话，请选用Convert.ToInt32() 和int.Parse()

                //result = int.Parse(message);
                //result = Convert.ToInt16(message);
                result = Convert.ToDouble(message);
                return true;
            }
            catch
            {
                return false;
            }
        }



        ////<summary>
        ////解析固定刀具表
        ////</summary>
        ////<param name="args"></param>
        public string ImportExcel(int cncTypeId, string path1)
        {

            //解析excel文件
            int MachTypeID = cncTypeId;
            string path = path1;
            string fileSuffix = Path.GetExtension(path);
            if (fileSuffix != ".xls" && fileSuffix != ".xlsx")
            {
                return ("请输入xls或xlsx格式文件！");

            }
            string err = "";
            var data = GetExcelData(path, ref err);
            var x = Convert.ToInt32(data.GetLongLength(0));
            var y = Convert.ToInt32(data.GetLongLength(1));
            int arrayx = 0;
            int arrayy = 0;
            var dataArray = new string[x, y];
            foreach (var item in data)
            {
                if (item != null)
                {
                    dataArray[arrayx, arrayy] = item.ToString();
                }
                arrayy++;
                if (arrayy >= y)
                {
                    arrayx++;
                    arrayy = 0;
                }
            }
            using (JDJS_WMS_DB_USEREntities wms = new JDJS_WMS_DB_USEREntities())
            {
                using (System.Data.Entity.DbContextTransaction mytran = wms.Database.BeginTransaction())
                {
                    try
                    {
                        List<ToolInfo> toolInfos = new List<ToolInfo>();
                        for (int i = 2; i < x; i++)
                        {
                            ToolInfo toolInfo = new ToolInfo();
                            double a = 0;
                            if (dataArray[i, 6] != null && isNumberic(dataArray[i, 6], out a))
                            {
                                toolInfo.Feed = Convert.ToDouble(dataArray[i, 6]);
                            }
                            toolInfo.Knife = 0;
                            toolInfo.MachTypeID = MachTypeID;
                            toolInfo.ProcessStage = "精";
                            if (dataArray[i, 5] != null && isNumberic(dataArray[i, 5], out a))
                            {
                                toolInfo.RotatingSpeed = Convert.ToDouble(dataArray[i, 5]);
                            }
                            toolInfo.Shank = dataArray[i, 1];
                            toolInfo.Sort = "常规";
                            toolInfo.Specification = dataArray[i, 4];
                            toolInfo.ToolID = "T" + dataArray[i, 2];
                            if (dataArray[i, 3] != null && isNumberic(dataArray[i, 3], out a))
                            {
                                toolInfo.ToolLength = Convert.ToDouble(dataArray[i, 3]);
                            }
                            toolInfo.ToolName = dataArray[i, 0];
                            toolInfos.Add(toolInfo);
                        }
                        var tool = wms.JDJS_WMS_Tool_Standard_Table.Where(r => r.MachTypeID == MachTypeID);
                        foreach (var real in tool)
                        {
                            wms.JDJS_WMS_Tool_Standard_Table.Remove(real);
                        }
                        foreach (var item in toolInfos)
                        {

                            JDJS_WMS_Tool_Standard_Table jDJS_WMS_Tool_Standard_Table = new JDJS_WMS_Tool_Standard_Table()
                            {
                                Feed = item.Feed,
                                Knife = item.Knife,
                                MachTypeID = item.MachTypeID,
                                Name = item.ToolName,
                                ProcessStage = item.ProcessStage,
                                RazorDiameter = item.RazorDiameter,
                                RotatingSpeed = item.RotatingSpeed,
                                Shank = item.Shank,
                                Sort = item.Sort,
                                Specification = item.Specification,
                                ToolDiameter = item.ToolDiameter,
                                ToolID = item.ToolID,
                                ToolLength = item.ToolLength
                            };
                            wms.JDJS_WMS_Tool_Standard_Table.Add(jDJS_WMS_Tool_Standard_Table);
                        }
                        wms.SaveChanges();
                        mytran.Commit();
                        return "ok";
                    }
                    catch (Exception ex)
                    {
                        mytran.Rollback();

                        return ex.Message;

                    }
                }

            }

        }
        //public static Array ReadXls(string filename, int index)//读取第index个sheet的数据   
        //{            //启动Excel应用程序    
        //    Microsoft.Office.Interop.Excel.Application xls = new Microsoft.Office.Interop.Excel.Application();
        //    //打开filename表          
        //    _Workbook book = xls.Workbooks.Open(filename, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
        //    _Worksheet sheet;//定义sheet变量         
        //    xls.Visible = false;//设置Excel后台运行        
        //    xls.DisplayAlerts = false;//设置不显示确认修改提示    
        //    try
        //    {
        //        sheet = (_Worksheet)book.Worksheets.get_Item(index);//获得第index个sheet，准备读取          
        //    }
        //    catch (Exception ex)//不存在就退出     
        //    {
        //        Console.WriteLine(ex.Message);
        //        return null;
        //    }
        //    Console.WriteLine(sheet.Name);
        //    int row = sheet.UsedRange.Rows.Count;//获取不为空的行数     
        //    int col = sheet.UsedRange.Columns.Count;//获取不为空的列数      
        //                                            // Array value = (Array)sheet.get_Range(sheet.Cells[1, 1], sheet.Cells[row, col]).Cells.Value2;//获得区域数据赋值给Array数组，方便读取                      
        //    Microsoft.Office.Interop.Excel.Range range = sheet.Range[sheet.Cells[1, 1], sheet.Cells[row, col]];
        //    Array value = (Array)range.Value2;
        //    book.Save();//保存           
        //    book.Close(false, System.Reflection.Missing.Value, System.Reflection.Missing.Value);//关闭打开的表   
        //    xls.Quit();//Excel程序退出        
        //               //sheet,book,xls设置为null，防止内存泄露     
        //    sheet = null;
        //    book = null;
        //    xls = null;
        //    GC.Collect();//系统回收资源      
        //    return value;
        //}

        //public static DataSet getData(string path)

        //{

        //    string fileSuffix = System.IO.Path.GetExtension(path);

        //    if (string.IsNullOrEmpty(fileSuffix))

        //        return null;

        //    using (DataSet ds = new DataSet())

        //    {

        //        //判断Excel文件是2003版本还是2007版本

        //        string connString = "";

        //        if (fileSuffix == ".xls")

        //            connString = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\"";

        //        else
        //            // connString = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\"";
        //            connString = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + path + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";

        //        //读取文件

        //        //string sql_select = "select * from [sheet1$]";
        //        string tableName;
        //        using (OleDbConnection conn = new OleDbConnection(connString))
        //        {

        //            //DataTable table = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, null);
        //            //string tableName = table.Rows[0]["Table_Name"].ToString();
        //            //string sql_select = "select * from " + "[" + tableName + "]";
        //            //string sql_select = "select * from [sheet1$]";



        //            conn.Open();

        //            System.Data.DataTable table = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, null);
        //            tableName = table.Rows[0]["Table_Name"].ToString();
        //            string strExcel = "select * from " + "[" + tableName + "]";
        //            OleDbDataAdapter adapter = new OleDbDataAdapter(strExcel, connString);

        //            adapter.Fill(ds, tableName);
        //            conn.Close();


        //        }

        //        if (ds == null)
        //        {

        //        }
        //        return ds;

        //    }

        //}
       


        /// <summary>
        /// 根据机床型号读取固定刀具信息
        /// </summary>
        /// <param name="MachTpyeID">机床型号主键ID</param>
        /// <returns></returns>
        private string ReadTool(int MachTpyeID)
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<ReadToolInfo> toolInfos = new List<ReadToolInfo>();
            using (JDJS_WMS_DB_USEREntities wms = new JDJS_WMS_DB_USEREntities())
            {
                var tools = wms.JDJS_WMS_Tool_Standard_Table.Where(r => r.MachTypeID == MachTpyeID);
                foreach (var item in tools)
                {
                    ReadToolInfo toolInfo = new ReadToolInfo();
                    toolInfo.id = item.ID;
                    toolInfo.Feed = item.Feed.ToString();
                    toolInfo.Knife = item.Knife.ToString();
                    toolInfo.MachTypeID = MachTpyeID;
                    toolInfo.ProcessStage = item.ProcessStage.ToString();
                    toolInfo.RazorDiameter = item.RazorDiameter.ToString();
                    toolInfo.RotatingSpeed = item.RotatingSpeed.ToString();
                    toolInfo.Shank = item.Shank.ToString();
                    toolInfo.Sort = item.Sort.ToString();
                    toolInfo.Specification = item.Specification.ToString();
                    toolInfo.ToolDiameter = item.ToolDiameter.ToString();
                    toolInfo.ToolID = item.ToolID.ToString();
                    toolInfo.ToolLength = item.ToolLength.ToString();
                    toolInfo.ToolName = item.Name;
                    toolInfos.Add(toolInfo);
                }
            }
            var json = serializer.Serialize(toolInfos);
            return json;
        }

        /// <summary>
        /// 修改单独一条固定刀具信息
        /// </summary>
        /// <param name="ToolID">该固定刀具主键ID</param>
        /// <param name="toolInfo">刀具信息</param>
        /// <param name="ErrStr">错误信息</param>
        /// <returns></returns>
        private bool AlterTool(int ToolID, ToolInfo toolInfo, ref string ErrStr)
        {
            using (JDJS_WMS_DB_USEREntities wms = new JDJS_WMS_DB_USEREntities())
            {
                using (System.Data.Entity.DbContextTransaction mytran = wms.Database.BeginTransaction())
                {
                    try
                    {
                        var tool = wms.JDJS_WMS_Tool_Standard_Table.Where(r => r.ID == ToolID).FirstOrDefault();
                        if (tool == null)
                        {
                            mytran.Rollback();
                            ErrStr = "该固定刀具不存在！";
                            return false;
                        }
                        tool.Feed = toolInfo.Feed;
                        tool.Knife = toolInfo.Knife;
                        tool.Name = toolInfo.ToolName;
                        tool.ProcessStage = toolInfo.ProcessStage;
                        tool.RazorDiameter = toolInfo.RazorDiameter;
                        tool.RotatingSpeed = toolInfo.RotatingSpeed;
                        tool.Shank = toolInfo.Shank;
                        tool.Sort = toolInfo.Sort;
                        tool.Specification = toolInfo.Specification;
                        tool.ToolDiameter = toolInfo.ToolDiameter;
                        tool.ToolID = toolInfo.ToolID;
                        tool.ToolLength = toolInfo.ToolLength;

                        wms.SaveChanges();
                        mytran.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        mytran.Rollback();
                        ErrStr = ex.Message;
                        return false;

                    }

                }
            }
        }

    }

    public class ToolInfon
    {
        public string PathName;
        public int ToolNO;
        public string ToolName;
        public string Shank;
        public double ToolLength;
        public double BladeLenth;
        public double EffectiveLength;

    }

    public class ToolInfo
    {
        /// <summary>
        /// 刀具编号
        /// </summary>
        public string ToolID;
        /// <summary>
        /// 刀具常规
        /// </summary>
        public string Sort;
        /// <summary>
        /// 刀具名称
        /// </summary>
        public string ToolName;
        /// <summary>
        /// 刀具规格
        /// </summary>
        public string Specification;
        /// <summary>
        /// 粗精？
        /// </summary>
        public string ProcessStage;
        /// <summary>
        /// 刀杆直径
        /// </summary>
        public double RazorDiameter;
        /// <summary>
        /// 刀具直径
        /// </summary>
        public double ToolDiameter;
        /// <summary>
        /// 装刀长度
        /// </summary>
        public double ToolLength;
        /// <summary>
        /// 转速
        /// </summary>
        public double RotatingSpeed;
        /// <summary>
        /// 进给
        /// </summary>
        public double Feed;
        /// <summary>
        /// 吃刀量
        /// </summary>
        public double Knife;
        /// <summary>
        /// 刀柄
        /// </summary>
        public string Shank;
        /// <summary>
        /// 机床型号主键
        /// </summary>
        public int MachTypeID;
    }

    public class ReadToolInfo
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int id;
        /// <summary>
        /// 刀具编号
        /// </summary>
        public string ToolID;
        /// <summary>
        /// 刀具常规
        /// </summary>
        public string Sort;
        /// <summary>
        /// 刀具名称
        /// </summary>
        public string ToolName;
        /// <summary>
        /// 刀具规格
        /// </summary>
        public string Specification;
        /// <summary>
        /// 粗精？
        /// </summary>
        public string ProcessStage;
        /// <summary>
        /// 刀杆直径
        /// </summary>
        public string RazorDiameter;
        /// <summary>
        /// 刀具直径
        /// </summary>
        public string ToolDiameter;
        /// <summary>
        /// 装刀长度
        /// </summary>
        public string ToolLength;
        /// <summary>
        /// 转速
        /// </summary>
        public string RotatingSpeed;
        /// <summary>
        /// 进给
        /// </summary>
        public string Feed;
        /// <summary>
        /// 吃刀量
        /// </summary>
        public string Knife;
        /// <summary>
        /// 刀柄
        /// </summary>
        public string Shank;
        /// <summary>
        /// 机床型号主键
        /// </summary>
        public int MachTypeID;
    }
}