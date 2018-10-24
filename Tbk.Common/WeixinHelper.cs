using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tbk.Common
{
    public class WeixinHelper
    {
        /// <summary>
        /// 获取临时二维码ticket
        /// </summary>
        /// <param name="scene_str">场景值ID openid做场景值ID</param>
        /// <returns></returns>
        public static string CreateTempQRCode(string scene_str)
        {
            var result = HttpUtility.SendPostHttpRequest($"https://api.weixin.qq.com/cgi-bin/qrcode/create?access_token={AccessTokenHelper.GetAccessToken()}", "application/json", "{\"expire_seconds\": 2592000, \"action_name\": \"QR_STR_SCENE\", \"action_info\": {\"scene\": {\"scene_str\": \"" + scene_str + "\"}}}");

            JObject jobect = (JObject)JsonConvert.DeserializeObject(result);

            string ticket = (string)jobect["ticket"];

            if (string.IsNullOrEmpty(ticket))
            {
                LogHelper.WriteLog(typeof(WeixinHelper), "获取临时二维码ticket失败" + result);
                return null;
            }

            return ticket;
        }

        public static string SendTemplateMsg(string accessToken,string data)
        {
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/message/template/send?access_token={0}", accessToken);
            string result = HttpUtility.SendPostHttpRequest(url, "application/json", data);

            return result;

        }
    }
}
