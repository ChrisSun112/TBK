/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：WeixinController.cs
    文件功能描述：用于处理微信回调的信息


    创建标识：Senparc - 20150312
----------------------------------------------------------------*/

using System;
using System.IO;
using System.Web.Mvc;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin;
using Senparc.Weixin.MP;
using Softcomm.Weixin.CommonService.CustomMessageHandler;
using Senparc.Weixin.MP.MvcExtension;
using System.Threading.Tasks;

namespace Softcomm.Weixin.Web.Controllers
{
    public partial class WeixinAsyncController : Controller
    {
        public static readonly string Token = Config.SenparcWeixinSetting.Token;//与微信公众账号后台的Token设置保持一致，区分大小写。
        public static readonly string EncodingAESKey = Config.SenparcWeixinSetting.EncodingAESKey;//与微信公众账号后台的EncodingAESKey设置保持一致，区分大小写。
        public static readonly string AppId = Config.SenparcWeixinSetting.WeixinAppId;//与微信公众账号后台的AppId设置保持一致，区分大小写。

        readonly Func<string> _getRandomFileName = () => DateTime.Now.ToString("yyyyMMdd-HHmmss") + Guid.NewGuid().ToString("n").Substring(0, 6);

        public WeixinAsyncController()
        {

        }

        [HttpGet]
        [ActionName("Index")]
        public Task<ActionResult> Get(string signature, string timestamp, string nonce, string echostr)
        {
            return Task.Factory.StartNew(() =>
            {
                if (CheckSignature.Check(signature, timestamp, nonce, Token))
                {
                    return echostr; //返回随机字符串则表示验证通过
                }
                else
                {
                    return "failed:" + signature + "," + CheckSignature.GetSignature(timestamp, nonce, Token) + "。" +
                        "如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。";
                }
            }).ContinueWith<ActionResult>(task => Content(task.Result));
        }


        /// <summary>
        /// 最简化的处理流程
        /// </summary>
        [HttpPost]
        [ActionName("Index")]
        public async Task<ActionResult> Post(PostModel postModel)
        {
            //if (!CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, Token))
            //{
            //    return new WeixinResult("参数错误！");
            //}

            postModel.Token = Token;
            postModel.EncodingAESKey = EncodingAESKey; //根据自己后台的设置保持一致
            postModel.AppId = AppId; //根据自己后台的设置保持一致

            var messageHandler = new CustomMessageHandler(Request.InputStream, postModel, 10);

            messageHandler.DefaultMessageHandlerAsyncEvent = Senparc.NeuChar.MessageHandlers.DefaultMessageHandlerAsyncEvent.SelfSynicMethod;//没有重写的异步方法将默认尝试调用同步方法中的代码（为了偷懒）

            await messageHandler.ExecuteAsync(); //执行微信处理过程

            
           return  new FixWeixinBugWeixinResult(messageHandler);
         
        }

    }
}
