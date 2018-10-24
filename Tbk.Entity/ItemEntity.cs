
using System;

namespace Tbk.Entity
{
    public class ItemEntity
    {
        public int id { get; set; }
        public string shop_title { get; set; }
        public float zk_final_price { get; set; }
	    public string title { get; set; }
	    public int    volume { get; set; }
        public string  pict_url { get; set; }
	    public float   commission_rate { get; set; }
	    public float   coupon_info { get; set; }
	    public int category_id { get; set; }
	    public Int64 item_id { get; set; }
	    public DateTime coupon_start_time { get; set; }
        public DateTime coupon_end_time { get; set; }
	    public string  coupon_click_url { get; set; }
	    public string item_description { get; set; }
        public string shop_type { get; set; }
    }
}
