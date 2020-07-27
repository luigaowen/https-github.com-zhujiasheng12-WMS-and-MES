﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication2.Model.生产管理.工程部;
using WebApplication2.生产管理.生产部.订单跟踪.Model;

namespace WebApplication2.生产管理.生产部.订单跟踪
{
    /// <summary>
    /// 生产进度 的摘要说明
    /// </summary>
    public class 生产进度 : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var orderId = int.Parse(context.Request.Form["orderId"]);
            var info = DataManage.GetWorkInfo(orderId);
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var json = serializer.Serialize(info);
            context.Response.Write(json);
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