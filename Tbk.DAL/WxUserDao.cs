using Tbk.Entity;
using MySql.Data.MySqlClient;


namespace Tbk.DAL
{
    public class WxUserDao
    {
        private Tbk.Common.MySqlHelper mySqlHelper = new Tbk.Common.MySqlHelper();

        public void Add(WxUser user)
        {
            string sqlStr = "insert into tb_weixin_info(openid,nickname,isSubscribe,createdatetime,headimgurl,ticket,ticketcreatedatetime,tj_openid) values(@openid,@nickname,@isSubscribe,@createdatetime,@headimgurl,@ticket,@ticketcreatedatetime,@tj_openid)";
            MySqlParameter[] mySqlParameters =
            {
                new MySqlParameter("openid",user.openid),
                new MySqlParameter("nickname",user.nickname),
                new MySqlParameter("isSubscribe",user.isSubscribe),
                new MySqlParameter("createdatetime",user.createdatetime),
                new MySqlParameter("headimgurl",user.headimgurl),
                new MySqlParameter("ticket",user.ticket),
                new MySqlParameter("ticketcreatedatetime",user.ticketcreatedatetime),
                new MySqlParameter("tj_openid",user.tj_openid)
            };

            mySqlHelper.ExecuteSql(sqlStr, mySqlParameters);
        }

        public WxUser Find(string openid)
        {
            string sqlStr = "select openid,nickname,ticket,isSubscribe,createdatetime,headimgurl,ticket,ticketcreatedatetime,issubscribe,remaining_sum,total,cash_out_sum from tb_weixin_info where openid=@openid";
            MySqlParameter[] mySqlParameters =
            {
                new MySqlParameter("openid",openid)

            };

            return mySqlHelper.Query<WxUser>(sqlStr, mySqlParameters);
        }

        public void Update(WxUser user)
        {
            string sqlStr = "update tb_weixin_info set nickname=@nickname,tj_openid=@tj_openid,isSubscribe=@isSubscribe,createdatetime=@createdatetime,headimgurl=@headimgurl,ticket=@ticket,ticketcreatedatetime=@ticketcreatedatetime where openid=@openid ";

            MySqlParameter[] mySqlParameters =
            {
                new MySqlParameter("openid",user.openid),
                new MySqlParameter("nickname",user.nickname),
                new MySqlParameter("tj_openid",user.tj_openid),
                new MySqlParameter("isSubscribe",user.isSubscribe),
                new MySqlParameter("createdatetime",user.createdatetime),
                new MySqlParameter("headimgurl",user.headimgurl),
                new MySqlParameter("ticket",user.ticket),
                new MySqlParameter("ticketcreatedatetime",user.ticketcreatedatetime)
            };

            mySqlHelper.ExecuteSql(sqlStr, mySqlParameters);
        }

        public int FindTjCount(string openid)
        {
            string sqlStr = "select count(id) from tb_weixin_info where tj_openid='"+openid+"'";

            return mySqlHelper.QueryInt(sqlStr, null);
        }
    }
}
