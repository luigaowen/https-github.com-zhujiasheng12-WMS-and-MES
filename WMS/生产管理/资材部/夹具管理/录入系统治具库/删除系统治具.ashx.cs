﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication2.生产管理.资材部.夹具管理.特殊治具管理Access数据库;

namespace WebApplication2.生产管理.资材部.夹具管理.录入系统治具库
{
    /// <summary>
    /// 删除系统治具 的摘要说明
    /// </summary>
    public class 删除系统治具 : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                int id = int.Parse(context.Request["id"]);//id
                using (FixtureModel model = new FixtureModel())
                {
                    var fx = model.JDJS_WMS_Fixture_System_Table.Where(r => r.Id  == id).FirstOrDefault();
                    if (fx == null)
                    {
                        context.Response.Write("该治具不存在！");
                        return;
                    }
                    
                    
                    using (System.Data.Entity.DbContextTransaction mytran = model.Database.BeginTransaction())
                    {
                        try
                        {
                            model.JDJS_WMS_Fixture_System_Table.Remove(fx);
                            model.SaveChanges();
                            mytran.Commit();
                            PathInfo info = new PathInfo();
                            if (System.IO.File.Exists(System.IO.Path.Combine(info.GetFixtrue_SurfMillFilePath(), fx.FileName)))
                            {
                                System.IO.File.Delete(System.IO.Path.Combine(info.GetFixtrue_SurfMillFilePath(), fx.FileName));
                            }
                            using (JDJS_WMS_DB_USEREntities  wms = new JDJS_WMS_DB_USEREntities())
                            {
                                var status = wms.JDJS_WMS_Device_Status_Table.Where(r => r.SystemId == id).FirstOrDefault();
                                if (status != null)
                                {
                                    wms.JDJS_WMS_Device_Status_Table.Remove(status);
                                }
                                wms.SaveChanges();
                            }
                                string str = "";
                            Fixture_SurfMill.DeleteChildJIG(fx.Name, fx.Desc, fx.StockAllNum.ToString (), fx.StockCurrNum.ToString(), ref str);
                            context.Response.Write(str);
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