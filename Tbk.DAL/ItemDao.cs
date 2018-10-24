using Tbk.Entity;
using System.Collections.Generic;


namespace Tbk.DAL
{
    public class ItemDao
    {
        private Tbk.Common.MySqlHelper mySqlHelper = new Tbk.Common.MySqlHelper();

      
        public IList<ItemEntity> GetAllItems(int pageNo)
        {
            string sqlStr = "select id,shop_title,zk_final_price,title,volume,pict_url,coupon_info,category_id,item_id,coupon_click_url,shop_type  from tb_item where volume>100 and commission_rate>1 order by id desc  limit " + (pageNo-1)*20+",20;";
            return mySqlHelper.QueryAll<ItemEntity>(sqlStr, null);
        }

        public IList<ItemEntity> GetItemsByCategory(int category_id,int pageNo)
        {
            string sqlStr = "select id,shop_title,zk_final_price,title,volume,pict_url,coupon_info,category_id,item_id,coupon_click_url,shop_type  from tb_item where volume>100 and commission_rate>1 and  category_id=" + category_id+" order by id desc  limit " + (pageNo - 1)*20 + ",20;";
            return mySqlHelper.QueryAll<ItemEntity>(sqlStr, null);
        }

        public ItemEntity GetItem(int id)
        {
            string sqlStr = "select shop_title,zk_final_price,title,volume,pict_url,coupon_info,category_id,item_id,coupon_click_url,shop_type from tb_item where id=" + id;
            return mySqlHelper.Query<ItemEntity>(sqlStr, null);
        }

        public ItemEntity GetItem_9k9(int id)
        {
            string sqlStr = "select shop_title,zk_final_price,title,volume,pict_url,coupon_info,category_id,item_id,coupon_click_url,shop_type from tb_9k9_item where id=" + id;
            return mySqlHelper.Query<ItemEntity>(sqlStr, null);
        }

        public ItemEntity GetItem_20(int id)
        {
            string sqlStr = "select shop_title,zk_final_price,title,volume,pict_url,coupon_info,category_id,item_id,coupon_click_url,shop_type from tb_20_item where id=" + id;
            return mySqlHelper.Query<ItemEntity>(sqlStr, null);
        }

        public IList<ItemEntity> GetAll9k9Items(int pageNo)
        {
            string sqlStr = "select id,shop_title,zk_final_price,title,volume,pict_url,coupon_info,category_id,item_id,coupon_click_url,shop_type  from tb_9k9_item where volume>100 and commission_rate>1 order by id desc  limit " + (pageNo - 1) * 20 + ",20;";
            return mySqlHelper.QueryAll<ItemEntity>(sqlStr, null);
        }



        public IList<ItemEntity> Get9k9ItemsByCategory(int category_id, int pageNo)
        {
            string sqlStr = "select id,shop_title,zk_final_price,title,volume,pict_url,coupon_info,category_id,item_id,coupon_click_url,shop_type  from tb_9k9_item where volume>100 and commission_rate>1 and  category_id=" + category_id + " order by id desc  limit " + (pageNo - 1) * 20 + ",20;";
            return mySqlHelper.QueryAll<ItemEntity>(sqlStr, null);
        }

        public IList<ItemEntity> GetAll20Items(int pageNo)
        {
            string sqlStr = "select id,shop_title,zk_final_price,title,volume,pict_url,coupon_info,category_id,item_id,coupon_click_url,shop_type  from tb_20_item where volume>100 and commission_rate>1 order by id desc  limit " + (pageNo - 1) * 20 + ",20;";
            return mySqlHelper.QueryAll<ItemEntity>(sqlStr, null);
        }



        public IList<ItemEntity> Get20ItemsByCategory(int category_id, int pageNo)
        {
            string sqlStr = "select id,shop_title,zk_final_price,title,volume,pict_url,coupon_info,category_id,item_id,coupon_click_url,shop_type  from tb_20_item where volume>100 and commission_rate>1 and  category_id=" + category_id + " order by id desc  limit " + (pageNo - 1) * 20 + ",20;";
            return mySqlHelper.QueryAll<ItemEntity>(sqlStr, null);
        }





    }
}
