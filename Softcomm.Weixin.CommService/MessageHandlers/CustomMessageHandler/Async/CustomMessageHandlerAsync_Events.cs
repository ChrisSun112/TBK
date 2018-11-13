/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc
    
    文件名：CustomMessageHandler_Events.cs
    文件功能描述：自定义MessageHandler
    
    
    创建标识：Senparc - 20150312
----------------------------------------------------------------*/

using System.Threading.Tasks;
using Senparc.Weixin.MP.Entities;

using Senparc.NeuChar.Entities;
using Senparc.NeuChar.Entities.Request;
using Softcomm.Weixin.CommService;

namespace Softcomm.Weixin.CommonService.CustomMessageHandler
{
    /// <summary>
    /// 自定义MessageHandler
    /// </summary>
    public partial class CustomMessageHandler
    {
        

        /// <summary>
        /// 点击事件
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override Task<IResponseMessageBase> OnEvent_ClickRequestAsync(RequestMessageEvent_Click requestMessage)
        {

            return Task.Factory.StartNew<IResponseMessageBase>(() =>
            {
                var syncResponseMessage = OnEvent_ClickRequest(requestMessage);
                 
                return syncResponseMessage;
            });
        }

        /// <summary>
        /// 处理文字请求
        /// </summary>
        /// <returns></returns>
        public override async Task<IResponseMessageBase> OnTextRequestAsync(RequestMessageText requestMessage)
        {

            var defaultResponseMessage = base.CreateResponseMessage<ResponseMessageText>();

           //匹配拼多多链接
            if (requestMessage.Content.Contains("yangkeduo.com"))
            {
               
                defaultResponseMessage.Content = await CouponHelper.GetPDDCouponAsync(requestMessage);
                return defaultResponseMessage;
            }

            //说明：实际项目中这里的逻辑可以交给Service处理具体信息，参考OnLocationRequest方法或/Service/LocationSercice.cs
            return await Task.Factory.StartNew<IResponseMessageBase>(() =>
             {
                 var responseMessage = base.CreateResponseMessage<ResponseMessageText>();

                 var requestHandler =
                  requestMessage.StartHandler()
                     //匹配淘宝、拼多多订单
                     .Regex(@"^(\d{22})$|(\d{6}-\d{15})$", () =>
                     {
                         responseMessage.Content = "您的订单编号已收到，预计1个工作日内核实后返利将通过现金红包形式发放。";
                         return responseMessage;
                     }) 
                     //京东查券
                     .Keyword("jd.com", () => 
                     {
                        
                         responseMessage.Content = CouponHelper.GetJDCoupon(requestMessage);
                         return responseMessage;
                     })
                     //淘宝查券
                     .Regex(@"【.*】", () =>
                     {
                         
                         responseMessage.Content = CouponHelper.GetTaobaoCoupon(requestMessage);
                         return responseMessage;
                     })
                     //其他不回复
                     .Default(() =>
                     {
                         return null;
                     });
                 return requestHandler.GetResponseMessage() as IResponseMessageBase;
             });
        }
    }
}