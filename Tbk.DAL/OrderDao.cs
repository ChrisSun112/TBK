using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tbk.Entity;

namespace Tbk.DAL
{
    public class OrderDao
    {
        private Tbk.Common.MySqlHelper mySqlHelper = new Tbk.Common.MySqlHelper();

        public IList<Order> GetOrders(string openid,string channel)
        {
            string sqlStr = "select id,orderNo,openid,payment,cashouttotal,status,channel,createdatetime from tb_order where openid=@openid and channel=@channel order by createdatetime desc";

            MySqlParameter[] parameters = {
                new MySqlParameter("openid",openid),
                new MySqlParameter("channel",channel)
            };
            return mySqlHelper.QueryAll<Order>(sqlStr,parameters);
        }
    }
}
