using PddOpenSdk.Services;
using System;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using Tbk.Common;

namespace Softcomm.Weixin.CommService.Service
{
    public class TaobaoCouponService : ICouponService
    {

        private TaobaoCommonApi taobaoCommonApi = new TaobaoCommonApi(ConfigurationManager.AppSettings["TaobaoAppKey"], ConfigurationManager.AppSettings["TaobaoSecret"], long.Parse(ConfigurationManager.AppSettings["AddzoneId"]));

        //分佣比例
        private decimal commission_rate = decimal.Parse(ConfigurationManager.AppSettings["commission_rate"]);

        /// <summary>
        /// 通过手淘分享内容中的链接获取商品关键字，查询淘宝商品优惠信息
        /// </summary>
        /// <param name="itemInfo"></param>
        /// <returns></returns>
        public string GetTaobaoCouponByLink(string itemInfo)
        {

            string tbk_nocoupon_msg = ConfigurationManager.AppSettings["tbk_nocoupon_msg"].Replace("\\n", "\n").Replace("\\ue231", "\ue231");

            Match m_url = Regex.Match(itemInfo, @"htt(p|ps):\/\/([\w\-]+(\.[\w\-]+)*\/)*[\w\-]+(\.[\w\-]+)*\/?(\?([\w\-\.,@?^=%&:\/~\+#]*)+)?");

            if (m_url.Value == "")
            {
                return tbk_nocoupon_msg;
            }
            var s = HttpUtility.HttpGet(m_url.Value, "", "utf-8");

           
            //获取宝贝item id
            Match m_item = Regex.Match(s, @"((?<=m.taobao.com\/i)([0-9]+))|((?<=&id=)([0-9]+))");
            string item_id = m_item.Value;

            Match am_url = Regex.Match(s, @"(?<=var url = ')(.*)(?=')");
            var htmlContent = HttpUtility.HttpGet(am_url.Value, "", "gbk");
            Match keyMatch = Regex.Match(htmlContent, "(?<=title\\>).*(?=</title)");


            if (string.IsNullOrEmpty(item_id))
            {
                Match re_m_item = Regex.Match(htmlContent, @"(?<=taobao.com/item.htm\?id=)([0-9]*)");
                item_id = re_m_item.Value;

            }

            if (string.IsNullOrEmpty(keyMatch.Value))
            {
                return tbk_nocoupon_msg;
            }

            var mapDataResponse = taobaoCommonApi.MaterialOptional(keyMatch.Value.Split('-')[0]);

          
            string responeMessage = "";

            float numid = float.Parse(item_id);

            if (mapDataResponse!=null&&mapDataResponse.Count > 0)
            {

                foreach (var g in mapDataResponse)
                {
                    if (g.NumIid == numid)
                    {
                        if (string.IsNullOrEmpty(g.CouponInfo))
                        {
                            var hongbao = decimal.Parse(g.ZkFinalPrice) * decimal.Parse(g.CommissionRate) / 10000 * commission_rate;

                            responeMessage = $"{g.Title}\n【在售价】{g.ZkFinalPrice}元\n【约返利】{Math.Round(hongbao, 2)}元\n复制这条信息，打开「手机绹宝」领巻下单{taobaoCommonApi.GetTaobaoKePassword(g.Url, g.PictUrl + "_400x400.jpg")}\n==========================\n下单确认收货后就能收到返利佣金啦~\n 点击查看  <a href='http://mp.weixin.qq.com/s?__biz=Mzg2NTAxOTEyMA==&mid=100000146&idx=1&sn=62405c8df3db46e74940aefb9ac3737b&chksm=4e61340d7916bd1bf645afbc6d10c1f19561d7fa59847516c01e64c0791e6d544f4f56c4f498#rd'>如何领取返利</a>";
                            return responeMessage;
                        }
                        else
                        {
                            var hongbao = (decimal.Parse(g.ZkFinalPrice) - decimal.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value)) * decimal.Parse(g.CommissionRate) / 10000 * commission_rate;
                            responeMessage = $"{g.Title}\n【在售价】{g.ZkFinalPrice}元\n【巻后价】{Math.Round(double.Parse(g.ZkFinalPrice) - double.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value), 2)} 元\n复制这条信息，打开「手机绹宝」领巻下单{taobaoCommonApi.GetTaobaoKePassword(g.CouponShareUrl, g.PictUrl + "_400x400.jpg")}\n";
                            return responeMessage;
                        }
                    }
                }

                //没有找到，有相似宝贝推荐
                var w = mapDataResponse.Where(y => !string.IsNullOrEmpty(y.CouponId)).OrderByDescending(y => y.Volume).FirstOrDefault();

                if (w == null)
                {
                    responeMessage = ConfigurationManager.AppSettings["tbk_nocoupon_msg"].Replace("\\n", "\n").Replace("\\ue231", "\ue231");
                }
                else
                {
                    var hongbao = (decimal.Parse(w.ZkFinalPrice) - decimal.Parse(Regex.Match(w.CouponInfo, "减" + @"(\d+)").Groups[1].Value)) * decimal.Parse(w.CommissionRate) / 10000 * commission_rate;

                    responeMessage = $"/:rose 亲，这款商品的优惠返利活动结束了~\n已为你推荐以下宝贝。\n==========================\n{w.Title}\n【在售价】{w.ZkFinalPrice}元\n【巻后价】{Math.Round(double.Parse(w.ZkFinalPrice) - double.Parse(Regex.Match(w.CouponInfo, "减" + @"(\d+)").Groups[1].Value), 2)} 元\n复制这条信息，打开「手机绹宝」领巻下单{taobaoCommonApi.GetTaobaoKePassword(w.CouponShareUrl, w.PictUrl + "_400x400.jpg")}\n";
                }

                return responeMessage;

            }
            else
            {
                return ConfigurationManager.AppSettings["tbk_nocoupon_msg"].Replace("\\n", "\n").Replace("\\ue231", "\ue231");

            }
        }

        /// <summary>
        /// 通过抓取手淘分享内容“【】”中关键字获取优惠劵信息
        /// 这种方式不能避免有些商品分享内容中【】非商品关键字，所有有了GetTaobaoCouponByLink方法
        /// </summary>
        /// <param name="xmlMsg"></param>
        /// <returns></returns>
        public string QueryCoupon(string itemInfo)
        {
            

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

                //解决手淘分享【】内非商品关键字bug
                if (title.Contains(",") | title.Contains("，"))
                {
                    return GetTaobaoCouponByLink(itemInfo);
                }

                var resultList = taobaoCommonApi.MaterialOptional(title);


                if (resultList!=null&&resultList.Count > 0)
                {
                    //获取淘宝短链接
                    Match m_url = Regex.Match(itemInfo, @"htt(p|ps):\/\/([\w\-]+(\.[\w\-]+)*\/)*[\w\-]+(\.[\w\-]+)*\/?(\?([\w\-\.,@?^=%&:\/~\+#]*)+)?");

                    if (m_url.Value == "")
                    {
                        return responeMessage;
                    }
                    var s = HttpUtility.HttpGet(m_url.Value, "", "utf-8");

                    //获取宝贝item id
                    Match m_item = Regex.Match(s, @"((?<=m.taobao.com\/i)([0-9]+))|((?<=&id=)([0-9]+))");
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
                       
                        var g = resultList.Where(y => !string.IsNullOrEmpty(y.CouponId)).OrderByDescending(w => w.Volume).FirstOrDefault();
                        if (g == null)
                        {
                            responeMessage = ConfigurationManager.AppSettings["tbk_nocoupon_msg"].Replace("\\n", "\n").Replace("\\ue231", "\ue231");

                        }
                        else
                        {
                            var hongbao = (decimal.Parse(g.ZkFinalPrice) - decimal.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value)) * decimal.Parse(g.CommissionRate) / 10000 * commission_rate;
                          
                            responeMessage = $"{g.Title}\n【在售价】{g.ZkFinalPrice}元\n【巻后价】{Math.Round(double.Parse(g.ZkFinalPrice) - double.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value), 2)} 元\n复制这条信息，打开「手机绹宝」领巻下单{taobaoCommonApi.GetTaobaoKePassword(g.CouponShareUrl, g.PictUrl + "_400x400.jpg")}\n";

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
                          
                            var g = resultList.Where(y => !string.IsNullOrEmpty(y.CouponId)).OrderByDescending(y => y.Volume).FirstOrDefault();

                            var hongbao = (decimal.Parse(g.ZkFinalPrice) - decimal.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value)) * decimal.Parse(g.CommissionRate) / 10000 * commission_rate;
                            responeMessage = $"{g.Title}\n【在售价】{g.ZkFinalPrice}元\n【巻后价】{Math.Round(double.Parse(g.ZkFinalPrice) - double.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value), 2)} 元\n复制这条信息，打开「手机绹宝」领巻下单{taobaoCommonApi.GetTaobaoKePassword(g.CouponShareUrl, g.PictUrl + "_400x400.jpg")}\n";

                            return responeMessage;

                        }

                        foreach (var g in resultList)
                        {
                            if (g.NumIid == numid)
                            {
                                if (string.IsNullOrEmpty(g.CouponInfo))
                                {
                                    var hongbao = decimal.Parse(g.ZkFinalPrice) * decimal.Parse(g.CommissionRate) / 10000 * commission_rate;

                                    responeMessage = $"{g.Title}\n【在售价】{g.ZkFinalPrice}元\n【约返利】{Math.Round(hongbao, 2)}元\n复制这条信息，打开「手机绹宝」领巻下单{taobaoCommonApi.GetTaobaoKePassword(g.Url, g.PictUrl + "_400x400.jpg")}\n==========================\n下单确认收货后就能收到返利佣金啦~\n 点击查看  <a href='http://mp.weixin.qq.com/s?__biz=Mzg2NTAxOTEyMA==&mid=100000146&idx=1&sn=62405c8df3db46e74940aefb9ac3737b&chksm=4e61340d7916bd1bf645afbc6d10c1f19561d7fa59847516c01e64c0791e6d544f4f56c4f498#rd'>如何领取返利</a>";
                                    return responeMessage;
                                }
                                else
                                {
                                    var hongbao = (decimal.Parse(g.ZkFinalPrice) - decimal.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value)) * decimal.Parse(g.CommissionRate) / 10000 * commission_rate;
                                    responeMessage = $"{g.Title}\n【在售价】{g.ZkFinalPrice}元\n【巻后价】{Math.Round(double.Parse(g.ZkFinalPrice) - double.Parse(Regex.Match(g.CouponInfo, "减" + @"(\d+)").Groups[1].Value), 2)} 元\n复制这条信息，打开「手机绹宝」领巻下单{taobaoCommonApi.GetTaobaoKePassword(g.CouponShareUrl, g.PictUrl + "_400x400.jpg")}\n";
                                    return responeMessage;
                                }
                            }
                        }

                        //没有找到，有相似宝贝推荐
                        var w = resultList.Where(y => !string.IsNullOrEmpty(y.CouponId)).OrderByDescending(y => y.Volume).FirstOrDefault();

                        if (w == null)
                        {
                            responeMessage = ConfigurationManager.AppSettings["tbk_nocoupon_msg"].Replace("\\n", "\n").Replace("\\ue231", "\ue231");
                        }
                        else
                        {
                            var hongbao = (decimal.Parse(w.ZkFinalPrice) - decimal.Parse(Regex.Match(w.CouponInfo, "减" + @"(\d+)").Groups[1].Value)) * decimal.Parse(w.CommissionRate) / 10000 * commission_rate;

                            responeMessage = $"/:rose 亲，这款商品的优惠返利活动结束了~\n已为你推荐以下宝贝。\n==========================\n{w.Title}\n【在售价】{w.ZkFinalPrice}元\n【巻后价】{Math.Round(double.Parse(w.ZkFinalPrice) - double.Parse(Regex.Match(w.CouponInfo, "减" + @"(\d+)").Groups[1].Value), 2)} 元\n复制这条信息，打开「手机绹宝」领巻下单{taobaoCommonApi.GetTaobaoKePassword(w.CouponShareUrl, w.PictUrl + "_400x400.jpg")}\n";
                        }

                        return responeMessage;
                    }

                }
                else
                {
                    responeMessage = GetTaobaoCouponByLink(itemInfo);

                }

            }
            catch (Exception ex)
            {
               Senparc.Weixin.WeixinTrace.SendCustomLog("查询淘宝优惠券异常",ex.Message);

            }
            return responeMessage;
        }

       
    }
}
