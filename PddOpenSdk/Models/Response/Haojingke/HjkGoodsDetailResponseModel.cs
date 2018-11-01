using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PddOpenSdk.Models.Response.Haojingke
{
    public class HjkGoodsDetailResponseModel
    {
     
        [JsonProperty("data")]
        public HjkGoodsDetail Data { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
    }

    public class HjkGoodsDetail
    {
        [JsonProperty("skuid")]
        public string SkuId { get; set; }

        [JsonProperty("skuName")]
        public string SkuName { get; set; }

        [JsonProperty("skuDesc")]
        public string SkuDesc { get; set; }

        [JsonProperty("materiaUrl")]
        public string MateriaUrl { get; set; }

        [JsonProperty("picUrl")]
        public string PicUrl { get; set; }

        [JsonProperty("wlPrice")]
        public string WlPrice { get; set; }

        [JsonProperty("wlPrice_after")]
        public string WlPriceAfter { get; set; }

        [JsonProperty("couponList")]
        public string CouponList { get; set; }

        [JsonProperty("wlCommissionShare")]
        public string WlCommissionShare{ set;get; }

        [JsonProperty("wlCommission")]
        public string WlCommission { get; set; }


        [JsonProperty("discount")]
        public string Discount { get; set; }



    }

}
