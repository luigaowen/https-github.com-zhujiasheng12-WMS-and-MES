﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.生产管理.维修部.报警历史
{
    /// <summary>
    /// alarmHistoryRead 的摘要说明
    /// </summary>
    public class alarmHistoryRead : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var page = int.Parse(context.Request["page"]);
            var limit = int.Parse(context.Request["limit"]);
            
            {
                int CncID = int.Parse(context.Request["id"]);
                List<AlarmRead> alarmReads = new List<AlarmRead>();
                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                using (JDJS_WMS_DB_USEREntities  wms = new JDJS_WMS_DB_USEREntities ())
                {
                    var alarmList = wms.JDJS_WMS_Device_Alarm_History_Table.Where(r => r.CncID == CncID).OrderBy(r => r.StartTime);
                    foreach (var alarm in alarmList)
                    {
                        AlarmRead alarmRead = new AlarmRead();
                        alarmRead.errCode = alarm.ErrCode.ToString();
                        alarmRead.errDesc = alarm.ErrDesc.ToString();
                        alarmRead.alarmTime = alarm.StartTime.ToString();
                        alarmReads.Add(alarmRead);
                    }

                }
                var order1 = alarmReads.Skip((page - 1) * limit).Take(limit);

                var order2 = order1.ToList();
                var model = new { code = 0, msg = "", count = alarmReads.Count(), data = order2 };
                string json = serializer.Serialize(model);
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
    public class AlarmRead
    {
        /// <summary>
        /// 错误编码
        /// </summary>
        public string errCode;
        /// <summary>
        /// 错误描述
        /// </summary>
        public string errDesc;
        /// <summary>
        /// 报警时间
        /// </summary>
        public string alarmTime;
    }
}