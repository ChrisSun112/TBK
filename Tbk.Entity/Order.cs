using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tbk.Entity
{
    public class Order
    {
        public int Id { get; set; }

        public string OrderNo { get; set; }

        public string OpenId { get; set; }

        public decimal Payment { get; set; }

        public decimal CashOutTotal { get; set; }

        public string Status { get; set; }

        public string Channel { get; set; }

        public DateTime? CreateDateTime { get; set; }
    }
}
