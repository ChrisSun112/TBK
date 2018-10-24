using Tbk.Common;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Tbk.Common
{
	
	 public class AccessTokenHelper
     {
		
		/// <summary>
		/// 过期时间为7200秒
		/// </summary>
		private static int Expires_Period = 7200;
		

        private static string AppID = "wxe2c5d6d728777f32";
        private static string AppSecret = "f476b26a053fbca01a5ca00c8d29dbde";

        //public static string MAccessToken
        //{
        //    get
        //    {
        //        //如果为空，或者过期，需要重新获取
        //        if (string.IsNullOrEmpty(mAccessToken))
        //        {
        //            //获取access_token
        //            mAccessToken = GetAccessToken(AppID, AppSecret);

        //        }
        //        else if (HasExpired())
        //        {
        //            mAccessToken = GetAccessToken(AppID, AppSecret);
        //        }

        //        return mAccessToken;
        //    }
        //}


        public static string GetAccessToken()
        {
            MySqlHelper mySqlHelper = new MySqlHelper();
            string sqlStr = "select access_token,createdatetime from tb_access_token order by createdatetime desc limit 1;";
            AccessToken accessToken = mySqlHelper.Query<AccessToken>(sqlStr, null);
            if (accessToken != null && HasExpired(accessToken))
            {
                return accessToken.Access_Token;
            }
            else
            {
                accessToken = GetAccessTokenByApi(AppID, AppSecret);
                accessToken.CreateDateTime = DateTime.Now;

                sqlStr = "insert into tb_access_token values(@access_token,@createdatetime)";

               
                MySqlParameter[] mySqlParameters = { new MySqlParameter("access_token", accessToken.Access_Token),new MySqlParameter("createdatetime", accessToken.CreateDateTime)};
                mySqlHelper.ExecuteSql(sqlStr, mySqlParameters);
                return accessToken.Access_Token;
            }

        }

        public static string GetJsapiTicket()
        {
            string jsapiTicket = (string)CacheHelper.GetCache("jsapiticket");
            if (jsapiTicket == null)
            {
                string url = $"https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={GetAccessToken()}&type=jsapi";

                string result = HttpUtility.GetData(url);

                if (result.Contains("ticket"))
                {
                    JObject jObject = (JObject)JsonConvert.DeserializeObject(result);
                    jsapiTicket = (string)jObject["ticket"];
                    CacheHelper.SetCache("jsapiticket", jsapiTicket, DateTime.Now.AddSeconds(7100), TimeSpan.Zero);
                }
                else
                {
                    LogHelper.WriteLog(typeof(AccessTokenHelper), $"通过微信接口获取jsapi ticket失败，返回{result}");

                    //获取失败，再获取一次
                    url = $"https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={GetAccessToken()}&type=jsapi";

                    result = HttpUtility.GetData(url);

                    if (result.Contains("ticket"))
                    {
                        JObject jObject = (JObject)JsonConvert.DeserializeObject(result);
                        jsapiTicket = (string)jObject["ticket"];
                        CacheHelper.SetCache("jsapiticket", jsapiTicket, DateTime.Now.AddSeconds(7100), TimeSpan.Zero);
                    }
                }
            }

            return jsapiTicket;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="appSecret"></param>
        /// <returns></returns>
        private static AccessToken GetAccessTokenByApi(string appId, string appSecret)
        {
            //string url = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", appId, appSecret);
            string url = "http://114.141.170.162:10230/AccessTokenHandle.ashx";
            string result = HttpUtility.GetData(url);

            if (result.Contains("access_token"))
            {
                AccessToken accessToken = JsonConvert.DeserializeObject<AccessToken>(result);
                return accessToken;
            }
            else
            {
                LogHelper.WriteLog(typeof(AccessTokenHelper), $"通过微信接口获取access token失败，返回{result}");
            }
            
            return null;
        }
        /// <summary>
        /// 判断Access_token是否过期,过期返回false,没过期返回true
        /// </summary>
        /// <returns>bool</returns>
        private static bool HasExpired(AccessToken accessToken)
		{
			if (accessToken.CreateDateTime != null)
			{
				//过期时间，允许有一定的误差，一分钟。获取时间消耗
				if (DateTime.Now < accessToken.CreateDateTime.AddSeconds(Expires_Period).AddSeconds(-60))
				{
					return true;
				}
			}
			return false;
		}


        public class AccessToken {
            public string Access_Token { get; set; }

            public int expires_in { get; set; }

            public DateTime CreateDateTime { get; set; }
        }

        

    }
}
