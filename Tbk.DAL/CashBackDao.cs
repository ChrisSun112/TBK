using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tbk.Entity;

namespace Tbk.DAL
{
    public class CashBackDao
    {
        private Tbk.Common.MySqlHelper mySqlHelper = new Tbk.Common.MySqlHelper();

        public IList<CashBackEntity> GetCashBack(string openid)
        {
            string sqlStr = $"select id,cash_back_total,openid,channel,createdatetime,mark from tb_cash_back where openid=@openid and createdatetime>'{DateTime.Now.AddMonths(-6).ToString()}' order by createdatetime desc";
            MySqlParameter parameter = new MySqlParameter("openid", openid);

            return mySqlHelper.QueryAll<CashBackEntity>(sqlStr,parameter);
        }
    }
}
