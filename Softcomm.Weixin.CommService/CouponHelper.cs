using PddOpenSdk.Models.Request.Ddk;
using PddOpenSdk.Models.Response.Ddk;
using PddOpenSdk.Services;
using PddOpenSdk.Services.PddApi;
using Senparc.Weixin.MP.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tbk.Common;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace Softcomm.Weixin.CommService
{
    public class CouponHelper
    {
        private static string taobaoApiUrl = "http://gw.api.taobao.com/router/rest";
        private static string taobaoAppkey = "25065474";
        private static string taobaoSecret = "79206c1c0c7531a3d4fb41c1095e1a7d";
        private static long addzoneId = 23004550492;
        private static decimal commission_rate = decimal.Parse(ConfigurationManager.AppSettings["commission_rate"].ToString());
        private static string pdd_pid = ConfigurationManager.AppSettings["pdd_pid"].ToString();

        private static HaojingkeApi hjkApi = new HaojingkeApi("f7e704a8ae9fc8bc", "1534105049", "http://api-gw.haojingke.com/index.php/api/index/myapi");

        public static string GetTaobaoCoupon(RequestMessageText responseMessageText)
        {
            string itemInfo = responseMessageText.Content.Trim();

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

                //通过商品关键字查询商品
                ITopClient client = new DefaultTopClient(taobaoApiUrl,taobaoAppkey, taobaoSecret);
                TbkDgMaterialOptionalRequest req = new TbkDgMaterialOptionalRequest();
                req.AdzoneId = addzoneId;
                req.Platform = 2L;
                
                req.PageSize = 100L;
                req.Q = title;
           
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
                        //LogHelper.WriteLog(typeof(WechatController), "通过抓包方式未获取到宝贝item id");
                        var g = rsp.ResultList.Where(y => !string.IsNullOrEmpty(y.CouponId)).OrderByDescending(w => w.Volume).FirstOrDefault();
                        if (g == null)
                        {
                            responeMessage = ConfigurationManager.AppSettings["tbk_nocoupon_msg"].Replace("\\n", "\n").Replace("\\ue231", "\ue231");

                        }
                        else
                        {
                            var hongbao = (decimal.Parse(g.ZkFinalPrice) - decimal.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value)) * decimal.Parse(g.CommissionRate) / 10000 * commission_rate;
                            
                            responeMessage = $"{g.Title}\n【在售价】{g.ZkFinalPrice}元\n【巻后价】{Math.Round(double.Parse(g.ZkFinalPrice) - double.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value), 2)} 元\n复制这条信息，打开「手机绹宝」领巻下单{GetTaobaoKePassword(g.CouponShareUrl, g.PictUrl + "_400x400.jpg")}\n";

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

                          //通过淘宝链接没有获取到item id,显示销量最高商品
                            var g = rsp.ResultList.Where(y => !string.IsNullOrEmpty(y.CouponId)).OrderByDescending(y => y.Volume).FirstOrDefault();

                            var hongbao = (decimal.Parse(g.ZkFinalPrice) - decimal.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value)) * decimal.Parse(g.CommissionRate) / 10000 * commission_rate;
                            responeMessage = $"{g.Title}\n【在售价】{g.ZkFinalPrice}元\n【巻后价】{Math.Round(double.Parse(g.ZkFinalPrice) - double.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value), 2)} 元\n复制这条信息，打开「手机绹宝」领巻下单{GetTaobaoKePassword(g.CouponShareUrl, g.PictUrl + "_400x400.jpg")}\n";

                            return responeMessage;

                        }

                        //在接口返回的商品中找查询的商品
                        foreach (var g in rsp.ResultList)
                        {
                            if (g.NumIid == numid)
                            {
                                if (string.IsNullOrEmpty(g.CouponInfo))
                                {
                                    var hongbao = decimal.Parse(g.ZkFinalPrice) * decimal.Parse(g.CommissionRate) / 10000 * commission_rate;

                                    responeMessage = $"{g.Title}\n【在售价】{g.ZkFinalPrice}元\n【约返利】{Math.Round(hongbao, 2)}元\n复制这条信息，打开「手机绹宝」领巻下单{GetTaobaoKePassword(g.Url, g.PictUrl + "_400x400.jpg")}\n==========================\n下单确认收货后就能收到返利佣金啦~\n 点击查看  <a href='http://mp.weixin.qq.com/s?__biz=Mzg2NTAxOTEyMA==&mid=100000146&idx=1&sn=62405c8df3db46e74940aefb9ac3737b&chksm=4e61340d7916bd1bf645afbc6d10c1f19561d7fa59847516c01e64c0791e6d544f4f56c4f498#rd'>如何领取返利</a>";
                                    return responeMessage;
                                }
                                else
                                {
                                    var hongbao = (decimal.Parse(g.ZkFinalPrice) - decimal.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value)) * decimal.Parse(g.CommissionRate) / 10000 * commission_rate;
                                    responeMessage = $"{g.Title}\n【在售价】{g.ZkFinalPrice}元\n【巻后价】{Math.Round(double.Parse(g.ZkFinalPrice) - double.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value), 2)} 元\n复制这条信息，打开「手机绹宝」领巻下单{GetTaobaoKePassword(g.CouponShareUrl, g.PictUrl + "_400x400.jpg")}\n";
                                    return responeMessage;
                                }
                            }
                        }

                        //没有找到，有相似宝贝推荐
                        var w = rsp.ResultList.Where(y => !string.IsNullOrEmpty(y.CouponId)).OrderByDescending(y => y.Volume).FirstOrDefault();

                        if (w == null)
                        {
                            responeMessage = ConfigurationManager.AppSettings["tbk_nocoupon_msg"].Replace("\\n", "\n").Replace("\\ue231", "\ue231");
                        }
                        else
                        {
                            var hongbao = (decimal.Parse(w.ZkFinalPrice) - decimal.Parse(Regex.Match(w.CouponInfo, "减" + @"(\d+)").Groups[1].Value)) * decimal.Parse(w.CommissionRate) / 10000 * commission_rate;

                            responeMessage = $"/:rose 亲，这款商品的优惠返利活动结束了~\n已为你推荐以下宝贝。\n==========================\n{w.Title}\n【在售价】{w.ZkFinalPrice}元\n【巻后价】{Math.Round(double.Parse(w.ZkFinalPrice) - double.Parse(Regex.Match(w.CouponInfo, "减" + @"(\d+)").Groups[1].Value), 2)} 元\n复制这条信息，打开「手机绹宝」领巻下单{GetTaobaoKePassword(w.CouponShareUrl, w.PictUrl + "_400x400.jpg")}\n";
                        }

                        return responeMessage;

                    }


                }
                else
                {
                    responeMessage = ConfigurationManager.AppSettings["tbk_nocoupon_msg"].Replace("\\n", "\n").Replace("\\ue231", "\ue231");

                }

            }
            catch (Exception ex)
            {
                //LogHelper.WriteLog(typeof(WechatController), "返回消息异常" + ex.Message);

            }

            return responeMessage;

        }

        public static async Task<string> GetPDDCouponAsync(RequestMessageText responseMessageText)
        {
            PddCommonApi.ClientId = "d8172a66ddf14220beac58e8eddca0d9";
            PddCommonApi.ClientSecret = "d5993ce16cc0d1bd35403176f35544962e786645";
            PddCommonApi.RedirectUri = "RedirectUri";
            PddCommonApi.AccessToken = "";

            string msg = responseMessageText.Content;
            Match m_goods = Regex.Match(msg, @"(?<=goods_id=)([0-9]*)");

            string goods_id = m_goods.Value;

            if (string.IsNullOrEmpty(goods_id))
            {
                //LogHelper.WriteLog(typeof(WechatController), "获取拼多多goods id失败" + msg);
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
            catch (Exception ex)
            {
                //LogHelper.WriteLog(typeof(WechatController), "调用拼多多获取商品详细信息失败" + ex.Message);
                return "";
            }


            var goods = result.GoodsDetailResponse.GoodsDetails.FirstOrDefault();

            if (goods == null) //无优惠券 无佣金
            {
                return "/:rose 亲，这款商品的优惠返利活动结束了~\n请换个商品试试吧。\n========================\n\ue231    <a href='https://mobile.yangkeduo.com/duo_cms_mall.html?pid=2495191_31302208cpsSign=CM2495191_31302208_3a1c1a0431608b9c1eb417183d57c1bdduoduo_type=2'>拼多多优惠券商城</a>\n下单确认收货后就能收到返利佣金啦~";
            }
            else if (goods.HasCoupon) //有优惠券 有佣金
            {
                try
                {
                    var promotionUrlModel = await api.GenerateDdkGoodsPromotionUrlAsync(new GenerateDdkGoodsPromotionUrlRequestModel
                    {
                        Type = "pdd.ddk.goods.promotion.url.generate",
                        PId = pdd_pid,
                        GoodsIdList = $"[{goods_id}]",
                        GenerateShortUrl = true,
                        CustomParameters = responseMessageText.FromUserName
                    });


                    return $"/:rose 亲，商品信息如下~\n========================\n{goods.GoodsName}\n【在售价】{((decimal)goods.MinGroupPrice) / 100}元\n【券后价】{Math.Round(((decimal)(goods.MinGroupPrice - goods.CouponDiscount.Value)) / 100, 2)}元\n\ue231 <a href='{promotionUrlModel.GoodsPromotionUrlGenerateResponse.GoodsPromotionUrlList.FirstOrDefault().Url}'>点击这里下单</a>\n下单确认收货后就能收到返利佣金啦~";

                    //return $"/:rose 亲，商品信息如下~\n========================\n{goods.GoodsName}\n【在售价】{((decimal)goods.MinGroupPrice) / 100}元\n【券后价】{Math.Round(((decimal)(goods.MinGroupPrice - goods.CouponDiscount.Value)) / 100,2)}元\n【约返利】{Math.Round((decimal)((goods.MinNormalPrice - goods.CouponDiscount.Value) * goods.PromotionRate) / 100000,2)}元\n\ue231 <a href='{promotionUrlModel.GoodsPromotionUrlGenerateResponse.GoodsPromotionUrlList.FirstOrDefault().Url}'>点击这里下单</a>\n下单确认收货后就能收到返利佣金啦~";

                }
                catch (Exception ex)
                {
                    //LogHelper.WriteLog(typeof(WechatController), "调用拼多多获取推广链接失败" + ex.Message);
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
                        CustomParameters = responseMessageText.FromUserName
                    });


                    return $"/:rose 亲，商品信息如下~\n========================\n{goods.GoodsName}\n【在售价】{((decimal)goods.MinGroupPrice) / 100}元\n【约返利】{Math.Round((decimal)(goods.MinGroupPrice * goods.PromotionRate) / 100000, 2)}元\n\ue231 <a href='{promotionUrlModel.GoodsPromotionUrlGenerateResponse.GoodsPromotionUrlList.FirstOrDefault().Url}'>点击这里下单</a>\n下单确认收货后就能收到返利佣金啦~\n\n 点击查看  <a href='http://mp.weixin.qq.com/s?__biz=Mzg2NTAxOTEyMA==&mid=100000146&idx=1&sn=62405c8df3db46e74940aefb9ac3737b&chksm=4e61340d7916bd1bf645afbc6d10c1f19561d7fa59847516c01e64c0791e6d544f4f56c4f498#rd'>如何领取返利</a>";
                }
                catch (Exception ex)
                {
                    //LogHelper.WriteLog(typeof(WechatController), "调用拼多多获取推广链接失败" + ex.Message);
                    return "";
                }

            }
            
        }

        public static string GetJDCoupon(RequestMessageText requestMessageText)
        {
           
            string msg = requestMessageText.Content;
            Match m_goods = Regex.Match(msg, @"(?<=product\/)([0-9]*)|(?<=sku=)([0-9]*)");

            string skuId = m_goods.Value;

            if (string.IsNullOrEmpty(skuId))
            {
               // LogHelper.WriteLog(typeof(WechatController), "获取京东skuid失败" + msg);
                return "";
            }

            try
            {
                var hjkGoodsDetail = hjkApi.GetJDGoodsDetail(skuId);

                if (hjkGoodsDetail.StatusCode == 200 && hjkGoodsDetail.Data != null && !string.IsNullOrEmpty(hjkGoodsDetail.Data.CouponList))
                {
                    var model = hjkApi.GetUnionUrl(skuId, hjkGoodsDetail.Data.CouponList);
                    if (model != null && model.StatusCode == 200 && !string.IsNullOrEmpty(model.Data))
                    {
                        return $"{hjkGoodsDetail.Data.SkuName}\n【在售价】{hjkGoodsDetail.Data.WlPrice}元\n【巻后价】{hjkGoodsDetail.Data.WlPriceAfter} 元\n\n\ue231 <a href='{model.Data}'>点击这里领券下单</a>\n\n";
                    }
                    else
                    {
                        return ConfigurationManager.AppSettings["jd_nocoupon_msg"].Replace("\\n", "\n").Replace("\\ue231", "\ue231");
                    }

                }
                else
                {
                    return ConfigurationManager.AppSettings["jd_nocoupon_msg"].Replace("\\n", "\n").Replace("\\ue231", "\ue231");
                }
            }
            catch (Exception ex)
            {
                //LogHelper.WriteLog(typeof(WechatController), "获取京东skuid失败" + ex.Message);
                return "";
            }
        }

        #region 获取淘口令
        private static string GetTaobaoKePassword(string url, string log_url)
            {
                try
                {
                    ITopClient client = new DefaultTopClient(taobaoApiUrl, taobaoAppkey, taobaoSecret);
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
                    //LogHelper.WriteLog(typeof(config), "生成淘宝口令失败" + ex.Message);
                    //LogHelper.WriteLog(typeof(config), "生成淘宝口令失败,url为" + url);
                    return "";
                }

            }
        #endregion

    }
}
