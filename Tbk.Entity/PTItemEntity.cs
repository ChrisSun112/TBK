using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tbk.Entity
{
    //拼团商品
    public class PTItemEntity
    {
        public string item_id { get; set; }

        public string title { get; set; }

        public float orig_price { get; set; }

        public float pref_price { get; set; }

        public int pingduan_num {get;set;}

        public string pict_url { get; set; }

        public string start_datetime { get; set; }

        public string end_datetime { get; set; }

        public int stock { get; set; }

        public int volume { get; set; }

        public int surp_num { get; set; }

        public string long_url { get; set; }

        public string short_url { get; set; }

        public float comm_rate { get; set; }

        public float comm_je { get; set; }

        public int category_id { get; set; }

    }
}
