using sare.Weixin.BLL;
using sare.Weixin.Model;
using sare.WeiXin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TaobaoKe.Controllers
{
    public class VipController : Controller
    {
        //
        // GET: /Vip/

        public ActionResult Add(string buguid)
        {
            try
            {
                BuildingBiz biz = new BuildingBiz();
                var list = biz.GetProjNameByBuGuid(buguid);
                return View(list);
                
            }
            catch(Exception ex)
            {
                LogHelper.WriteLog(typeof(VipController), ex.Message);
                return View();
            }
           
            
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Sava(PRememberEntity pRemeber)
        {
            PRememberBiz biz = new PRememberBiz();

            try
            {
                if (biz.Insert(pRemeber))
                {
                    //LogHelper.WriteLog(typeof(VipController), "申请入会成功。姓名："+pRemeber.MemName+" 手机号:"+pRemeber.MobileTel);
                    return Json(new { success = true });
                }
                else
                {
                    //LogHelper.WriteLog(typeof(VipController), "申请入会失败。姓名：" + pRemeber.MemName + " 手机号:" + pRemeber.MobileTel);
                    return Json(new { success = false });
                }
            }catch(Exception ex)
            {
                LogHelper.WriteLog(typeof(VipController), "申请入会失败。姓名：" + pRemeber.MemName + " 手机号:" + pRemeber.MobileTel+"\n" + ex.Message);
                return Json(new { success = false });
            }

            
        }
    }
}
