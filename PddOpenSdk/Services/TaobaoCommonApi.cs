using System;
using System.Collections.Generic;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;
using static Top.Api.Response.TbkDgMaterialOptionalResponse;

namespace PddOpenSdk.Services
{
    public class TaobaoCommonApi
    {
        //淘宝联盟接口url
        private string apiUrl= "http://gw.api.taobao.com/router/rest";
        //淘宝联盟appkey
        private string appkey;

        //淘宝联盟 secret
        private string secret;

        //淘宝联盟addzoneid
        private long addzoneId;

        public TaobaoCommonApi(string appkey,string secret,long addzoneId)
        {
            this.appkey = appkey;
            this.secret = secret;
            this.addzoneId = addzoneId;
            
        }

        /// <summary>
        /// 淘宝物料搜索接口，获取佣金、优惠券信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IList<MapDataDomain> MaterialOptional(string key)
        {

            if (string.IsNullOrEmpty(appkey)||string.IsNullOrEmpty(secret)||addzoneId==0)
            {
                throw new Exception("请检查是否设置接口url、淘宝appkey、secret及addzoneId");
            }

            ITopClient client = new DefaultTopClient(apiUrl,appkey,secret);

            TbkDgMaterialOptionalRequest req = new TbkDgMaterialOptionalRequest();
            req.AdzoneId = addzoneId;
            req.Platform = 2L;

            req.PageSize = 100L;
            req.Q = key;

            req.PageNo = 1L;
            TbkDgMaterialOptionalResponse rsp = client.Execute(req);
            return rsp.ResultList;
        }

        /// <summary>
        /// 获取淘口令
        /// </summary>
        /// <param name="url"></param>
        /// <param name="log_url"></param>
        /// <returns></returns>
        public string GetTaobaoKePassword(string url, string log_url)
        {
            
            ITopClient client = new DefaultTopClient(apiUrl, appkey, secret);
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

    }
}
