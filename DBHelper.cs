using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;


namespace SBS
{
    /// <summary>
    /// SqlHelper操作类
    /// </summary>
    public static partial class DBHelper
    {
        /// <summary>
        /// 批量操作每批次记录数
        /// </summary>
        private const int BatchSize = 2000;


        /// <summary>
        /// 超时时间
        /// </summary>
        private const int CommandTimeOut = 600;


        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        //private static readonly string ConnString = "Data Source=VINIU\\ms2014;Initial Catalog=ViNiuOA;User ID=viniuoa1;Password=viniuoa1";
        //private static readonly string ConnString = "Data Source=.;Initial Catalog=ViNiuOA;Integrated Security=True";
        private static readonly string ConnString = ConfigurationManager.ConnectionStrings["SqlConnection"].ConnectionString;
        //private static readonly string ConnString = ConfigurationManager.AppSettings["Connstr"];
        #region 静态方法


        private static void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction,
            CommandType commandType, string commandText, SqlParameter[] parms)
        {
            if (connection.State != ConnectionState.Open) connection.Open();


            command.Connection = connection;
            command.CommandTimeout = CommandTimeOut;
            // 设置命令文本(存储过程名或SQL语句)
            command.CommandText = commandText;
            // 分配事务
            if (transaction != null)
            {
                command.Transaction = transaction;
            }
            // 设置命令类型.
            command.CommandType = commandType;
            if (parms != null && parms.Length > 0)
            {
                //预处理SqlParameter参数数组，将为NULL的参数赋值为DBNull.Value;
                foreach (var parameter in parms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput ||
                         parameter.Direction == ParameterDirection.Input) && (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                }
                command.Parameters.AddRange(parms);
            }
        }


        #region ExecuteNonQuery


        /// <summary>
        /// 执行SQL语句,返回影响的行数
        /// </summary>
        /// <param name="commandText">SQL语句</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回影响的行数</returns>
        public static int ExecuteNonQuery(string commandText, params SqlParameter[] parms)
        {
            using (var connection = new SqlConnection(ConnString))
            {
                return ExecuteNonQuery(connection, CommandType.Text, commandText, parms);
            }
        }


        /// <summary>
        /// 执行SQL语句,返回影响的行数
        /// </summary>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回影响的行数</returns>
        public static int ExecuteNonQuery(CommandType commandType, string commandText, params SqlParameter[] parms)
        {
            using (var connection = new SqlConnection(ConnString))
            {
                return ExecuteNonQuery(connection, commandType, commandText, parms);
            }
        }


        /// <summary>
        /// 执行SQL语句,返回影响的行数
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回影响的行数</returns>
        public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText,
            params SqlParameter[] parms)
        {
            return ExecuteNonQuery(connection, null, commandType, commandText, parms);
        }


        /// <summary>
        /// 执行SQL语句,返回影响的行数
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回影响的行数</returns>
        public static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText,
            params SqlParameter[] parms)
        {
            return ExecuteNonQuery(transaction.Connection, transaction, commandType, commandText, parms);
        }


        /// <summary>
        /// 执行SQL语句,返回影响的行数
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回影响的行数</returns>
        private static int ExecuteNonQuery(SqlConnection connection, SqlTransaction transaction, CommandType commandType,
            string commandText, params SqlParameter[] parms)
        {
            var command = new SqlCommand();
            PrepareCommand(command, connection, transaction, commandType, commandText, parms);
            var retval = command.ExecuteNonQuery();
            command.Parameters.Clear();
            return retval;
        }


        #endregion ExecuteNonQuery


        #region ExecuteScalar


        /// <summary>
        /// 执行SQL语句,返回结果集中的第一行第一列
        /// </summary>
        /// <typeparam name="T">返回对象类型</typeparam>
        /// <param name="commandText">SQL语句</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static T ExecuteScalar<T>(string commandText, params SqlParameter[] parms)
        {
            var result = ExecuteScalar(commandText, parms);
            if (result != null)
            {
                return (T)Convert.ChangeType(result, typeof(T));
                ;
            }
            return default(T);
        }


        /// <summary>
        /// 执行SQL语句,返回结果集中的第一行第一列
        /// </summary>
        /// <param name="commandText">SQL语句</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(string commandText, params SqlParameter[] parms)
        {
            using (var connection = new SqlConnection(ConnString))
            {
                return ExecuteScalar(connection, CommandType.Text, commandText, parms);
            }
        }


        /// <summary>
        /// 执行SQL语句,返回结果集中的第一行第一列
        /// </summary>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(CommandType commandType, string commandText, params SqlParameter[] parms)
        {
            using (var connection = new SqlConnection(ConnString))
            {
                return ExecuteScalar(connection, commandType, commandText, parms);
            }
        }


        /// <summary>
        /// 执行SQL语句,返回结果集中的第一行第一列
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText,
            params SqlParameter[] parms)
        {
            return ExecuteScalar(connection, null, commandType, commandText, parms);
        }


        /// <summary>
        /// 执行SQL语句,返回结果集中的第一行第一列
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText,
            params SqlParameter[] parms)
        {
            return ExecuteScalar(transaction.Connection, transaction, commandType, commandText, parms);
        }


        /// <summary>
        /// 执行SQL语句,返回结果集中的第一行第一列
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        private static object ExecuteScalar(SqlConnection connection, SqlTransaction transaction,
            CommandType commandType, string commandText, params SqlParameter[] parms)
        {
            var command = new SqlCommand();
            PrepareCommand(command, connection, transaction, commandType, commandText, parms);
            var retval = command.ExecuteScalar();
            command.Parameters.Clear();
            return retval;
        }


        #endregion ExecuteScalar


        #region ExecuteDataReader


        /// <summary>
        /// 执行SQL语句,返回只读数据集
        /// </summary>
        /// <param name="commandText">SQL语句</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回只读数据集</returns>
        private static SqlDataReader ExecuteDataReader(string commandText, params SqlParameter[] parms)
        {
            var connection = new SqlConnection(ConnString);
            return ExecuteDataReader(connection, null, CommandType.Text, commandText, parms);
        }


        /// <summary>
        /// 执行SQL语句,返回只读数据集
        /// </summary>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回只读数据集</returns>
        private static SqlDataReader ExecuteDataReader(CommandType commandType, string commandText,
            params SqlParameter[] parms)
        {
            var connection = new SqlConnection(ConnString);
            return ExecuteDataReader(connection, null, commandType, commandText, parms);
        }


        /// <summary>
        /// 执行SQL语句,返回只读数据集
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回只读数据集</returns>
        private static SqlDataReader ExecuteDataReader(SqlConnection connection, CommandType commandType,
            string commandText, params SqlParameter[] parms)
        {
            return ExecuteDataReader(connection, null, commandType, commandText, parms);
        }


        /// <summary>
        /// 执行SQL语句,返回只读数据集
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回只读数据集</returns>
        private static SqlDataReader ExecuteDataReader(SqlTransaction transaction, CommandType commandType,
            string commandText, params SqlParameter[] parms)
        {
            return ExecuteDataReader(transaction.Connection, transaction, commandType, commandText, parms);
        }


        /// <summary>
        /// 执行SQL语句,返回只读数据集
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回只读数据集</returns>
        private static SqlDataReader ExecuteDataReader(SqlConnection connection, SqlTransaction transaction,
            CommandType commandType, string commandText, params SqlParameter[] parms)
        {
            var command = new SqlCommand();
            PrepareCommand(command, connection, transaction, commandType, commandText, parms);
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }


        #endregion


        #region ExecuteDataRow


        /// <summary>
        /// 执行SQL语句,返回结果集中的第一行
        /// </summary>
        /// <param name="commandText">SQL语句</param>
        /// <param name="parms">查询参数</param>
        /// <returns>,返回结果集中的第一行</returns>
        public static DataRow ExecuteDataRow(string commandText, params SqlParameter[] parms)
        {
            var dt = ExecuteDataTable(CommandType.Text, commandText, parms);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }


        /// <summary>
        /// 执行SQL语句,返回结果集中的第一行
        /// </summary>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>,返回结果集中的第一行</returns>
        public static DataRow ExecuteDataRow(CommandType commandType, string commandText, params SqlParameter[] parms)
        {
            var dt = ExecuteDataTable(commandType, commandText, parms);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }


        /// <summary>
        /// 执行SQL语句,返回结果集中的第一行
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>,返回结果集中的第一行</returns>
        public static DataRow ExecuteDataRow(SqlConnection connection, CommandType commandType, string commandText,
            params SqlParameter[] parms)
        {
            var dt = ExecuteDataTable(connection, commandType, commandText, parms);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }


        /// <summary>
        /// 执行SQL语句,返回结果集中的第一行
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>,返回结果集中的第一行</returns>
        public static DataRow ExecuteDataRow(SqlTransaction transaction, CommandType commandType, string commandText,
            params SqlParameter[] parms)
        {
            var dt = ExecuteDataTable(transaction, commandType, commandText, parms);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }


        #endregion ExecuteDataRow


        #region ExecuteDataTable


        /// <summary>
        /// 执行SQL语句,返回结果集中的第一个数据表
        /// </summary>
        /// <param name="commandText">SQL语句</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集中的第一个数据表</returns>
        public static DataTable ExecuteDataTable(string commandText, params SqlParameter[] parms)
        {
            return ExecuteDataSet(commandText, parms).Tables[0];
        }


        /// <summary>
        /// 执行SQL语句,返回结果集中的第一个数据表
        /// </summary>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集中的第一个数据表</returns>
        public static DataTable ExecuteDataTable(CommandType commandType, string commandText,
            params SqlParameter[] parms)
        {
            return ExecuteDataSet(commandType, commandText, parms).Tables[0];
        }


        /// <summary>
        /// 执行SQL语句,返回结果集中的第一个数据表
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集中的第一个数据表</returns>
        public static DataTable ExecuteDataTable(SqlConnection connection, CommandType commandType, string commandText,
            params SqlParameter[] parms)
        {
            return ExecuteDataSet(connection, commandType, commandText, parms).Tables[0];
        }


        /// <summary>
        /// 执行SQL语句,返回结果集中的第一个数据表
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集中的第一个数据表</returns>
        public static DataTable ExecuteDataTable(SqlTransaction transaction, CommandType commandType, string commandText,
            params SqlParameter[] parms)
        {
            return ExecuteDataSet(transaction, commandType, commandText, parms).Tables[0];
        }


        /// <summary>
        /// 获取空表结构
        /// </summary>
        /// <param name="tableName">数据表名称</param>
        /// <returns>返回结果集中的第一个数据表</returns>
        public static DataTable ExecuteEmptyDataTable(string tableName)
        {
            return ExecuteDataSet(CommandType.Text, string.Format("select * from {0} where 1=-1", tableName)).Tables[0];
        }


        /// <summary>
        ///  执行SQL语句,返回结果集中的第一个数据表
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="order">排序SQL,如"ORDER BY ID DESC"</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pageIndex">页索引</param>
        /// <param name="parms">查询参数</param>      
        /// <param name="query">查询SQL</param>
        /// <param name="cte">CTE表达式</param>
        /// <returns></returns>
        public static DataTable ExecutePageDataTable(string sql, string order, int pageSize, int pageIndex,
            SqlParameter[] parms = null, string query = null, string cte = null)
        {
            var psql = string.Format(@"
                                        {3}
                                        SELECT  *
                                        FROM    (
                                                 SELECT ROW_NUMBER() OVER (ORDER BY {1}) RowNumber,*
                                                 FROM   (
                                                         {0}
                                                        ) t
                                                 WHERE  1 = 1 {2}
                                                ) t
                                        WHERE   RowNumber BETWEEN @RowNumber_Begin
                                                          AND     @RowNumber_End", sql, order, query, cte);


            var paramlist = new List<SqlParameter>()
            {
                new SqlParameter("@RowNumber_Begin", SqlDbType.Int) {Value = (pageIndex - 1)*pageSize + 1},
                new SqlParameter("@RowNumber_End", SqlDbType.Int) {Value = pageIndex*pageSize}
            };
            if (parms != null) paramlist.AddRange(parms);
            return ExecuteDataTable(psql, paramlist.ToArray());
        }


        #endregion ExecuteDataTable


        #region ExecuteDataSet


        /// <summary>
        /// 执行SQL语句,返回结果集
        /// </summary>
        /// <param name="commandText">SQL语句</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集</returns>
        public static DataSet ExecuteDataSet(string commandText, params SqlParameter[] parms)
        {
            return ExecuteDataSet(CommandType.Text, commandText, parms);
        }


        /// <summary>
        /// 执行SQL语句,返回结果集
        /// </summary>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集</returns>
        public static DataSet ExecuteDataSet(CommandType commandType, string commandText, params SqlParameter[] parms)
        {
            using (var connection = new SqlConnection(ConnString))
            {
                return ExecuteDataSet(connection, commandType, commandText, parms);
            }
        }


        /// <summary>
        /// 执行SQL语句,返回结果集
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集</returns>
        public static DataSet ExecuteDataSet(SqlConnection connection, CommandType commandType, string commandText,
            params SqlParameter[] parms)
        {
            return ExecuteDataSet(connection, null, commandType, commandText, parms);
        }


        /// <summary>
        /// 执行SQL语句,返回结果集
        /// </summary>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集</returns>
        public static DataSet ExecuteDataSet(SqlTransaction transaction, CommandType commandType, string commandText,
            params SqlParameter[] parms)
        {
            return ExecuteDataSet(transaction.Connection, transaction, commandType, commandText, parms);
        }


        /// <summary>
        /// 执行SQL语句,返回结果集
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型(存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">SQL语句或存储过程名称</param>
        /// <param name="parms">查询参数</param>
        /// <returns>返回结果集</returns>
        private static DataSet ExecuteDataSet(SqlConnection connection, SqlTransaction transaction,
            CommandType commandType, string commandText, params SqlParameter[] parms)
        {
            var command = new SqlCommand();


            PrepareCommand(command, connection, transaction, commandType, commandText, parms);
            var adapter = new SqlDataAdapter(command);


            var ds = new DataSet();
            adapter.Fill(ds);
            if (commandText.IndexOf("@", System.StringComparison.Ordinal) > 0)
            {
                commandText = commandText.ToLower();
                var index = commandText.IndexOf("where ", System.StringComparison.Ordinal);
                if (index < 0)
                {
                    index = commandText.IndexOf("\nwhere", System.StringComparison.Ordinal);
                }
                if (index > 0)
                {
                    ds.ExtendedProperties.Add("SQL", commandText.Substring(0, index - 1));
                    //将获取的语句保存在表的一个附属数组里，方便更新时生成CommandBuilder
                }
                else
                {
                    ds.ExtendedProperties.Add("SQL", commandText); //将获取的语句保存在表的一个附属数组里，方便更新时生成CommandBuilder
                }
            }
            else
            {
                ds.ExtendedProperties.Add("SQL", commandText); //将获取的语句保存在表的一个附属数组里，方便更新时生成CommandBuilder
            }


            foreach (DataTable dt in ds.Tables)
            {
                dt.ExtendedProperties.Add("SQL", ds.ExtendedProperties["SQL"]);
            }


            command.Parameters.Clear();
            return ds;
        }


        #endregion ExecuteDataSet


        #region 批量操作


        /// <summary>
        /// 大批量数据插入
        /// </summary>
        /// <param name="table">数据表</param>
        public static bool BulkInsert(DataTable table)
        {
            try
            {
                if (string.IsNullOrEmpty(table.TableName))
                    return false;

                using (var bulk = new SqlBulkCopy(ConnString))
                {
                    bulk.BatchSize = BatchSize;
                    bulk.BulkCopyTimeout = CommandTimeOut;
                    bulk.DestinationTableName = table.TableName;
                    foreach (DataColumn col in table.Columns)
                    {
                        bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                    }
                    bulk.WriteToServer(table);
                    bulk.Close();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// 使用MySqlDataAdapter批量更新数据
        /// </summary>
        /// <param name="table">数据表</param>
        public static void BatchUpdate(DataTable table)
        {
            var connection = new SqlConnection(ConnString);
            var command = connection.CreateCommand();
            command.CommandTimeout = CommandTimeOut;
            command.CommandType = CommandType.Text;
            var adapter = new SqlDataAdapter(command);
            var commandBulider = new SqlCommandBuilder(adapter);
            commandBulider.ConflictOption = ConflictOption.OverwriteChanges;


            SqlTransaction transaction = null;
            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();
                //设置批量更新的每次处理条数
                adapter.UpdateBatchSize = BatchSize;
                //设置事物
                adapter.SelectCommand.Transaction = transaction;


                if (table.ExtendedProperties["SQL"] != null)
                {
                    adapter.SelectCommand.CommandText = table.ExtendedProperties["SQL"].ToString();
                }
                adapter.Update(table);
                transaction.Commit(); //提交事务
            }
            catch (SqlException ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }


        /// <summary>
        /// 分批次批量删除数据
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="batchSize">每批次更新记录行数</param>
        /// <param name="interval">批次执行间隔(秒)</param>
        public static void BatchDelete(string sql, int batchSize = 1000, int interval = 1)
        {
            sql = sql.ToLower();


            if (batchSize < 1000) batchSize = 1000;
            if (interval < 1) interval = 1;
            while (ExecuteScalar(sql.Replace("delete", "select top 1 1")) != null)
            {
                ExecuteNonQuery(CommandType.Text, sql.Replace("delete", string.Format("delete top ({0})", batchSize)));
                System.Threading.Thread.Sleep(interval * 1000);
            }
        }


        /// <summary>
        /// 分批次批量更新数据
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="batchSize">每批次更新记录行数</param>
        /// <param name="interval">批次执行间隔(秒)</param>
        public static void BatchUpdate(string sql, int batchSize = 1000, int interval = 1)
        {
            if (batchSize < 1000) batchSize = 1000;
            if (interval < 1) interval = 1;
            var existsSql = Regex.Replace(sql, @"[\w\s.=,']*from", "select top 1 1 from", RegexOptions.IgnoreCase);
            existsSql = Regex.Replace(existsSql, @"set[\w\s.=,']* where", "where", RegexOptions.IgnoreCase);
            existsSql = Regex.Replace(existsSql, @"update", "select top 1 1 from", RegexOptions.IgnoreCase);
            while (ExecuteScalar<int>(existsSql) != 0)
            {
                ExecuteNonQuery(CommandType.Text,
                    Regex.Replace(sql, "update", string.Format("update top ({0})", batchSize), RegexOptions.IgnoreCase));
                System.Threading.Thread.Sleep(interval * 1000);
            }
        }


        #endregion 批量操作


        #endregion 静态方法


        #region 执行一个带参数的存储过程，返回 DataSet +DataSet RunProcedure(string commandText, SqlParameter [] commandParameters)


        /// <summary>
        /// 执行一个带参数的存储过程，返回 DataSet
        /// 格式 DataSet ds = ExecuteDataset("GetOrders", new SqlParameter("@prodid", 24));
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        public static DataSet RunProcedure(string commandText, SqlParameter[] commandParameters)
        {
            using (var myConn = new SqlConnection(ConnString))
            {
                //创建一个命令
                var sqlCommand = new SqlCommand();
                sqlCommand.CommandText = commandText;
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Connection = myConn;
                sqlCommand.Parameters.Clear();
                sqlCommand.Parameters.AddRange(commandParameters);


                //创建 DataAdapter 和 DataSet
                var da = new SqlDataAdapter(sqlCommand);
                var ds = new DataSet();
                try
                {
                    da.Fill(ds);
                    var xx = sqlCommand.Parameters[3].Value;
                    sqlCommand.Parameters.Clear();
                    //返回结果集
                    return ds;
                }
                catch (Exception ex)
                {
                    var parasList = "";
                    foreach (var para in commandParameters)
                    {
                        parasList += "<\r\n>@" + para.ParameterName + ":" + para.Value;
                    }
                    throw new Exception(ex + "<\r\n>" + commandText + parasList);
                }
            }
        }


        #endregion
    }

    public static partial class DBHelper
    {
        #region Schema 数据库结构


        /// <summary>
        /// 执行指定数据库连接字符串的命令,返回服务器数据库名称数组
        /// </summary>
        /// <returns></returns>
        public static string[] GetDatabases()
        {
            const string sql = "select name from sys.databases where name not in ('master','model','msdb','tempdb')";
            var dt = ExecuteDataTable(sql);
            return dt.Rows.Cast<DataRow>().Select(row => row["name"].ToString()).ToArray();
        }


        /// <summary>
        /// 执行指定数据库连接字符串的命令,返回指定数据库的表信息
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static List<DbTable> GetDbTables(string database)
        {
            #region /// SQL


            var sql = string.Format(@"SELECT
                                        obj.name tablename,
                                        schem.name schemname,
                                        idx.rows,
                                        CAST
                                        (
	                                        CASE 
		                                        WHEN (SELECT COUNT(1) FROM sys.indexes WHERE object_id= obj.OBJECT_ID AND is_primary_key=1) >=1 THEN 1
		                                        ELSE 0
	                                        END 
                                        AS BIT) HasPrimaryKey                                         
                                        from {0}.sys.objects obj 
                                        inner join {0}.dbo.sysindexes idx on obj.object_id=idx.id and idx.indid<=1
                                        INNER JOIN {0}.sys.schemas schem ON obj.schema_id=schem.schema_id
                                        where type='U' 
                                        order by obj.name", database);
            #endregion


            var dt = ExecuteDataTable(sql);
            return dt.Rows.Cast<DataRow>().Select(row => new DbTable
            {
                TableName = row.Field<string>("tablename"),
                SchemaName = row.Field<string>("schemname"),
                Rows = row.Field<int>("rows"),
                HasPrimaryKey = row.Field<bool>("HasPrimaryKey")
            }).ToList();
        }


        /// <summary>
        /// 执行指定数据库连接字符串的命令,返回指定数据库、表的字段信息
        /// </summary>
        /// <param name="database"></param>
        /// <param name="tableName"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public static List<DbColumn> GetDbColumns(string database, string tableName, string schema = "dbo")
        {
            #region /// SQL
            var sql = string.Format(@"
                                        WITH indexCTE AS
                                        (
	                                        SELECT 
                                            ic.column_id,
                                            ic.index_column_id,
                                            ic.object_id    
                                            FROM {0}.sys.indexes idx
                                            INNER JOIN {0}.sys.index_columns ic ON idx.index_id = ic.index_id AND idx.object_id = ic.object_id
                                            WHERE  idx.object_id =OBJECT_ID(@tableName) AND idx.is_primary_key=1
                                        )
                                        select
                                        colm.column_id ColumnID,
                                        CAST(CASE WHEN indexCTE.column_id IS NULL THEN 0 ELSE 1 END AS BIT) IsPrimaryKey,
                                        colm.name ColumnName,
                                        systype.name ColumnType,
                                        colm.is_identity IsIdentity,
                                        colm.is_nullable IsNullable,
                                        cast(colm.max_length as int) ByteLength,
                                        (
                                            case 
                                                when systype.name='nvarchar' and colm.max_length>0 then colm.max_length/2 
                                                when systype.name='nchar' and colm.max_length>0 then colm.max_length/2
                                                when systype.name='ntext' and colm.max_length>0 then colm.max_length/2 
                                                else colm.max_length
                                            end
                                        ) CharLength,
                                        cast(colm.precision as int) Precision,
                                        cast(colm.scale as int) Scale,
                                        prop.value Remark
                                        from {0}.sys.columns colm
                                        inner join {0}.sys.types systype on colm.system_type_id=systype.system_type_id and colm.user_type_id=systype.user_type_id
                                        left join {0}.sys.extended_properties prop on colm.object_id=prop.major_id and colm.column_id=prop.minor_id
                                        LEFT JOIN indexCTE ON colm.column_id=indexCTE.column_id AND colm.object_id=indexCTE.object_id                                        
                                        where colm.object_id=OBJECT_ID(@tableName)
                                        order by colm.column_id", database);
            #endregion


            var param = new SqlParameter("@tableName", SqlDbType.NVarChar, 100) { Value = string.Format("{0}.{1}.{2}", database, schema, tableName) };
            var dt = ExecuteDataTable(sql, param);
            return dt.Rows.Cast<DataRow>().Select(row => new DbColumn()
            {
                ColumnID = row.Field<int>("ColumnID"),
                IsPrimaryKey = row.Field<bool>("IsPrimaryKey"),
                ColumnName = row.Field<string>("ColumnName"),
                ColumnType = row.Field<string>("ColumnType"),
                IsIdentity = row.Field<bool>("IsIdentity"),
                IsNullable = row.Field<bool>("IsNullable"),
                ByteLength = row.Field<int>("ByteLength"),
                CharLength = row.Field<int>("CharLength"),
                Scale = row.Field<int>("Scale"),
                Remark = row["Remark"].ToString()
            }).ToList();
        }


        /// <summary>
        /// 执行指定数据库连接字符串的命令,返回指定数据库、表的索引信息
        /// </summary>
        /// <param name="database"></param>
        /// <param name="tableName"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public static List<DbIndex> GetDbIndexs(string database, string tableName, string schema = "dbo")
        {
            #region /// SQL
            var sql = string.Format(@"
                                        select 
                                        idx.name IndexName
                                        ,idx.type_desc IndexType
                                        ,idx.is_primary_key IsPrimaryKey
                                        ,idx.is_unique IsUnique
                                        ,idx.is_unique_constraint IsUniqueConstraint
                                        ,STUFF(
                                        (
	                                        SELECT  ','+c.name from {0}.sys.index_columns ic
	                                        inner join {0}.sys.columns c on ic.column_id=c.column_id and ic.object_id=c.object_id
	                                        WHERE ic.is_included_column = 0 and ic.index_id=idx.index_id AND ic.object_id=idx.object_id
	                                        ORDER BY ic.key_ordinal
	                                        FOR XML PATH('')
                                        ),1,1,'') IndexColumns
                                        ,STUFF(
                                        (
	                                        SELECT  ','+c.name from {0}.sys.index_columns ic
	                                        inner join {0}.sys.columns c on ic.column_id=c.column_id and ic.object_id=c.object_id
	                                        WHERE ic.is_included_column = 1 and ic.index_id=idx.index_id AND ic.object_id=idx.object_id
	                                        ORDER BY ic.key_ordinal
	                                        FOR XML PATH('')
                                        ),1,1,'') IncludeColumns
                                        from {0}.sys.indexes idx
                                        where object_id =OBJECT_ID(@tableName)", database);
            #endregion


            var param = new SqlParameter("@tableName", SqlDbType.NVarChar, 100) { Value = string.Format("{0}.{1}.{2}", database, schema, tableName) };
            var dt = ExecuteDataTable(sql, param);
            return dt.Rows.Cast<DataRow>().Select(row => new DbIndex()
            {
                IndexName = row.Field<string>("IndexName"),
                IndexType = row.Field<string>("IndexType"),
                IsPrimaryKey = row.Field<bool>("IsPrimaryKey"),
                IsUnique = row.Field<bool>("IsUnique"),
                IsUniqueConstraint = row.Field<bool>("IsUniqueConstraint"),
                IndexColumns = row.Field<string>("IndexColumns"),
                IncludeColumns = row.Field<string>("IncludeColumns")
            }).ToList();
        }


        #endregion
    }


    /// <summary>
    /// 表索引结构
    /// </summary>
    public sealed class DbIndex
    {
        /// <summary>
        /// 索引名称
        /// </summary>
        public string IndexName { get; set; }
        /// <summary>
        /// 索引类型
        /// </summary>
        public string IndexType { get; set; }
        /// <summary>
        /// 是否为主键
        /// </summary>
        public bool IsPrimaryKey { get; set; }


        /// <summary>
        /// 是否唯一索引
        /// </summary>
        public bool IsUnique { get; set; }
        /// <summary>
        /// 是否唯一约束
        /// </summary>
        public bool IsUniqueConstraint { get; set; }
        /// <summary>
        /// 索引列
        /// </summary>
        public string IndexColumns { get; set; }


        /// <summary>
        /// 覆盖索引列
        /// </summary>
        public string IncludeColumns { get; set; }
    }


    /// <summary>
    /// 表结构
    /// </summary>
    public sealed class DbTable
    {
        /// <summary>
        /// 表名称
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 表的架构
        /// </summary>
        public string SchemaName { get; set; }
        /// <summary>
        /// 表的记录数
        /// </summary>
        public int Rows { get; set; }


        /// <summary>
        /// 是否含有主键
        /// </summary>
        public bool HasPrimaryKey { get; set; }
    }


    /// <summary>
    /// 表字段结构
    /// </summary>
    public sealed class DbColumn
    {
        /// <summary>
        /// 字段ID
        /// </summary>
        public int ColumnID { get; set; }


        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsPrimaryKey { get; set; }


        /// <summary>
        /// 字段名称
        /// </summary>
        public string ColumnName { get; set; }


        /// <summary>
        /// 字段类型
        /// </summary>
        public string ColumnType { get; set; }


        /// <summary>
        /// 数据库类型对应的C#类型
        /// </summary>
        public string CSharpType
        {
            get
            {
                return SqlMap.MapCsharpType(ColumnType);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public Type CommonType
        {
            get
            {
                return SqlMap.MapCommonType(ColumnType);
            }
        }


        /// <summary>
        /// 字节长度
        /// </summary>
        public int ByteLength { get; set; }


        /// <summary>
        /// 字符长度
        /// </summary>
        public int CharLength { get; set; }


        /// <summary>
        /// 小数位
        /// </summary>
        public int Scale { get; set; }


        /// <summary>
        /// 是否自增列
        /// </summary>
        public bool IsIdentity { get; set; }


        /// <summary>
        /// 是否允许空
        /// </summary>
        public bool IsNullable { get; set; }


        /// <summary>
        /// 描述
        /// </summary>
        public string Remark { get; set; }
    }


    /// <summary>
    /// Sql类型转换
    /// </summary>
    public class SqlMap
    {
        public static string MapCsharpType(string dbtype)
        {
            if (string.IsNullOrEmpty(dbtype)) return dbtype;
            dbtype = dbtype.ToLower();
            string csharpType = "object";
            switch (dbtype)
            {
                case "bigint": csharpType = "long"; break;
                case "binary": csharpType = "byte[]"; break;
                case "bit": csharpType = "bool"; break;
                case "char": csharpType = "string"; break;
                case "date": csharpType = "DateTime"; break;
                case "datetime": csharpType = "DateTime"; break;
                case "datetime2": csharpType = "DateTime"; break;
                case "datetimeoffset": csharpType = "DateTimeOffset"; break;
                case "decimal": csharpType = "decimal"; break;
                case "float": csharpType = "double"; break;
                case "image": csharpType = "byte[]"; break;
                case "int": csharpType = "int"; break;
                case "money": csharpType = "decimal"; break;
                case "nchar": csharpType = "string"; break;
                case "ntext": csharpType = "string"; break;
                case "numeric": csharpType = "decimal"; break;
                case "nvarchar": csharpType = "string"; break;
                case "real": csharpType = "Single"; break;
                case "smalldatetime": csharpType = "DateTime"; break;
                case "smallint": csharpType = "short"; break;
                case "smallmoney": csharpType = "decimal"; break;
                case "sql_variant": csharpType = "object"; break;
                case "sysname": csharpType = "object"; break;
                case "text": csharpType = "string"; break;
                case "time": csharpType = "TimeSpan"; break;
                case "timestamp": csharpType = "byte[]"; break;
                case "tinyint": csharpType = "byte"; break;
                case "uniqueidentifier": csharpType = "Guid"; break;
                case "varbinary": csharpType = "byte[]"; break;
                case "varchar": csharpType = "string"; break;
                case "xml": csharpType = "string"; break;
                default: csharpType = "object"; break;
            }
            return csharpType;
        }


        public static Type MapCommonType(string dbtype)
        {
            if (string.IsNullOrEmpty(dbtype)) return Type.Missing.GetType();
            dbtype = dbtype.ToLower();
            Type commonType = typeof(object);
            switch (dbtype)
            {
                case "bigint": commonType = typeof(long); break;
                case "binary": commonType = typeof(byte[]); break;
                case "bit": commonType = typeof(bool); break;
                case "char": commonType = typeof(string); break;
                case "date": commonType = typeof(DateTime); break;
                case "datetime": commonType = typeof(DateTime); break;
                case "datetime2": commonType = typeof(DateTime); break;
                case "datetimeoffset": commonType = typeof(DateTimeOffset); break;
                case "decimal": commonType = typeof(decimal); break;
                case "float": commonType = typeof(double); break;
                case "image": commonType = typeof(byte[]); break;
                case "int": commonType = typeof(int); break;
                case "money": commonType = typeof(decimal); break;
                case "nchar": commonType = typeof(string); break;
                case "ntext": commonType = typeof(string); break;
                case "numeric": commonType = typeof(decimal); break;
                case "nvarchar": commonType = typeof(string); break;
                case "real": commonType = typeof(Single); break;
                case "smalldatetime": commonType = typeof(DateTime); break;
                case "smallint": commonType = typeof(short); break;
                case "smallmoney": commonType = typeof(decimal); break;
                case "sql_variant": commonType = typeof(object); break;
                case "sysname": commonType = typeof(object); break;
                case "text": commonType = typeof(string); break;
                case "time": commonType = typeof(TimeSpan); break;
                case "timestamp": commonType = typeof(byte[]); break;
                case "tinyint": commonType = typeof(byte); break;
                case "uniqueidentifier": commonType = typeof(Guid); break;
                case "varbinary": commonType = typeof(byte[]); break;
                case "varchar": commonType = typeof(string); break;
                case "xml": commonType = typeof(string); break;
                default: commonType = typeof(object); break;
            }
            return commonType;
        }
    }
}