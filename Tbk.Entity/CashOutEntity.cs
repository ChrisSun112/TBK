using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tbk.Entity
{
    /// <summary>
    /// 提现实体类
    /// </summary>
    public class CashOutEntity
    {
        public int Id { get; set; }

        public string Openid { get; set; }

        public decimal Cash_out_total { get; set; }

        public string Status { get; set; }

        public DateTime? CreateDatetime { get; set; }

        public DateTime? DealedDateTime { get; set; }


    }
}
