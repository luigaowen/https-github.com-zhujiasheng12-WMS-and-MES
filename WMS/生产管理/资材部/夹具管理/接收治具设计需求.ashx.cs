﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace WebApplication2.生产管理.资材部.夹具管理
{
    /// <summary>
    /// 接收治具设计需求 的摘要说明
    /// </summary>
    public class 接收治具设计需求 : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                int fxDemandId = int.Parse(context.Request["fxDemandId"]);
                int personId = int.Parse(context.Session["id"].ToString());
                string personName = context.Session["UserName"].ToString();
                using (JDJS_WMS_DB_USEREntities model = new JDJS_WMS_DB_USEREntities())
                {
                    var demand = model.JDJS_WMS_Fixture_Manage_Demand_Table.Where(r => r.ID == fxDemandId).FirstOrDefault();
                    if (demand == null)
                    {
                        context.Response.Write("该治具需求不存在");
                        return;
                    }
                    if (demand.DesignPersonId != null && demand.DesignPersonName != null)
                    {
                        context.Response.Write("该治具设计需求暂不支持接收");
                        return;
                    }
                    if (demand.State =="审核通过")
                    {
                        context.Response.Write("该治具设计需求暂不支持接收");
                        return;
                    }
                    using (System.Data.Entity.DbContextTransaction mytran = model.Database.BeginTransaction())
                    {
                        try
                        {
                            demand.DesignPersonId = personId;
                            demand.DesignPersonName = personName;
                            demand.State = "设计中";
                            model.SaveChanges();
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
            catch (Exception ex)
            {
                context.Response.Write(ex.Message);
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