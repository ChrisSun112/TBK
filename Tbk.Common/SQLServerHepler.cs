//------------------------------------------------------------
// All Rights Reserved , Copyright (C) 2010 , lusens 
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace Tbk.Common
{
    /// <summary>
    /// SQLServer数据库操作类
    /// 本类主要包括SQLServer数据库的基本操作
    /// 
    /// 修改纪录
    /// 
    ///     2010.10.22 版本：1.1 lusens 新增了用户传入sql语句列表的事务操作
    ///		2010.09.05 版本：1.0 lusens 创建。
    /// 
    /// 版本：1.1
    /// 
    /// <author>
    ///		<name>lusens</name>
    ///		<date>2010.09.05</date>
    ///		<EMail>lusens@foxmail.com</EMail>
    /// </author> 
    /// </summary>
    public abstract class SQLServerHepler
    {
        //public static readonly string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SQLConnString"].ConnectionString;

        #region public static string GetConnectionStringByConfig()
        /// <summary>
        /// 读取配置文件的ConnectionString字符串
        /// </summary>
        /// <returns>Connection连接字符串</returns>
        public static string GetConnectionStringByConfig()
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings["SQLConnString"].ConnectionString;
        }
       
        #endregion

        #region 执行SQL，返回被操作的行数,public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)

        /// <summary>
        /// 执行SQL语句，返回被操作的行数
        /// 使用using语句进行conn对象的释放
        /// </summary>
        /// <param name="connectionString">连接字符</param>
        /// <param name="cmdType">Command类型，SQL语句或存储过程</param>
        /// <param name="cmdText">SQL语句或存储过程名称</param>
        /// <param name="commandParameters">参数数组</param>
        /// <returns>返回被操作行数</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                int val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return val;
            }
        }
        #endregion

        #region 执行SQL，返回被操作的行数,public static int ExecuteNonQuery(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        /// <summary>
        /// 执行SQL语句，返回被操作的行数
        /// 这里默认使用外面传入的conn对象，使用完成后不会对conn对象进行释放，需要自己在外面进行数据库连接释放
        /// </summary>
        /// <param name="connection">数据库连接对象</param>
        /// <param name="cmdType">command命令类型，SQL语句或存储过程</param>
        /// <param name="cmdText">SQL语句或存储过程名称</param>
        /// <param name="commandParameters">参数数组</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }
        #endregion

        #region 执行SQL语句，返回被操作的行数,public static int ExecuteNonQuery(SqlTransaction trans, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        /// <summary>
        /// 执行SQL语句，返回被操作的行数
        /// 这里默认使用外面传入的conn对象，使用完成后不会对conn对象进行释放，需要自己在外面进行数据库连接释放
        /// </summary>
        /// <param name="trans">SQL事务</param>
        /// <param name="cmdType">command类型，SQL语句或存储过程</param>
        /// <param name="cmdText">SQL语句或存储过程名称</param>
        /// <param name="commandParameters">参数数组</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(SqlTransaction trans, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        #endregion

        #region 执行SQL，返回SqlDataReader,public static SqlDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        /// <summary>
        /// 执行SQL，返回SqlDataReader
        /// 返回一个连接，所以不能进行conn释放，在外界代码中使用完DataReader后，注意需要释放reader对象
        /// 当返回连接对象报错时，这里进行数据库连接的关闭，保证数据库连接使用完成后保持关闭
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="cmdType">command命令类型，SQL语句或存储过程</param>
        /// <param name="cmdText">SQL语句或存储过程名称</param>
        /// <param name="commandParameters">参数数组</param>
        /// <returns></returns>
        public static SqlDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            SqlConnection conn = new SqlConnection(connectionString);

            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return rdr;
            }
            catch (Exception e)
            {
                conn.Close();
                conn.Dispose();
                throw e;
            }
        }
        #endregion

        #region 执行SQL，返回SqlDataReader, public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        /// <summary>
        /// 执行SQL，返回SqlDataReader
        /// 返回一个连接，所以不能进行conn释放，在外界代码中使用完DataReader后，注意需要释放reader对象
        /// 当返回连接对象报错时，这里进行数据库连接的关闭，保证数据库连接使用完成后保持关闭
        /// </summary>
        /// <param name="connection">SqlConnection数据库连接对象</param>
        /// <param name="cmdType">command命令类型，SQL语句或存储过程</param>
        /// <param name="cmdText">SQL语句或存储过程名称</param>
        /// <param name="commandParameters">参数数组</param>
        /// <returns></returns>
        public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            try
            {
                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return rdr;
            }
            catch (Exception e)
            {
                connection.Close();
                throw e;
            }
        }
        #endregion

        #region 执行SQL，返回DataTable， public static DataTable ExecuteDataTable(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        /// <summary>
        /// 执行SQL，返回DataTable
        /// 使用using语句进行conn对象的释放
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="cmdType">command命令类型，SQL语句或存储过程</param>
        /// <param name="cmdText">SQL语句或存储过程名称</param>
        /// <param name="commandParameters">参数数组</param>
        /// <returns></returns>
        public static DataTable ExecuteDataTable(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                using (SqlDataAdapter sda = new SqlDataAdapter())
                {
                    sda.SelectCommand = cmd;
                    sda.Fill(dt);
                }
            }

            return dt;
        }
        #endregion

        #region 执行SQL，返回DataTable,public static DataTable ExecuteDataTable(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        /// <summary>
        /// 执行SQL，返回DataTable
        /// 这里默认使用外面传入的conn对象，使用完成后不会对conn对象进行释放，需要自己在外面进行数据库连接释放
        /// </summary>
        /// <param name="connection">数据库连接对象</param>
        /// <param name="cmdType">Command命令类型，SQL语句或存储过程</param>
        /// <param name="cmdText">SQL语句或存储过程名称</param>
        /// <param name="commandParameters">参数数组</param>
        /// <returns></returns>
        public static DataTable ExecuteDataTable(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();

            PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
            using (SqlDataAdapter sda = new SqlDataAdapter())
            {
                sda.SelectCommand = cmd;
                sda.Fill(dt);
            }

            return dt;
        }
        #endregion

        #region 返回第一行第一列，public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        /// <summary>
        /// 返回第一行第一列
        /// 使用using语句进行conn对象的释放
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="cmdType">Command命令类型，SQL语句还是存储过程</param>
        /// <param name="cmdText">SQL语句或存储过程名称</param>
        /// <param name="commandParameters">参数数组</param>
        /// <returns></returns>
        public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
        }
        #endregion

        #region 返回第一行第一列，public static object ExecuteScalar(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        /// <summary>
        /// 返回第一行第一列
        /// 使用外面传入的conn对象，使用完成后不会对conn对象进行释放，需要自己在外面进行数据库连接释放
        /// </summary>
        /// <param name="connection">SqlConnection数据库连接对象</param>
        /// <param name="cmdType">Command命令类型，SQL语句还是存储过程</param>
        /// <param name="cmdText">SQL语句或存储过程名称</param>
        /// <param name="commandParameters">参数数组</param>
        /// <returns></returns>
        public static object ExecuteScalar(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }
        #endregion

        #region 以事务执行sql语句列表，public static bool ExecuteTransaction(SqlConnection conn, List<string> cmdTextes, List<SqlParameter[]> commandParameterses)
        /// <summary>
        /// 以事务执行sql语句列表，返回事务执行是否成功
        /// </summary>
        /// <param name="conn">数据库连接对象</param>
        /// <param name="cmdTextes">sql语句列表</param>
        /// <param name="commandParameterses">sql语句列表对应的参数列表，参数列表必须与sql语句列表匹配</param>
        /// <returns>事务执行是否成功</returns>
        public static bool ExecuteTransaction(SqlConnection conn, List<string> cmdTextes, List<SqlParameter[]> commandParameterses)
        {
            bool flag = false;
            if (cmdTextes.Count == commandParameterses.Count)
            {
                SqlTransaction sqlTran = conn.BeginTransaction();
                try
                {
                    for (int i = 0; i < cmdTextes.Count; i++)
                    {
                        ExecuteNonQuery(sqlTran, CommandType.Text, cmdTextes[i], commandParameterses[i]);
                    }
                    sqlTran.Commit();
                    flag = true;
                }
                catch (Exception e)
                {
                    sqlTran.Rollback();
                }
            }
            return flag;
        }
        #endregion

        #region 以事务执行sql语句列表，返回事务执行是否成功，public static bool ExecuteTransaction(string connectionString, List<string> cmdTextes, List<SqlParameter[]> commandParameterses)
        /// <summary>
        /// 以事务执行sql语句列表，返回事务执行是否成功
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="cmdTextes">sql语句列表</param>
        /// <param name="commandParameterses">sql语句列表对应的参数列表，参数列表必须与sql语句列表匹配</param>
        /// <returns></returns>
        public static bool ExecuteTransaction(string connectionString, List<string> cmdTextes, List<SqlParameter[]> commandParameterses)
        {
            bool flag = false;
            if (cmdTextes.Count == commandParameterses.Count)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlTransaction sqlTran = conn.BeginTransaction();
                    try
                    {
                        for (int i = 0; i < cmdTextes.Count; i++)
                        {
                            ExecuteNonQuery(sqlTran, CommandType.Text, cmdTextes[i], commandParameterses[i]);
                        }
                        sqlTran.Commit();
                        flag = true;
                    }
                    catch (Exception e)
                    {
                        sqlTran.Rollback();
                    }
                }
            }
            return flag;
        }
        #endregion

        #region 构造SQLCommand,private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
        /// <summary>
        /// 构造SQLCommand
        /// </summary>
        /// <param name="cmd">Command对象</param>
        /// <param name="conn">SqlConnection数据库连接对象</param>
        /// <param name="trans">SQL事务</param>
        /// <param name="cmdType">Command命令类型</param>
        /// <param name="cmdText">SQL语句或存储过程名称</param>
        /// <param name="cmdParms">参数数组</param>
        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = cmdText;

            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = cmdType;

            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }
        #endregion


        #region 拓展

      
        #endregion
    }
}