using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tbk.Entity;

namespace Tbk.DAL
{
    public class CashOutDao
    {
        private Tbk.Common.MySqlHelper mySqlHelper = new Tbk.Common.MySqlHelper();

        public void Add(CashOutEntity cashOut)
        {
            string sqlStr = "insert into tb_cash_out(openid,cash_out_total,status,createdatetime) values(@openid,@cash_out_total,@status,@createdatetime)";
            MySqlParameter[] parameters =
            {
                new MySqlParameter("openid",cashOut.Openid),
                new MySqlParameter("cash_out_total",cashOut.Cash_out_total),
                new MySqlParameter("status",cashOut.Status),
                new MySqlParameter("createdatetime",cashOut.CreateDatetime)
            };

            mySqlHelper.ExecuteSql(sqlStr, parameters);
        }

        /// <summary>
        /// 提现
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="cashoutTotal"></param>
        public string CashOut(string openid,decimal cashoutTotal)
        {
            MySqlParameter parameter = new MySqlParameter("openid", openid);
            //1 查询可提现余额是否大于申请提现金额,为否 直接返回
            var result = mySqlHelper.GetSingle("select remaining_sum from tb_weixin_info where openid=@openid", parameter);
            decimal remaining_sum = result==null?0:(decimal)result;

            if (remaining_sum < cashoutTotal) return "提现金额不足";

            //2 提现金额充足，开始提现。先减去微信用户表中可提现金额。
            mySqlHelper.ExecuteSql("update tb_weixin_info set remaining_sum=" + (remaining_sum - cashoutTotal) + " where openid=@openid", parameter);

            //3 向提现明细表里新增提现申请
            Add(new CashOutEntity() { Openid = openid, Cash_out_total = cashoutTotal, Status = "申请中", CreateDatetime = DateTime.Now });

            return "成功";
        }

        /// <summary>
        /// 查询最近6个月内指定openid的提现明细
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public IList<CashOutEntity> GetCashOut(string openid)
        {
            string sqlStr = "select id,openid,cash_out_total,status,createdatetime from tb_cash_out where openid=@openid and createdatetime>'"+DateTime.Now.AddMonths(-6).ToString()+"' order by createdatetime desc";

            MySqlParameter parameter = new MySqlParameter("openid", openid);

            return mySqlHelper.QueryAll<CashOutEntity>(sqlStr, parameter);

        }
    }
}
