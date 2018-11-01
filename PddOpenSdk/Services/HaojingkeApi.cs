using PddOpenSdk.Models.Response.Haojingke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            return null;
        }
    }
}
