/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc
    
    文件名：CustomMessageHandler_Events.cs
    文件功能描述：自定义MessageHandler
    
    
    创建标识：Senparc - 20150312
----------------------------------------------------------------*/

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Senparc.Weixin.Exceptions;
using Senparc.CO2NET.Extensions;

using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.Entities;

using Senparc.NeuChar.Entities;
using Senparc.NeuChar.Agents;
using System.Web;
using System.Configuration;

namespace Softcomm.Weixin.CommonService.CustomMessageHandler
{
    /// <summary>
    /// 自定义MessageHandler
    /// </summary>
    public partial class CustomMessageHandler
    {
        private string GetWelcomeInfo()
        {
            return $"/:rose 亲，您好，我是网购省钱助手。\n您今后在淘宝天猫、京东、拼多多 想购买的商品都可发给我，查询优惠券！\n\n \ue231 <a href='{ConfigurationManager.AppSettings["tb_find_coupon_sop"]}'>点击淘宝优惠券返利教程</a> \n\n \ue231 <a href='{ConfigurationManager.AppSettings["jd_find_coupon_sop"]}'>点击京东优惠券返利教程</a> \n\n \ue231 <a href='{ConfigurationManager.AppSettings["pdd_find_coupon_sop"]}'>点击拼多多优惠券返利教程</a> \n\n 可【置顶公众号】，方便以后购物哦。";
        }

        /// <summary>
        /// 点击事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_ClickRequest(RequestMessageEvent_Click requestMessage)
        {
            IResponseMessageBase reponseMessage = null;
            //菜单点击，需要跟创建菜单时的Key匹配

            switch (requestMessage.EventKey)
            {
                case "zfbhb":
                    {
                        //这个过程实际已经在OnTextOrEventRequest中完成，这里不会执行到。
                        var strongResponseMessage = CreateResponseMessage<ResponseMessageText>();
                        reponseMessage = strongResponseMessage;
                        strongResponseMessage.Content = "打开支付宝首页搜索“552353665” 立即领红包";
                    }
                    break;
                case "usage":
                    {
                        var strongResponseMessage = CreateResponseMessage<ResponseMessageText>();
                        reponseMessage = strongResponseMessage;
                        strongResponseMessage.Content = $"\ue231 <a href='{ConfigurationManager.AppSettings["tb_find_coupon_sop"]}'>点击淘宝优惠券返利教程</a> \n\n\ue231 <a href='{ConfigurationManager.AppSettings["jd_find_coupon_sop"]}'>点击京东优惠券返利教程</a> \n\n\ue231 <a href='{ConfigurationManager.AppSettings["pdd_find_coupon_sop"]}'>点击拼多多优惠券返利教程</a> \n";
                    }
                    break;
                              
              
                default:
                    {
                        var strongResponseMessage = CreateResponseMessage<ResponseMessageText>();

                        reponseMessage = null;
                    }
                    break;
            }

            return reponseMessage;
        }


        /// <summary>
        /// 订阅（关注）事件
        /// </summary>
        /// <returns></returns>
        public override IResponseMessageBase OnEvent_SubscribeRequest(RequestMessageEvent_Subscribe requestMessage)
        {
            var responseMessage = ResponseMessageBase.CreateFromRequestMessage<ResponseMessageText>(requestMessage);
            responseMessage.Content = GetWelcomeInfo();
            if (!string.IsNullOrEmpty(requestMessage.EventKey))
            {
                responseMessage= null;
            }

            return responseMessage;
        }

    }
}