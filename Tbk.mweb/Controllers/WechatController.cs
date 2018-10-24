using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSoup.Nodes;
using PddOpenSdk.Models.Request.Ddk;
using PddOpenSdk.Models.Response.Ddk;
using PddOpenSdk.Services;
using PddOpenSdk.Services.PddApi;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml;
using Tbk.Common;
using Tbk.DAL;
using Tbk.Entity;
using Tbk.mweb;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace TaobaoKe.Controllers
{
    public class WechatController : Controller
    {
        private string Token = "weixin";
        private decimal commission_rate = decimal.Parse(ConfigurationManager.AppSettings["commission_rate"].ToString());
        private string pdd_pid = ConfigurationManager.AppSettings["pdd_pid"].ToString();

        public WechatController()
        {
            PddCommonApi.ClientId = "d8172a66ddf14220beac58e8eddca0d9";
            PddCommonApi.ClientSecret = "d5993ce16cc0d1bd35403176f35544962e786645";
            PddCommonApi.RedirectUri = "RedirectUri";
            PddCommonApi.AccessToken = "";
        }

        //private string EncodingAESKey = "LmKRk0rhpU98ODyfqP0f9VGUNkjOHvV7uMxMWKp3FV3";

        //private string AppId = "wxe2c5d6d728777f32";
        //private string AppSecret = "f476b26a053fbca01a5ca00c8d29dbde";
        #region 微信公众号接口验证
        [HttpGet]
        [ActionName("Index")]
        public ActionResult Get(string echostr)
        {
            try
            {
                if (CheckSignature())
                {
                    LogHelper.WriteLog(typeof(WechatController), echostr);
                    return Content(echostr); //返回随机字符串则表示验证通过
                }
                else
                {
                    return Content("如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。");
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(WechatController), ex.Message);
                return Content("验证失败。" + ex.Message);
            }

        }
        #endregion

        /// </summary>
        [HttpPost]
        [ActionName("Index")]
        public async Task<ActionResult> Post()
        {
         
            try
            {
                string weixin = "";
                weixin = PostInput();//获取xml数据

                //LogHelper.WriteLog(typeof(WechatController),weixin);

                if (!string.IsNullOrEmpty(weixin))
                {
                    return Content(await ResponseMsgAsync(weixin));////调用消息适配器
                }

                return Content("");
            }
            catch (Exception ex)
            {
                #region 异常处理
                LogHelper.WriteLog(typeof(WechatController), $"MessageHandler错误：{ex.Message}");
                LogHelper.WriteLog(typeof(WechatController), ex.Source);
                LogHelper.WriteLog(typeof(WechatController), ex.StackTrace);


                return Content("服务失败");
                #endregion
            }
        }

        [HttpGet]
        public ActionResult WebBaseAuthorize(string code,string state)
        {
         
            if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(state))
            {
                string openid = "";
                string access_token = "";
                string result = "";
                if (Session[code] == null)
                {
                    result = HttpUtility.HttpGet($"https://api.weixin.qq.com/sns/oauth2/access_token?appid={ ConfigurationManager.AppSettings["weixin_appid"].ToString()}&secret={ConfigurationManager.AppSettings["weixin_secret"]}&code={code}&grant_type=authorization_code", "", "utf-8");
                    JObject jobect = (JObject)JsonConvert.DeserializeObject(result);
                    openid = (string)jobect["openid"];
                    access_token = (string)jobect["access_token"];
                    Session[code] = openid + "|" + access_token;
                }
                else
                {
                    var temp = (string)Session[code];
                    openid = temp.Split('|')[0];
                    access_token = temp.Split('|')[1];
                }

                //获取openid,网页授权access_token               
                if (!string.IsNullOrEmpty(openid))
                {
                    switch (state)
                    {
                        case "share":
                            return Redirect($"Share?openid={openid}&access_token={access_token}");
                        case "me":
                            return Redirect($"{config.root_url}/me/index?openid={openid}&access_token={access_token}");
                        case "cash":
                            return Redirect($"{config.root_url}/me/wallet?openid={openid}&access_token={access_token}");
                        default:
                            return View("Error");
                    }
                }

                LogHelper.WriteLog(typeof(WechatController), "网页授权获取openid失败" + result);
                ViewData["result"] = result+"|code:"+code;
                return View();
            }
            else
            {
                
                return View("Error");
            }
            
        }

        public WxUser RegiseterWxUser(string openid)
        {

            if (!string.IsNullOrEmpty(openid))
            {
                WxUserDao userDao = new WxUserDao();


                //1 查询是否已经存在ticket和nickname信息
                WxUser user = userDao.Find(openid);

                bool isExist = user != null;
                //2 如果没有nickname需获取用户信息
                if (user == null || string.IsNullOrEmpty(user.headimgurl) || string.IsNullOrEmpty(user.nickname))
                {
                    var userinfo = HttpUtility.HttpGet($"https://api.weixin.qq.com/cgi-bin/user/info?access_token={AccessTokenHelper.GetAccessToken()}&openid={openid}&lang=zh_CN", "", "utf-8");

                    JObject jobect = (JObject)JsonConvert.DeserializeObject(userinfo);
                    if ((string)jobect["subscribe"] == "0")
                    {
                        LogHelper.WriteLog(typeof(WechatController), "未关注");
                        return null;
                    }

                    user = new WxUser();
                    user.openid = openid;
                    user.headimgurl = (string)jobect["headimgurl"];
                    user.nickname = (string)jobect["nickname"];

                    user.ticket = WeixinHelper.CreateTempQRCode(openid);
                    user.ticketcreatedatetime = DateTime.Now;
                    user.tj_openid = (string)jobect["qr_scene_str"];
                    user.isSubscribe = "是";

                    if (!isExist)
                    {
                        userDao.Add(user);
                    }
                    else
                    {
                        userDao.Update(user);
                    }

                }
                else if (string.IsNullOrEmpty(user.ticket)) //3 如果没有ticket信息，则需要创建ticket，并存库
                {
                    user.ticket = WeixinHelper.CreateTempQRCode(openid);
                    user.ticketcreatedatetime = DateTime.Now;
                    user.isSubscribe = "是";
                    userDao.Update(user);
                }
                else if(user.isSubscribe=="否")
                {
                    string sqlStr = "update tb_weixin_info set isSubscribe='是' where openid='" + user.openid + "'";
                    MySqlHelper helper = new MySqlHelper();
                    helper.ExecuteSql(sqlStr);
                }
                return user;
            }
            return null;
        }

        public JsonResult GetDetailByTaobao(string id)
        {
            string url = $"https://h5api.m.taobao.com/h5/mtop.taobao.detail.getdesc/6.0/?id={id}&radom="+ new Random().Next();
            string htmlChild = HttpUtility.HttpGet(url, "", "utf-8");
            var jobject = (JObject)JsonConvert.DeserializeObject(htmlChild.Replace("max-width:750.0px","width:100%"));
            return Json(new { data = (string)jobject["data"]["pcDescContent"] });
        }

        public JsonResult GetDetailImage(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success=false,message="id不能为空"});
            }

            string url = $"http://demo.huiyunmp.cn/index.php?s=tcms/goods&id={id}&pid=1000089_28822128&tbpid=mm_14645718_25722686_116308606";

            string htmlChild = HttpUtility.HttpGet(url, "", "utf-8");

            Document docNoval = NSoup.NSoupClient.Parse(htmlChild);


            Element ele = docNoval.GetElementById("descimg");

            var imagelist = ele.GetElementsByClass("lazy");

            IList<string> list = new List<string>();

            foreach (var g in imagelist)
            {
                list.Add(g.Attr("src"));
            }
            return Json(new { success = true, data = list });
        }

        public ActionResult Share(string openid,string access_token)
        {
            if (!string.IsNullOrEmpty(openid))
            {
                WxUserDao userDao = new WxUserDao();


                //1 查询是否已经存在ticket和nickname信息
                WxUser user = userDao.Find(openid);

                bool isExist = user != null;
                //2 如果没有nickname需获取用户信息
                if (user == null|| string.IsNullOrEmpty(user.headimgurl) || string.IsNullOrEmpty(user.nickname))
                {
                    var userinfo = HttpUtility.HttpGet($"https://api.weixin.qq.com/cgi-bin/user/info?access_token={AccessTokenHelper.GetAccessToken()}&openid={openid}&lang=zh_CN", "", "utf-8");

                    JObject jobect = (JObject)JsonConvert.DeserializeObject(userinfo);
                    if ((string)jobect["subscribe"] == "0")
                    {
                        return View("Error", new { message = "未关注" });
                    }

                    user = new WxUser();
                    user.openid = openid;
                    user.headimgurl = (string)jobect["headimgurl"];
                    user.nickname = (string)jobect["nickname"];

                    user.ticket = WeixinHelper.CreateTempQRCode(openid);
                    user.ticketcreatedatetime = DateTime.Now;

                    if (!isExist)
                    {
                        userDao.Add(user);
                    }
                    else
                    {
                        userDao.Update(user);
                    }
                    
                }else if (string.IsNullOrEmpty(user.ticket)) //3 如果没有ticket信息，则需要创建ticket，并存库
                {
                    user.ticket = WeixinHelper.CreateTempQRCode(openid);
                    user.ticketcreatedatetime = DateTime.Now;
                    userDao.Update(user);
                }

                //4 查询推广关注人数
                user.tj_count = userDao.FindTjCount(openid);
                
                return View(user);
            }
            else
            {
                return View("Error");
            }
            
        }

       

        #region 文本消息处理
        private string PostInput()
        {
            Stream s = Request.InputStream;
            byte[] b = new byte[s.Length];
            s.Read(b, 0, (int)s.Length);
            return Encoding.UTF8.GetString(b);
        }

        private async Task<string> ResponseMsgAsync(string weixin)// 服务器响应微信请求
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(weixin);//读取xml字符串
            XmlElement root = doc.DocumentElement;
            ExmlMsg xmlMsg = ExmlMsg.GetExmlMsg(root);
            //XmlNode MsgType = root.SelectSingleNode("MsgType");
            //string messageType = MsgType.InnerText;
            string messageType = xmlMsg.MsgType;//获取收到的消息类型。文本(text)，图片(image)，语音等。

            try
            {

                switch (messageType)
                {
                    //当消息为文本时
                    case "text":
                        return await TextCaseAsync(xmlMsg);

                    case "event":
                        LogHelper.WriteLog(typeof(WechatController), $"openid为{xmlMsg.FromUserName}的用户访问事件，Event name为{xmlMsg.EventName},Event key为{xmlMsg.EventKey}");
                        if (!string.IsNullOrEmpty(xmlMsg.EventName) && xmlMsg.EventName.Trim() == "subscribe")
                        {
                            //刚关注时的时间，用于欢迎词  
                            int nowtime = ConvertDateTimeInt(DateTime.Now);
                            string msg = "/:rose 亲，您好，我是网购省钱助手。\n您今后在淘宝天猫/拼多多 想购买的商品都可发给我，查询优惠券和返利！\n\n \ue231 <a href='https://mp.weixin.qq.com/s/ho825gKx2VP0oayX7pMdYQ'>点击淘宝优惠券返利教程</a> \n\n \ue231 <a href='https://mp.weixin.qq.com/s/1Nbyse1WBKpSOUjO3gOg0g'>点击拼多多优惠券返利教程</a> \n\n \ue231<a href='http://m.yshizi.cn'>淘宝优惠券商城</a> \n\n \ue231 <a href='https://mobile.yangkeduo.com/duo_cms_mall.html?pid=2495191_31302208cpsSign&state=&opt_id=-1&sort_type=1'>拼多多商城</a> \r\n ";
                            string resxml = "<xml><ToUserName><![CDATA[" + xmlMsg.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + xmlMsg.ToUserName + "]]></FromUserName><CreateTime>" + nowtime + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[" + msg + "]]></Content><FuncFlag>0</FuncFlag></xml>";

                            try
                            {
                                RegiseterWxUser(xmlMsg.FromUserName);
                            }catch(Exception ex)
                            {
                                LogHelper.WriteLog(typeof(WechatController), "处理订阅事件失败："+ex.Message);
                                LogHelper.WriteLog(typeof(WechatController), "处理订阅事件失败：" + ex.StackTrace);
                            }

                            return resxml;
                        }
                        else if(!string.IsNullOrEmpty(xmlMsg.EventName) && xmlMsg.EventName.Trim() == "unsubscribe")
                        {
                            try
                            {
                                Unsubscribe(xmlMsg.FromUserName);
                            }catch(Exception ex)
                            {
                                LogHelper.WriteLog(typeof(WechatController), "取消订阅事件出错：" + ex.Message);
                            }
                            
                            return "";
                        }
                        else if(!string.IsNullOrEmpty(xmlMsg.EventName) && xmlMsg.EventName.Trim().ToLower() == "click")
                        {
                            int nowtime = ConvertDateTimeInt(DateTime.Now);
                            switch (xmlMsg.EventKey.Trim().ToLower())
                            {
                                case "tuiguang":

                                    SendTextMessage(xmlMsg.FromUserName, "Hi,将您的专属海报分享到微信群或朋友圈，有5个小伙伴扫码关注后，联系客服将可获得5元现金红包。");
                                    //return "<xml><ToUserName><![CDATA[" + xmlMsg.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + xmlMsg.ToUserName + "]]></FromUserName><CreateTime>" + nowtime + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[推广菜单]]></Content><FuncFlag>0</FuncFlag></xml>";
                                    WxUserDao userDao = new WxUserDao();
                                    var user = userDao.Find(xmlMsg.FromUserName);
                                    if (user == null || string.IsNullOrEmpty(user.ticket))
                                    {
                                        user = RegiseterWxUser(xmlMsg.FromUserName);
                                    }
                                   else
                                    {
                                        string imagePath = Draw(user);
                                       
                                        string result = HttpUtility.UploadFile($"https://api.weixin.qq.com/cgi-bin/material/add_material?access_token={AccessTokenHelper.GetAccessToken()}&type=image", imagePath);

                                        JObject jObject = (JObject)JsonConvert.DeserializeObject(result);
                                        
                                        string media_id = (string)jObject["media_id"];
                                        if (!string.IsNullOrEmpty(media_id))
                                        {
                                          
                                            string resxml = "<xml><ToUserName><![CDATA[" + xmlMsg.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + xmlMsg.ToUserName + "]]></FromUserName><CreateTime>" + nowtime + "</CreateTime><MsgType><![CDATA[image]]></MsgType><Image><MediaId><![CDATA[" + media_id + "]]></MediaId></Image></xml>";
                                            return resxml;
                                        }
                                        LogHelper.WriteLog(typeof(WechatController), "上传专属推广图片素材失败" + result);
                                    }
                                      return "";
                                case "hongbao":

                                    string msg = "双11超级荭包\n\n活动时间：10月20日-11月10日，每天可领3次！好福利记得分享哦！\n——————\n领取口令：￥FgIib6pJREH￥\n——————\n\ue231①长按复制本段口令消息，\n\ue231②打开手机淘寳领取荭包！";
                                    return "<xml><ToUserName><![CDATA[" + xmlMsg.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + xmlMsg.ToUserName + "]]></FromUserName><CreateTime>" + nowtime + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[" + msg + "]]></Content><FuncFlag>0</FuncFlag></xml>";
                            }
                            return "";
                        }
                        else
                        {
                            return "";
                        }

                    case "image":
                        return "";
                        ;
                    case "voice":
                        return "";
                    case "vedio":
                        return "";
                    case "location":
                        return "";
                    case "link":
                        return "";
                    default:
                        return "";
                }

            }
            catch (Exception)
            {
                return "";
            }
        }

        #region 将datetime.now 转换为 int类型的秒
        /// <summary>
        /// datetime转换为unixtime
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private int ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }
        private int converDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }

        /// <summary>
        /// unix时间转换为datetime
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        private DateTime UnixTimeToTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }
        #endregion


        /// <summary>
        /// 验证微信签名
        /// </summary>
        /// * 将token、timestamp、nonce三个参数进行字典序排序
        /// * 将三个参数字符串拼接成一个字符串进行sha1加密
        /// * 开发者获得加密后的字符串可与signature对比，标识该请求来源于微信。
        /// <returns></returns>
        private bool CheckSignature()
        {
            string signature = Request.QueryString["signature"].ToString();
            string timestamp = Request.QueryString["timestamp"].ToString();
            string nonce = Request.QueryString["nonce"].ToString();
            string[] ArrTmp = { Token, timestamp, nonce };
            Array.Sort(ArrTmp);     //字典排序
            string tmpStr = string.Join("", ArrTmp);
            tmpStr = FormsAuthentication.HashPasswordForStoringInConfigFile(tmpStr, "SHA1");
            tmpStr = tmpStr.ToLower();
            if (tmpStr == signature)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Unsubscribe(string openid)
        {
            string sqlStr = "update tb_weixin_info set isSubscribe='否' where openid='" + openid + "'";
            MySqlHelper helper = new MySqlHelper();
            helper.ExecuteSql(sqlStr);
        }

        private string SendTextMessage(string openid,string message)
        {
            string url = $"https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token={AccessTokenHelper.GetAccessToken()}";

            string data = "{\"touser\":\""+openid+"\",\"msgtype\":\"text\",\"text\":{\"content\":\""+message+"\"}}";

             string result = HttpUtility.SendPostHttpRequest(url, "application/json", data);
            return result;
        }

        private async Task<string> TextCaseAsync(ExmlMsg xmlMsg)
        {
            int nowtime = ConvertDateTimeInt(DateTime.Now);
            string msg = "";

            try
            {
                string content = xmlMsg.Content.Trim();

                if (content.Contains("yangkeduo.com"))
                {
                    msg = await GetPddCouponAsync(xmlMsg);
                }
                else if (content.Length == 18 && new Regex("^[0-9]*$").IsMatch(content)) //匹配淘宝订单号
                {
                    msg = "您的订单编号已收到，稍后请留意订单状态更新通知。";

                }else if (content.Length == 22 && new Regex("^[0-9-]*$").IsMatch(content)) //匹配拼多多订单号
                {
                    msg = "您的订单编号已收到，稍后请留意订单状态更新通知。";
                }
                else
                {
                    msg = GetCoupon(xmlMsg);
                }
            }catch(Exception ex)
            {
                LogHelper.WriteLog(typeof(WechatController), "解析消息出错"+ex.Message);
                LogHelper.WriteLog(typeof(WechatController), ex.StackTrace);
                return "";
            }

           
                      
            if (msg == "")
            {
                return "";
            }

            //登记用户信息
            Task t = new Task(()=> {
                try
                {
                    RegiseterWxUser(xmlMsg.FromUserName);
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(typeof(WechatController), "登记用户信息失败：" + ex.Message);
                    LogHelper.WriteLog(typeof(WechatController), "登记用户信息失败：" + ex.StackTrace);
                }
            });
            t.Start();

           

            string resxml = "<xml><ToUserName><![CDATA[" + xmlMsg.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + xmlMsg.ToUserName + "]]></FromUserName><CreateTime>" + nowtime + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[" + msg + "]]></Content><FuncFlag>0</FuncFlag></xml>";
            return resxml;

        }

        public async System.Threading.Tasks.Task<string> GetPddCouponAsync(ExmlMsg xmlMsg)
        {
            string msg = xmlMsg.Content;
            Match m_goods = Regex.Match(msg, @"(?<=goods_id=)([0-9]*)");
            
            string goods_id = m_goods.Value;

            if (string.IsNullOrEmpty(goods_id))
            {
                LogHelper.WriteLog(typeof(WechatController), "获取拼多多goods id失败" + msg);
                return "";
            }
  
            DdkApi api = new DdkApi();

            var model = new DetailDdkGoodsRequestModel()
            {
                Type = "pdd.ddk.goods.detail",
                GoodsIdList = $"[{goods_id}]"
            };
            DetailDdkGoodsResponseModel result = null;
            try
            {
                result = await api.DetailDdkGoodsAsync(model);
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(typeof(WechatController), "调用拼多多获取商品详细信息失败" + ex.Message);
                return "";
            }
           

            var goods = result.GoodsDetailResponse.GoodsDetails.FirstOrDefault();

            if (goods==null) //无优惠券 无佣金
            {
                return " 亲，这款商品的优惠返利活动结束了~\n请换个商品试试吧。\n========================\n\ue231    <a href='https://mobile.yangkeduo.com/duo_cms_mall.html?pid=2495191_31302208cpsSign=CM2495191_31302208_3a1c1a0431608b9c1eb417183d57c1bdduoduo_type=2'>拼多多优惠券商城</a>\n下单确认收货后就能收到返利佣金啦~";
            }
            else if(goods.HasCoupon) //有优惠券 有佣金
            {
                try
                {
                    var promotionUrlModel = await api.GenerateDdkGoodsPromotionUrlAsync(new GenerateDdkGoodsPromotionUrlRequestModel
                    {
                        Type = "pdd.ddk.goods.promotion.url.generate",
                        PId = pdd_pid,
                        GoodsIdList = $"[{goods_id}]",
                        GenerateShortUrl = true,
                        CustomParameters = xmlMsg.FromUserName
                    });


                    return $" 亲，商品信息如下~\n========================\n{goods.GoodsName}\n【在售价】{((decimal)goods.MinNormalPrice) / 100}元\n【券后价】{Math.Round(((decimal)(goods.MinNormalPrice - goods.CouponDiscount)) / 100,2)}元\n【约返利】{Math.Round((decimal)((goods.MinNormalPrice - goods.CouponDiscount) * goods.PromotionRate) / 100000,2)}元\n\ue231 <a href='{promotionUrlModel.GoodsPromotionUrlGenerateResponse.GoodsPromotionUrlList.FirstOrDefault().Url}'>拼多多返利下单</a>\n下单确认收货后就能收到返利佣金啦~";

                }catch(Exception ex)
                {
                    LogHelper.WriteLog(typeof(WechatController), "调用拼多多获取推广链接失败" + ex.Message);
                    return "";
                }
             }
            else //无优惠券 但有佣金
            {
                try
                {
                    var promotionUrlModel = await api.GenerateDdkGoodsPromotionUrlAsync(new GenerateDdkGoodsPromotionUrlRequestModel
                    {
                        Type = "pdd.ddk.goods.promotion.url.generate",
                        GoodsIdList = $"[{goods_id}]",
                        PId = pdd_pid,
                        GenerateShortUrl = true,
                        CustomParameters = xmlMsg.FromUserName
                    });


                    return $"亲，商品信息如下~\n========================\n{goods.GoodsName}\n【在售价】{((decimal)goods.MinNormalPrice) / 100}元\n【约返利】{Math.Round((decimal)(goods.MinNormalPrice * goods.PromotionRate) / 100000,2)}元\n\ue231 <a href='{promotionUrlModel.GoodsPromotionUrlGenerateResponse.GoodsPromotionUrlList.FirstOrDefault().Url}'>拼多多返利下单</a>\n下单确认收货后就能收到返利佣金啦~";
                }catch(Exception ex)
                {
                    LogHelper.WriteLog(typeof(WechatController), "调用拼多多获取推广链接失败" + ex.Message);
                    return "";
                }
                
            }


        }

        /// <summary>
        /// 通过抓消息关键字获取item title
        /// </summary>
        /// <param name="xmlMsg"></param>
        /// <returns></returns>
        private string GetCoupon(ExmlMsg xmlMsg)
        {
            string itemInfo = xmlMsg.Content.Trim();

            string responeMessage = "";
            try
            {
                Match m_title = Regex.Match(itemInfo, @"【.*】");
                string temp = m_title.Value;
                if (!string.IsNullOrEmpty(temp))
                {
                    temp = temp.Substring(1, temp.Length - 2);
                }
                else
                {
                    return "";
                }

                if (temp.Contains("#手聚App"))
                {
                    int IndexofA = temp.IndexOf("宝贝不错:");
                    int IndexofB = temp.IndexOf("(分享自");
                    temp = temp.Substring(IndexofA + 5, IndexofB - IndexofA - 5);
                }

                string title = temp;

                ITopClient client = new DefaultTopClient(config.url, config.appkey, config.secret);

                TbkDgMaterialOptionalRequest req = new TbkDgMaterialOptionalRequest();
                req.AdzoneId = config.addzoneId;
                req.Platform = 2L;
                //req.Cat = category.Taobao_Categorys;
                req.PageSize = 100L;
                req.Q = title;
                //req.EndTkRate = 5000L;
                //req.StartTkRate = 500L;
                //req.HasCoupon = true;
                req.PageNo = 1L;
                TbkDgMaterialOptionalResponse rsp = client.Execute(req);


                if (rsp.ResultList.Count > 0)
                {
                    //获取淘宝短链接
                    Match m_url = Regex.Match(itemInfo, @"htt(p|ps):\/\/([\w\-]+(\.[\w\-]+)*\/)*[\w\-]+(\.[\w\-]+)*\/?(\?([\w\-\.,@?^=%&:\/~\+#]*)+)?");

                    if (m_url.Value == "")
                    {
                        return responeMessage;
                    }
                    var s = HttpUtility.HttpGet(m_url.Value, "", "utf-8");

                    //Match am_url = Regex.Match(s, @"(?<=var url = ')(.*)(?=')");
                    //获取宝贝item id
                    Match m_item = Regex.Match(s, @"(?<=m.taobao.com\/i)([0-9]*)");
                    string item_id = m_item.Value;

                    if (string.IsNullOrEmpty(item_id))
                    {
                        Match am_url = Regex.Match(s, @"(?<=var url = ')(.*)(?=')");
                        var htmlContent = HttpUtility.HttpGet(am_url.Value, "", "gbk");
                        Match re_m_item = Regex.Match(htmlContent, @"(?<=taobao.com/item.htm\?id=)([0-9]*)");
                        item_id = re_m_item.Value;
                    }

                    if (string.IsNullOrEmpty(item_id))
                    {
                        LogHelper.WriteLog(typeof(WechatController), "通过抓包方式未获取到宝贝item id");
                        var g = rsp.ResultList.Where(y => !string.IsNullOrEmpty(y.CouponId)).OrderByDescending(w => w.Volume).FirstOrDefault();
                        if (g == null)
                        {
                            responeMessage = responeMessage = "亲，这款商品的优惠返利活动结束了~\n请换一个商品试试吧。\n========================\n\ue231 <a href='http://m.yshizi.cn'>掏宝优惠券商城</a> \n";

                        }
                        else
                        {
                            var hongbao = (decimal.Parse(g.ZkFinalPrice) - decimal.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value)) * decimal.Parse(g.CommissionRate) / 10000 * commission_rate;
                            responeMessage = $"{g.Title}\n【在售价】{g.ZkFinalPrice}元\n【巻后价】{Math.Round(double.Parse(g.ZkFinalPrice) - double.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value), 2)} 元\n【约返利】{Math.Round(hongbao, 2)}元\n复制这条信息，打开「手机绹宝」领巻下单{config.GetTaobaoKePassword(g.CouponShareUrl, g.PictUrl + "_400x400.jpg")}\n==========================\n下单确认收货后就能收到返利佣金啦~";

                        }
                        return responeMessage;

                    }
                    else
                    {
                        float numid = 0;
                        try
                        {
                            numid = float.Parse(item_id);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteLog(typeof(WechatController), "解析宝贝item id出错" + ex.Message);
                            var g = rsp.ResultList.Where(y => !string.IsNullOrEmpty(y.CouponId)).OrderByDescending(y => y.Volume).FirstOrDefault();

                            var hongbao = (decimal.Parse(g.ZkFinalPrice) - decimal.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value)) * decimal.Parse(g.CommissionRate) / 10000 * commission_rate;
                            responeMessage = $"{g.Title}\n【在售价】{g.ZkFinalPrice}元\n【巻后价】{Math.Round(double.Parse(g.ZkFinalPrice) - double.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value), 2)} 元\n【约返利】{Math.Round(hongbao, 2)}元\n复制这条信息，打开「手机绹宝」领巻下单{config.GetTaobaoKePassword(g.CouponShareUrl, g.PictUrl + "_400x400.jpg")}\n==========================\n下单确认收货后就能收到返利佣金啦~";

                            return responeMessage;

                        }


                        foreach (var g in rsp.ResultList)
                        {
                            if (g.NumIid == numid)
                            {
                                if (string.IsNullOrEmpty(g.CouponInfo))
                                {
                                    var hongbao = decimal .Parse(g.ZkFinalPrice) * decimal.Parse(g.CommissionRate) / 10000 * commission_rate;
                                    
                                    responeMessage = $"{g.Title}\n【在售价】{g.ZkFinalPrice}元\n【约返利】{Math.Round(hongbao, 2)}元\n复制这条信息，打开「手机绹宝」领巻下单{config.GetTaobaoKePassword(g.Url, g.PictUrl + "_400x400.jpg")}\n==========================\n下单确认收货后就能收到返利佣金啦~";
                                    return responeMessage;
                                }
                                else
                                {
                                    var hongbao = (decimal.Parse(g.ZkFinalPrice)- decimal.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value)) * decimal.Parse(g.CommissionRate) / 10000 * commission_rate;
                                    responeMessage = $"{g.Title}\n【在售价】{g.ZkFinalPrice}元\n【巻后价】{Math.Round(double.Parse(g.ZkFinalPrice) - double.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value), 2)} 元\n【约返利】{Math.Round(hongbao, 2)}元\n复制这条信息，打开「手机绹宝」领巻下单{config.GetTaobaoKePassword(g.CouponShareUrl, g.PictUrl + "_400x400.jpg")}\n==========================\n下单确认收货后就能收到返利佣金啦~";
                                    return responeMessage;
                                }
                            }
                        }

                        //没有找到，有相似宝贝推荐
                        var w = rsp.ResultList.Where(y => !string.IsNullOrEmpty(y.CouponId)).OrderByDescending(y => y.Volume).FirstOrDefault();

                        if (w == null)
                        {
                            responeMessage = "亲，这款商品的优惠返利活动结束了~\n请换一个商品试试吧。\n========================\n\ue231 <a href='http://m.yshizi.cn'>掏宝优惠券商城</a> \n";
                        }
                        else
                        {
                            var hongbao = (decimal.Parse(w.ZkFinalPrice) - decimal.Parse(Regex.Match(w.CouponInfo, "减" + @"(\d+)").Groups[1].Value)) * decimal.Parse(w.CommissionRate) / 10000 * commission_rate;

                            responeMessage = $"亲，这款商品的优惠返利活动结束了~\n已为你推荐以下宝贝。\n==========================\n{w.Title}\n【在售价】{w.ZkFinalPrice}元\n【巻后价】{Math.Round(double.Parse(w.ZkFinalPrice) - double.Parse(Regex.Match(w.CouponInfo, "减" + @"(\d+)").Groups[1].Value), 2)} 元\n【约返利】{Math.Round(hongbao, 2)}元\n复制这条信息，打开「手机绹宝」领巻下单{config.GetTaobaoKePassword(w.CouponShareUrl, w.PictUrl + "_400x400.jpg")}\n==========================\n下单确认收货后就能收到返利佣金啦~";
                        }

                        return responeMessage;

                    }


                }
                else
                {
                    responeMessage = "亲，这款商品的优惠返利活动结束了~\n请换一个商品试试吧。\n========================\n\ue231 <a href='http://m.yshizi.cn'>掏宝优惠券商城</a> \n";

                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(WechatController), "返回消息异常" + ex.Message);

            }

            return responeMessage;

        }

        private string getText(ExmlMsg xmlMsg)
        {
            string con = xmlMsg.Content.Trim();

            string responeMessage = "";

            try { 
                //获取淘宝短链接
                Match m_url = Regex.Match(con, @"htt(p|ps):\/\/([\w\-]+(\.[\w\-]+)*\/)*[\w\-]+(\.[\w\-]+)*\/?(\?([\w\-\.,@?^=%&:\/~\+#]*)+)?");

                if (m_url.Value == "")
                {
                    return responeMessage;
                }
                var s = HttpUtility.HttpGet(m_url.Value, "","utf-8");

                Match am_url = Regex.Match(s, @"(?<=var url = ')(.*)(?=')");

                Match m_item = Regex.Match(s, @"(?<=m.taobao.com\/i)([0-9]*)");

                var de = HttpUtility.HttpGet(am_url.Value, "", "gbk");

                Document docNoval = NSoup.NSoupClient.Parse(de);


                NSoup.Select.Elements ele = docNoval.GetElementsByTag("meta");

                string title = "";

                foreach (var g in ele)
                {
                    if (g.Attr("name") == "keywords")
                    {
                        title = g.Attr("content").ToString();
                    }
                }

                ITopClient client = new DefaultTopClient(config.url, config.appkey, config.secret);
                TbkDgItemCouponGetRequest req = new TbkDgItemCouponGetRequest();
                req.AdzoneId = config.addzoneId;
                req.Platform = 2L;

                req.PageSize = config.pageSize + 10;
                req.Q = title;
                req.PageNo = 1;

                LogHelper.WriteLog(typeof(WechatController), "title:"+title);

                TbkDgItemCouponGetResponse rsp = client.Execute(req);

                if (rsp.Results.Count > 0)
                {
                    bool isCoupon = false;
                    foreach (var g in rsp.Results)
                    {
                        if (long.Parse(m_item.Value) == g.NumIid)
                        {
                            isCoupon = true;
                            responeMessage = $"亲，已为您查找到优惠券。\n==========================\n{g.Title}\n【原价】{g.ZkFinalPrice}元\n【巻后价】{Math.Round(double.Parse(g.ZkFinalPrice) - double.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value))} 元\n复制这条信息，打开「手机绹宝」领巻下单{config.GetTaobaoKePassword(g.CouponClickUrl, g.PictUrl + "_400x400.jpg")}\n";
                            return responeMessage;
                        }
                    }

                    if (!isCoupon)
                    {
                        responeMessage = "抱歉亲，该宝贝优惠券已经领完。\n========================\n您可点击 <a href='http://m.yshizi.cn'>搜索宝贝</a> \n或点击下方“领优惠券”菜单，查找更多宝贝！";
                    }
                }
                else
                {
                    responeMessage = "抱歉亲，该宝贝没有优惠券。\n========================\n您可点击 <a href='http://m.yshizi.cn'>搜索宝贝</a> \n或点击下方“领优惠券”菜单，查找更多宝贝！";
                }

            }catch(Exception ex)
            {
                LogHelper.WriteLog(typeof(WechatController), "返回消息异常" + ex.Message);
                
            }

            return responeMessage;
        }

        

        
        #endregion

        /// <summary>
        /// 生成带二维码的专属推广图片
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public string Draw(WxUser user)
        {

            string path = Server.MapPath("/Content/images/tg.jpg");

            System.Drawing.Image imgSrc = System.Drawing.Image.FromFile(path);

            System.Drawing.Image qrCodeImage = ReduceImage("https://mp.weixin.qq.com/cgi-bin/showqrcode?ticket="+user.ticket, 240, 240);


            Image titleImage = ReduceImage(user.headimgurl, 100, 100);

            using (Graphics g = Graphics.FromImage(imgSrc))
            {
                //画专属推广二维码
                g.DrawImage(qrCodeImage, new Rectangle(imgSrc.Width - qrCodeImage.Width - 200,
                imgSrc.Height - qrCodeImage.Height - 200,
                qrCodeImage.Width,
                qrCodeImage.Height),
                0, 0, qrCodeImage.Width, qrCodeImage.Height, GraphicsUnit.Pixel);

                //画头像
                g.DrawImage(titleImage, 8, 8, titleImage.Width, titleImage.Height);

                Font font = new Font("宋体", 30, FontStyle.Bold);

                g.DrawString(user.nickname, font, new SolidBrush(Color.Red), 110, 10);
            }
            string newpath = Server.MapPath(@"/Content/images/newtg_" + Guid.NewGuid().ToString() + ".jpg");
            imgSrc.Save(newpath, System.Drawing.Imaging.ImageFormat.Jpeg);

            return newpath;
            //16031923
        }

        public Image ReduceImage(string url, int toWidth, int toHeight)
        {
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream(); // 到这里都没有错，这里的Stream responseStream已经是字节流了。 // 接下来要放到Response里面 

            Image originalImage = Image.FromStream(responseStream);
            if (toWidth <= 0 && toHeight <= 0)
            {
                return originalImage;
            }
            else if (toWidth > 0 && toHeight > 0)
            {
                if (originalImage.Width < toWidth && originalImage.Height < toHeight)
                    return originalImage;
            }
            else if (toWidth <= 0 && toHeight > 0)
            {
                if (originalImage.Height < toHeight)
                    return originalImage;
                toWidth = originalImage.Width * toHeight / originalImage.Height;
            }
            else if (toHeight <= 0 && toWidth > 0)
            {
                if (originalImage.Width < toWidth)
                    return originalImage;
                toHeight = originalImage.Height * toWidth / originalImage.Width;
            }
            Image toBitmap = new Bitmap(toWidth, toHeight);
            using (Graphics g = Graphics.FromImage(toBitmap))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.Clear(Color.Transparent);
                g.DrawImage(originalImage,
                            new Rectangle(0, 0, toWidth, toHeight),
                            new Rectangle(0, 0, originalImage.Width, originalImage.Height),
                            GraphicsUnit.Pixel);
                originalImage.Dispose();
                return toBitmap;
            }
        }

    }
}
