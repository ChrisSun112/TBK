using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tbk.Entity
{
    public class DTKGoodsDetailResponse
    {
        [JsonProperty("api_type")]
        public string Api_Type { get; set; }

        [JsonProperty("update_time")]
        public string UpdateTime { get; set; }

        [JsonProperty("total_num")]
        public string TotalNum { get; set; }

        [JsonProperty("api_content")]
        public string ApiContent { get; set; }

        [JsonProperty("result")]
        public IList<DTKGoodsModel> Result { get; set; }
    }
    public class DTKGoodsModel
    {
        public string ID { get; set; }

        public string GoodsID { get; set; }

        public string Title { get; set; }

        public string D_title { get; set; }

        public string Pic { get; set; }

        public string Cid { get; set; }

        public string Org_Price { get; set; }

        public string Price { get; set; }

        public string IsTmall { get; set; }

        public string Sales_num { get; set; }

        public string Dsr { get; set; }

        public string SellerID { get; set; }

        public string Commission_jihua { get; set; }

        public string Commission_queqiao { get; set; }
        public string Jihua_link { get; set; }
        public string Jihua_shenhe { get; set; }
        public string Introduce { get; set; }
        public string Quan_id { get; set; }
        public string Quan_price { get; set; }
        public string Quan_time { get; set; }
        public string Quan_surplus { get; set; }
        public string Quan_receive { get; set; }
        public string Quan_condition { get; set; }
        public string Quan_link { get; set; }
        public string Quan_m_link { get; set; }
    }
}
