using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tbk.Common;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Tbk.mweb.Tests
{
    [TestClass]
    public class WeixinApiTest
    {
        
        [TestMethod]
        public void SendTemplateMsg()
        {
          
            //var data = new {
            //    touser= "o8IJf5hcH-rrubcZQUcN56xPizig",
            //    template_id= "qjoS1AGFKs9FuFRJI2oh5W1HMz9J5Og1XT9fTQmtZQU",
            //    data = new{
            //        first = new{
            //            value= "徐航，订单绑定成功。"
            //        },
            //        OrderSn = new
            //        {
            //            value= "242226243293916259"
            //        },
            //        OrderStatus = new
            //        {
            //            value="已付款"
            //        },
            //        remark = new
            //        {
            //            value= "可在“我的订单”中查看追踪订单信息。订单付款28.00元，预计可返现0.98元。"
            //        }

            //    }
            //};

            var data = new
            {
                touser = "o8IJf5hcH-rrubcZQUcN56xPizig",
                template_id = "WdfEogtJVP1p1uD2MyvQawv1bWwJZVg_F5gJvqo-8sg",
                url = "http://m.yshizi.cn/me/wallet?openid=o8IJf5hcH-rrubcZQUcN56xPizig",
                data = new
                {
                    first = new
                    {
                        value = "徐航，您的返现已到账。"
                    },
                    order = new
                    {
                        value = "242226243293916259"
                    },
                    money = new
                    {
                        value = "8.25元"
                    },
                    remark = new
                    {
                        value = "点击进入“我的钱包”查看。"
                    }

                }
            };

            var result = WeixinHelper.SendTemplateMsg(AccessTokenHelper.GetAccessToken(), JsonConvert.SerializeObject(data));

            Assert.IsNull(result);
        }
    }
}
