using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tbk.Entity
{
    public class CashBackEntity
    {
        public int Id { get; set; }

        public decimal Cash_Back_Total { get; set; }

        public string Openid { get; set; }

        public string Channel { get; set; }

        public DateTime? CreateDateTime { get; set; }

        public string Mark { get; set; }
    }
}
