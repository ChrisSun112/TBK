using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tbk.Common;
using Tbk.DAL;
using Tbk.Entity;

namespace Tbk.mweb.Controllers
{
    public class MeController : Controller
    {
        // GET: Me
        public ActionResult Index(string openid)
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
                    var userinfo = Common.HttpUtility.HttpGet($"https://api.weixin.qq.com/cgi-bin/user/info?access_token={AccessTokenHelper.GetAccessToken()}&openid={openid}&lang=zh_CN", "", "utf-8");

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

                }


                return View(user);
            }
            else
            {
                return View("Error");
            }  
        }

        public ActionResult Wallet(string openid)
        {
            if (!string.IsNullOrEmpty(openid))
            {
                WxUserDao userDao = new WxUserDao();
               
                WxUser user = userDao.Find(openid);
                if (user == null)
                {
                    return View("Error");
                }
                return View(user);
            }
            else
            {
                return View("Error");
            }
                
        }

        /// <summary>
        /// 申请提现post
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="cashouttotal"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyCashOut(string openid,decimal cashouttotal)
        {
            CashOutDao cashOutDao = new CashOutDao();

            try
            {
                string result = cashOutDao.CashOut(openid, cashouttotal);
                if (result == "成功")
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = result });
                }

            }catch(Exception ex)
            {
                LogHelper.WriteLog(typeof(MeController), "提现出错:" + ex.Message);
                LogHelper.WriteLog(typeof(MeController), "提现出错:" + ex.StackTrace);
                return Json(new { success = false, message = "提现出错，请联系客服处理。" });
            }
            
        }

        /// <summary>
        /// 查询最近6个月内提现明细
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public JsonResult GetCashOutDesc(string openid)
        {
            CashOutDao cashOutDao = new CashOutDao();

            try
            {
                var list = cashOutDao.GetCashOut(openid);
                return Json(new { success = true, data = list });

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(MeController), "查询提现明细出错:" + ex.Message);
                LogHelper.WriteLog(typeof(MeController), "查询提现明细出错:" + ex.StackTrace);
                return Json(new { success = false, message = "查询提现明细出错，请联系客服处理。" });
            }
        }

        public ActionResult BingOrder()
        {
            return View();
        }

        public ActionResult Order()
        {
            return View();
        }

        public JsonResult OrderBind(string orderNo,string openId)
        {
            UnbindOrderDao dao = new UnbindOrderDao();
            try
            {
                if(dao.BindOrder(orderNo, openId))
                {
                    return Json(new { success = 0 });
                }
                else
                {
                    //没有查询到订单，返回-1
                    return Json(new { success = -1 });
                }
            }catch(Exception ex)
            {
                //内部错误，返回-2
                LogHelper.WriteLog(typeof(MeController), "绑定订单出错，" + ex.Message);
                return Json(new { success = -1 });
            }
            
        }

        public JsonResult GetCashBackDesc(string openid)
        {
            CashBackDao cashBackDao = new CashBackDao();

            try
            {
                var list = cashBackDao.GetCashBack(openid);
                return Json(new { success = true, data = list });

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(MeController), "查询收入明细出错:" + ex.Message);
                LogHelper.WriteLog(typeof(MeController), "查询收入明细出错:" + ex.StackTrace);
                return Json(new { success = false, message = "查询收入明细出错，请联系客服处理。" });
            }
        }

        public JsonResult GetOrders(string openid,string channel)
        {
            try
            {
                OrderDao dao = new OrderDao();
                return Json(new { success = true, data = dao.GetOrders(openid, channel) });
            }catch(Exception ex)
            {
                LogHelper.WriteLog(typeof(MeController), "查询订单列表出错:" + ex.Message);
                LogHelper.WriteLog(typeof(MeController), "查询订单列表出错:" + ex.StackTrace);
                return Json(new { success = false, message = "查询订单列表出错，请联系客服处理。" });
            }
            
        }
    }
}