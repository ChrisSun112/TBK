using Tbk.DAL;
using Tbk.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tbk.Common;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;
using Tbk.mweb;

namespace TaobaoKe.Controllers
{
    public class HomeController : Controller
    {

        [HttpGet]
        [ActionName("Index")]
        public ActionResult Get()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Index")]
        public ActionResult Post(int id = 0, int pageno = 1)
        {

            ItemDao itemDao = new ItemDao();
            IList<ItemEntity> list = null;
            if (id == 0)
            {
                list = itemDao.GetAllItems(pageno);
            }
            else
            {
                list = itemDao.GetItemsByCategory(id, pageno);
            }
            return Json(new { success = true, data = list });
        }


      

        public ActionResult SearchDetail(ItemEntity item)
        {
            return View(item);
        }

        public ActionResult Detail(int id = 0)
        {
            ItemDao itemDao = new ItemDao();
            var item = itemDao.GetItem(id);

            if (item == null)
            {
                return RedirectToAction("Error");
            }

            return View(item);
        }

        public ActionResult Detail9k9(int id = 0)
        {
            ItemDao itemDao = new ItemDao();
            var item = itemDao.GetItem_9k9(id);

            if (item == null)
            {
                return RedirectToAction("Error");
            }

            return View(item);
        }

        public ActionResult Detail20(int id = 0)
        {
            ItemDao itemDao = new ItemDao();
            var item = itemDao.GetItem_20(id);

            if (item == null)
            {
                return RedirectToAction("Error");
            }

            return View(item);
        }



        public ActionResult Search()
        {

            //ITopClient client = new DefaultTopClient(config.url, config.appkey, config.secret);
            //TbkDgItemCouponGetRequest req = new TbkDgItemCouponGetRequest();
            //req.AdzoneId = config.addzoneId;
            //req.Platform = 2L;

            //req.PageSize = config.pageSize + 10;
            //req.Q = q;
            //req.PageNo = pageno;

            //TbkDgItemCouponGetResponse rsp = client.Execute(req);

            //Common.LogHelper.WriteLog(typeof(HomeController), "搜索记录:" + q);

            //return View(rsp);
            return View();
        }

        
        public JsonResult GetCoupons(string q, int pageno=1)
        {
            ITopClient client = new DefaultTopClient(config.url, config.appkey, config.secret);
            TbkDgItemCouponGetRequest req = new TbkDgItemCouponGetRequest();
            req.AdzoneId = config.addzoneId;
            req.Platform = 2L;

            req.PageSize = 40L;
            req.Q = q;
            req.PageNo = pageno;

            LogHelper.WriteLog(typeof(HomeController), "搜索记录:" + q);

            TbkDgItemCouponGetResponse rsp = client.Execute(req);

            if (rsp.Results != null && rsp.Results.Count > 0)
            {
                return Json(new { success = true, data = rsp.Results.OrderByDescending(s=>s.Volume) });
            }
            else {
                return Json(new { success=false});
            }


        }

        public ActionResult Tuiguang9k9()
        {
            return View();

        }

        public JsonResult Get9K9(int id = 0,int pageno = 1){

            ItemDao itemDao = new ItemDao();
            IList<ItemEntity> list = null;
            if (id == 0)
            {
                list = itemDao.GetAll9k9Items(pageno);
            }
            else
            {
                list = itemDao.Get9k9ItemsByCategory(id, pageno);
            }
            return Json(new { success = true, data = list });
        }

        public ActionResult Tb20()
        {
           
            return View();

           
        }

        public JsonResult Get20(int id = 0, int pageno = 1)
        {
            ItemDao itemDao = new ItemDao();
            IList<ItemEntity> list = null;
            if (id == 0)
            {
                list = itemDao.GetAll20Items(pageno);
            }
            else
            {
                list = itemDao.Get20ItemsByCategory(id, pageno);
            }
            return Json(new { success = true, data = list });
        }



        public ActionResult Search1()
        {
            return View();
        }

        public ActionResult Jusp()
        {
           
            return View();
        }

        public JsonResult GetJusp(int id = 0, int pageno = 1)
        {
            PTItemDao itemDao = new PTItemDao();
            IList<PTItemEntity> list = null;
            if (id == 0)
            {
                list = itemDao.GetAllItems(pageno, config.pageSize);
            }
            else
            {
                list = itemDao.GetItemsByCategory(id, pageno, config.pageSize);
            }
            return Json(new { success = true, data = list });
        }
        public ActionResult JDetail(long id=0)
        {
            
            PTItemDao itemDao = new PTItemDao();
            var item = itemDao.GetItem(id);

            if (item == null)
            {
                return RedirectToAction("Error");
            }

            return View(item);
        }

        public ActionResult Error()
        {
            return View();
        }
    }
}