using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tbk.Entity
{
    public class UnbindOrder
    {
        public int Id { get; set; }

        public string OrderNo { set; get; }

        public string Channel { get; set; }

        public decimal Payment { get; set; }

        public decimal CashBackTotal { get; set; }

        public DateTime? OrderCreateDateTime { set; get; }

        public string OrderStatus { get; set; }
    }
}
