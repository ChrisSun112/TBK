using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PddOpenSdk.Models.Response.Haojingke
{
    /// <summary>
    /// 好京客转链api Response model
    /// </summary>
    public class HjkUnionUrlResponseModel
    {
        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
    }
}
