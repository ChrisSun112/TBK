using Microsoft.VisualStudio.TestTools.UnitTesting;
using PddOpenSdk.Models.Request.Ddk;
using PddOpenSdk.Services;
using PddOpenSdk.Services.PddApi;

namespace Tbk.mweb.Tests
{
    [TestClass]
    public class PddApiTest
    {
       

        [TestMethod]
        public async void TestMethod1Async()
        {
            PddCommonApi.ClientId = "d8172a66ddf14220beac58e8eddca0d9";
            PddCommonApi.ClientSecret = "d5993ce16cc0d1bd35403176f35544962e786645";
            PddCommonApi.RedirectUri = "RedirectUri";
            PddCommonApi.AccessToken = "";
            DdkApi ddkApi = new DdkApi();
            var model = new DetailDdkGoodsRequestModel()
            {
                Type = "pdd.ddk.goods.detail",
                GoodsIdList = "[1268788439]"
            };
            var result = await ddkApi.DetailDdkGoodsAsync(model);

            var a = result;

            Assert.IsNotNull(a);

            //var result = ddkApi.SearchDdkGoodsAsync()
        }
    }
}
