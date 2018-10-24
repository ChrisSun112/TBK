using Tbk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace Tbk.mweb
{
    /// <summary>
    /// config 的摘要说明
    /// </summary>
    public class config
    {
        public config()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        public static string url = "http://gw.api.taobao.com/router/rest";
        public static string appkey = "25065474";
        public static string secret = "79206c1c0c7531a3d4fb41c1095e1a7d";
        public  static long addzoneId = 23004550492;
        public static string root_url = "http://m.yshizi.cn";


        public static int pageSize = 30;

        public static Dictionary<int, string> category_list = new Dictionary<int, string>() {
            { 1 ,"女装"},
            { 2 ,"男装"},
            { 3 ,"鞋包"},
            { 4,"珠宝配饰"},
            { 5,"运动户外"},
            { 6 ,"美妆"},
            { 7 ,"母婴"},
            { 8 ,"食品"},
            { 9 ,"内衣"},
            { 10 ,"数码"},
            { 11 ,"家装"},
            { 12 ,"家居用品"},
            { 13 ,"家电"},
            { 15 ,"生活服务"},
            { 16 ,"图书音像"},
            { 17 ,"其他"}
        };

   


        public static string GetTaobaoKePassword(string url, string log_url)
        {
            try
            {
                ITopClient client = new DefaultTopClient(config.url, appkey, secret);
                TbkTpwdCreateRequest req = new TbkTpwdCreateRequest();

                if (url.Substring(0, 4) != "http")
                {
                    url = "https:" + url;
                }
                req.Text = "关注“网购有券”,超值活动，惊喜多多！";
                req.Url = url;
                req.Logo = log_url;

                TbkTpwdCreateResponse rsp = client.Execute(req);
                return rsp.Data.Model;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(config), "生成淘宝口令失败" + ex.Message);
                LogHelper.WriteLog(typeof(config), "生成淘宝口令失败,url为" + url);
                return "";
            }

        }

        public static string GetQueryTbPassword(string passwordContent)
        {
            try
            {
                ITopClient client = new DefaultTopClient(url, appkey, secret);
                WirelessShareTpwdQueryRequest req = new WirelessShareTpwdQueryRequest();
                req.PasswordContent = passwordContent;
                WirelessShareTpwdQueryResponse rsp = client.Execute(req);

                return rsp.Content;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(config), "生成淘宝口令失败" + ex.Message);
                return "";
            }

        }

    }
}