using System;

namespace Tbk.Entity
{
    public class WxUser
    {
        public  uint id { get; set; }
        public string openid { get; set; }

        //微信昵称
        public string nickname { get; set; }

        //推荐者openid
        public string tj_openid { get; set; }

        //是否订阅，是或者否
        public string isSubscribe { get; set; }

        public string ticket { get; set; }

        public string headimgurl { get; set; }
        //创建时间
        public DateTime createdatetime { get; set; }

        public DateTime ticketcreatedatetime { get; set; }

        public int tj_count { get; set; }

        //可用余额
        public decimal remaining_sum { get; set; }

        //总收益
        public decimal total { get; set; }

        //已提现金额
        public decimal cash_out_sum { set; get; }
    }
}
