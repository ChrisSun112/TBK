using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Reflection;


namespace Tbk.Common
{
    public class MySqlHelper
    {


        private MySqlConnection connection ;

        public MySqlHelper()
        {
            connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["mysqlConn"].ToString());
        }

        public MySqlHelper(string connStr)
        {
            connection = new MySqlConnection(connStr);
        }



        #region 执行查询语句，返回MySqlDataReader
        /// <summary>
        /// 执行查询语句，返回MySqlDataReader
        /// </summary>
        /// <param name="sqlString"></param>
        /// <returns></returns>
        public MySqlDataReader ExecuteReader(string sqlString)
        { 
            MySqlCommand cmd = new MySqlCommand(sqlString, connection);
            MySqlDataReader myReader = null;
            try
            {
                connection.Open();
                myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return myReader;
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                connection.Close();
                throw new Exception(e.Message);
            }
            finally
            {
                if (myReader == null)
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }
        #endregion

        #region 执行带参数的查询语句，返回MySqlDataReader
        /// <summary>
        /// 执行带参数的查询语句，返回MySqlDataReader
        /// </summary>
        /// <param name="sqlString"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public  MySqlDataReader ExecuteReader(string sqlString, params MySqlParameter[] cmdParms)
        {
            
            MySqlCommand cmd = new MySqlCommand();
            MySqlDataReader myReader = null;
            try
            {
                PrepareCommand(cmd, connection, null, sqlString, cmdParms);
                myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                connection.Close();
                throw new Exception(e.Message);
            }
            finally
            {
                if (myReader == null)
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }
        #endregion


        #region 执行sql语句,返回执行行数
        /// <summary>
        /// 执行sql语句,返回执行行数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public  int ExecuteSql(string sql)
        {
            
            
            using (MySqlCommand cmd = new MySqlCommand(sql, connection))
            {
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (MySql.Data.MySqlClient.MySqlException e)
                {
                    connection.Close();
                    throw e;
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
            
        }
        #endregion

        #region 执行带参数的sql语句，并返回执行行数
        /// <summary>
        /// 执行带参数的sql语句，并返回执行行数
        /// </summary>
        /// <param name="sqlString"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public int ExecuteSql(string sqlString, params MySqlParameter[] cmdParms)
        {
           
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, sqlString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (System.Data.SqlClient.SqlException E)
                    {
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>mysql数据库
        /// <param name="SQLStringList">多条SQL语句</param>
        public void ExecuteSqlTran(List<string> SQLStringList)
        {
            using (MySqlCommand cmd = new MySqlCommand())
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                cmd.Connection = connection;
            
                MySqlTransaction tx = connection.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n].ToString();
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            cmd.ExecuteNonQuery();
                        }
                        //后来加上的
                        if (n > 0 && (n % 500 == 0 || n == SQLStringList.Count - 1))
                        {
                            tx.Commit();
                            tx = connection.BeginTransaction();
                        }
                    }
                    //tx.Commit();//原来一次性提交,改成每500条sql提交一次
                }
                catch (System.Data.SqlClient.SqlException E)
                {
                    tx.Rollback();
                    throw new Exception(E.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Dispose();
                }
            }
        }


        #endregion

        #region 执行查询语句，返回DataSet
        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataSet GetDataSet(string sql)
        {
           
            DataSet ds = new DataSet();
            try
            {
                connection.Open();
                MySqlDataAdapter DataAdapter = new MySqlDataAdapter(sql, connection);
                DataAdapter.Fill(ds);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
            return ds;
            
        }
        #endregion

        #region 执行带参数的查询语句，返回DataSet
        /// <summary>
        /// 执行带参数的查询语句，返回DataSet
        /// </summary>
        /// <param name="sqlString"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public DataSet GetDataSet(string sqlString, params MySqlParameter[] cmdParms)
        {
           
            MySqlCommand cmd = new MySqlCommand();
            PrepareCommand(cmd, connection, null, sqlString, cmdParms);
            using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();
                try
                {
                    da.Fill(ds, "ds");
                    cmd.Parameters.Clear();
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
                return ds;
            }
            
        }
        #endregion

        #region 执行带参数的sql语句，并返回object
        /// <summary>
        /// 执行带参数的sql语句，并返回object
        /// </summary>
        /// <param name="sqlString"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public object GetSingle(string sqlString, params MySqlParameter[] cmdParms)
        {
           
            using (MySqlCommand cmd = new MySqlCommand())
            {
                try
                {
                    PrepareCommand(cmd, connection, null, sqlString, cmdParms);
                    object obj = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();
                    if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                    {
                        return null;
                    }
                    else
                    {
                        return obj;
                    }
                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    throw new Exception(e.Message);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
            
        }
        #endregion
        
        public IList<T> QueryAll<T>(string sqlStr, params MySqlParameter[] cmdParms)
        {
            using(MySqlDataReader reader=ExecuteReader(sqlStr,cmdParms)){

                IList<T> list = new List<T>();

                try
                {
                    while (reader.Read())
                    {
                        //创建泛型对象
                        T _t = Activator.CreateInstance<T>();
                        //获取对象所有属性
                        PropertyInfo[] propertyInfo = _t.GetType().GetProperties();

                        foreach (PropertyInfo info in propertyInfo)
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                //属性名称和列名相同时赋值
                                if (reader.GetName(i).ToUpper().Equals(info.Name.ToUpper()))
                                {
                                    if (!reader.IsDBNull(i))
                                    {
                                        info.SetValue(_t, reader.GetValue(i), null);
                                    }
                                    else
                                    {
                                        info.SetValue(_t, null, null);
                                    }
                                    break;
                                }
                            }
                        }

                        list.Add(_t);
                    }

                }
                catch (Exception ex)
                {
                    connection.Close();
                    throw ex;
                }
                finally
                {
                    connection.Close();
                }
                
                
                return list;
            }
           
        }

        public T Query<T>(string sqlStr, params MySqlParameter[] cmdParms)
        {
            using (MySqlDataReader reader = ExecuteReader(sqlStr, cmdParms))
            {
                try
                {
                    if(reader.Read())
                    {
                        //创建泛型对象
                        T _t = Activator.CreateInstance<T>();
                        //获取对象所有属性
                        PropertyInfo[] propertyInfo = _t.GetType().GetProperties();

                        foreach (PropertyInfo info in propertyInfo)
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                //属性名称和列名相同时赋值
                                if (reader.GetName(i).ToUpper().Equals(info.Name.ToUpper()))
                                {
                                    if (!reader.IsDBNull(i))
                                    {
                                        info.SetValue(_t, reader.GetValue(i), null);
                                    }
                                    else
                                    {
                                        info.SetValue(_t, null, null);
                                    }
                                    break;
                                }
                            }
                        }
                        reader.Close();
                        return _t;
                    }
                    return default(T);

                }
                catch (Exception ex)
                {
                    reader.Close();
                    connection.Close();
                    throw ex;
                }
                finally
                {
                    reader.Close();
                    connection.Close();
                }

            }

        }

        public int QueryInt(string sqlStr, params MySqlParameter[] cmdParms)
        {
            using (MySqlDataReader reader = ExecuteReader(sqlStr, cmdParms))
            {
                try
                {
                    if (reader.Read())
                    {
                        int result = reader.GetInt32(0);
                       
                        reader.Close();
                        return result;
                    }
                    else
                    {
                        return -1;
                    }
                
                    
                }
                catch (Exception ex)
                {
                    reader.Close();
                    connection.Close();
                    throw ex;
                }
                finally
                {
                    reader.Close();
                    connection.Close();
                }

            }

        }


        /// <summary>
        /// 执行存储过程,返回数据集
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>DataSet</returns>
        public DataSet RunProcedureForDataSet(string storedProcName, IDataParameter[] parameters)
        {
           
                DataSet dataSet = new DataSet();
                connection.Open();
                MySqlDataAdapter sqlDA = new MySqlDataAdapter();
                sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
                sqlDA.Fill(dataSet);
                connection.Close();
               
                return dataSet;
            
        }

        /// <summary>
        /// 构建 SqlCommand 对象(用来返回一个结果集，而不是一个整数值)
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlCommand</returns>
        private MySqlCommand BuildQueryCommand(MySqlConnection connection, string storedProcName,
            IDataParameter[] parameters)
        {
            MySqlCommand command = new MySqlCommand(storedProcName, connection);
            command.CommandType = CommandType.StoredProcedure;
            foreach (MySqlParameter parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }
            return command;
        }

        #region 装载MySqlCommand对象
        /// <summary>
        /// 装载MySqlCommand对象
        /// </summary>
        private void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, string cmdText,MySqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            cmd.CommandTimeout = 300;
            if (trans != null)
            {
                cmd.Transaction = trans;
            }
            cmd.CommandType = CommandType.Text; //cmdType;
            if (cmdParms != null)
            {
                foreach (MySqlParameter parm in cmdParms)
                {
                    cmd.Parameters.Add(parm);
                }
            }
        }
        #endregion
    }
}
