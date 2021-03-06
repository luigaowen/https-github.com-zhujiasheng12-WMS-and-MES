﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebApplication2.生产管理.资材部.夹具管理.审核功能.审核文件管理
{
    /// <summary>
    /// 查看审核文件 的摘要说明
    /// </summary>
    public class 查看审核文件 : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            string fxNum = context.Request["fxNum"];
            PathInfo info = new PathInfo();
            string path = System.IO.Path.Combine(info.upLoadPath(), @"特殊治具管理", fxNum, @"审核文件");
            DirectoryInfo root = new DirectoryInfo(path);
            if (!root.Exists)
            {
                root.Create();
            }
            FileInfo[] files = root.GetFiles();
            List<File> file = new List<File>();


            foreach (var item in files)
            {
                file.Add(new File
                {
                    fileName = item.Name,
                    filePath = item.FullName,
                    fileSize = (item.Length / 1024).ToString() + "  kB",
                    fileTime = item.CreationTime.ToString()
                });
            }
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var model = new { msg = "", code = 0, count = file.Count, data = file };
            var json = serializer.Serialize(model);
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
    class File
    {
        public string fileName;
        public string filePath;
        public string fileSize;
        public string fileTime;

    }
}