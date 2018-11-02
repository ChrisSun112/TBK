using PddOpenSdk.Models.Response.Haojingke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tbk.Common;
using Newtonsoft.Json;

namespace PddOpenSdk.Services
{
    public class HaojingkeApi
    {
        private string appKey;
        private string positionId;

        private string apiUrl;

        public HaojingkeApi(string appKey,string positionId,string apiUrl)
        {
            this.appKey = appKey;
            this.positionId = positionId;
            this.apiUrl = apiUrl;
        }

        public HjkGoodsDetailResponseModel GetJDGoodsDetail(string skuId)
        {
            if (string.IsNullOrEmpty(appKey))
            {
                throw new Exception("appKey不能为空");
            }

            string url = apiUrl + $"?type=goodsdetail&apikey={appKey}&skuid={skuId}";

            string jsonContent = HttpUtility.SendPostHttpRequest(url, "application/json", "");

            HjkGoodsDetailResponseModel model = JsonConvert.DeserializeObject<HjkGoodsDetailResponseModel>(jsonContent);

            return model;
        }

        public HjkUnionUrlResponseModel GetUnionUrl(string skuId,string couponUrl)
        {
            if (string.IsNullOrEmpty(appKey)||string.IsNullOrEmpty(positionId))
            {
                throw new Exception("appKey和推广位ID不能为空");
            }
            string url = apiUrl + $"?type=unionurl&apikey={appKey}&materialIds={skuId}&positionId={positionId}&couponUrl={System.Web.HttpUtility.UrlEncode(couponUrl)}";

            string jsonContent = HttpUtility.SendPostHttpRequest(url, "application/json", "");

            HjkUnionUrlResponseModel model = JsonConvert.DeserializeObject<HjkUnionUrlResponseModel>(jsonContent);

            return model;
        }
    }
}
