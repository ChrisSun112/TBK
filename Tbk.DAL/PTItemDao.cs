using Tbk.Entity;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tbk.DAL
{
    public class PTItemDao
    {
        private Common.MySqlHelper mySqlHelper = new Common.MySqlHelper();

      
        public IList<PTItemEntity> GetAllItems(int pageNo,int pageSize)
        {
            string sqlStr = "select item_id,title,orig_price,pref_price,pingduan_num,pict_url,volume,short_url,comm_je,category_id from tb_pingduan where comm_je>0.5 and volume>20 order by comm_je desc, volume desc  limit " + (pageNo-1)* pageSize + ","+ pageSize + ";";
            return mySqlHelper.QueryAll<PTItemEntity>(sqlStr, null);
        }

        public IList<PTItemEntity> GetItemsByCategory(int category_id,int pageNo,int pageSize)
        {
            string sqlStr = "select item_id,title,orig_price,pref_price,pingduan_num,pict_url,volume,short_url,comm_je,category_id from tb_pingduan where comm_je>0.5 and volume>20 and category_id=" + category_id+ " order  by comm_je desc, volume desc  limit " + (pageNo - 1) * pageSize + "," + pageSize + ";";
            return mySqlHelper.QueryAll<PTItemEntity>(sqlStr, null);
        }

        public PTItemEntity GetItem(long id)
        {
            string sqlStr = "select item_id,title,orig_price,pref_price,pingduan_num,pict_url,volume,short_url,comm_je,category_id from tb_pingduan where item_id='" + id+"'";
            return mySqlHelper.Query<PTItemEntity>(sqlStr, null);
        }



    }
}
