using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Tbk.Entity;

namespace Tbk.DAL
{
    public class UnbindOrderDao
    {
        private Tbk.Common.MySqlHelper mySqlHelper = new Tbk.Common.MySqlHelper();

        public bool BindOrder(string orderNo,string openId)
        {
            string sqlStr = "select id,orderno,channel,payment,cashbacktotal,ordercreatedatetime,orderstatus from tb_unbind_order where orderno=@orderNo";

            MySqlParameter mySqlParameter = new MySqlParameter("orderNo", orderNo);

            var unbindOrder = mySqlHelper.Query<UnbindOrder>(sqlStr, mySqlParameter);

            if (unbindOrder == null)
            {
                return false;
            }

            List<string> sqlList = new List<string>();
            sqlList.Add($"insert into tb_order(orderno,openid,payment,cashouttotal,`status`,channel,createdatetime) values('{unbindOrder.OrderNo}','{openId}',{unbindOrder.Payment},{unbindOrder.CashBackTotal},'{unbindOrder.OrderStatus}','{unbindOrder.Channel}','{unbindOrder.OrderCreateDateTime}')");

            sqlList.Add($"delete from tb_unbind_order where orderno='{unbindOrder.OrderNo}'");

            mySqlHelper.ExecuteSqlTran(sqlList);

            return true;
            
        }
    }
}
