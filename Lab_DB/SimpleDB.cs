using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Lab_DB
{
    public static class SimpleDB
    {
        public static Dictionary<eDBType, DBConfig> mDBConfigMap { get; private set; }
        private static bool msInitFlag = false;

        private static int mTmpExCount = 0;

        public static void Initialize(Dictionary<eDBType, DBConfig> dbConfigMap)
        {
            if (dbConfigMap == null)
                throw new ArgumentNullException(nameof(dbConfigMap));

            if (msInitFlag == false)
            {
                mDBConfigMap = dbConfigMap;
                msInitFlag = true;
            }
        }

        private static bool Connect(eDBType type, out SqlConnection connection)
        {
            connection = new SqlConnection(mDBConfigMap[type].GetConnectionString());
            if (connection != null)
            {
                connection.Open();
                return true;
            }
            else
            {
                connection = null;
                return false;
            }
        }

        private static SqlConnection dbOpenSync(eDBType type)
        {
            var dbData = mDBConfigMap;
            if (dbData.ContainsKey(type))
            {
                var connection = new SqlConnection(dbData[type].GetConnectionString());
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                return connection;
            }
            return null;          
        }

        private static async Task<bool> ConnectAsync(eDBType type, SqlConnection connection)
        {
            connection = new SqlConnection(mDBConfigMap[type].GetConnectionString());
            if (connection != null)
            {
                await connection.OpenAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        private static async Task<SqlConnection> dbOpenAsync(eDBType type)
        {
            var dbData = mDBConfigMap;
            if (dbData.ContainsKey(type))
            {
                var connection = new SqlConnection(dbData[type].GetConnectionString());
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();                 
                return connection;
            }
            return null;
        }

        #region "query (sync)"
        /// <summary>
        /// 단일 쿼리 실행 메서드(트랜잭션처리) - 결과를 받아올 필요가 없을 때 (UPDATE, INSERT, DELETE...)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="query">실행할 쿼리</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">쿼리 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static int ExecuteQueryTransaction(eDBType type, string query, SqlParameter parameter = null, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;
            int result = 0;
            try
            {
                var connected = Connect(type, out connection);
                if (connected)
                {
                    using (transaction = connection.BeginTransaction())
                    using (var command = new SqlCommand(query, connection, transaction))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = timeout;

                        if (parameter != null)
                            command.Parameters.Add(parameter);

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        command.Prepare();
                        command.ExecuteNonQuery();
                        //Task.Run(async () => { result = await command.ExecuteNonQueryAsync(); }).Wait();

                        transaction.Commit();
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                transaction?.Rollback();
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryTransaction - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryTransaction - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.Close();
            }

            return result;
        }

        /// <summary>
        /// 단일 쿼리 실행 메서드(트랜잭션처리) - 결과를 받아올 필요가 없을 때 (UPDATE, INSERT, DELETE...)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="query">실행할 쿼리</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">쿼리 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static int ExecuteQueryTransaction(eDBType type, string query, IEnumerable<SqlParameter> parameters = null, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;
            int result = 0;
            try
            {
                var connected = Connect(type, out connection);
                if (connected)
                {
                    using(transaction = connection.BeginTransaction())
                    using (var command = new SqlCommand(query, connection, transaction))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = timeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                                command.Parameters.Add(parameter);
                        }

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        command.Prepare();
                        command.ExecuteNonQuery();
                        //Task.Run(async () => { result = await command.ExecuteNonQueryAsync(); }).Wait();

                        transaction.Commit();
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                transaction?.Rollback();
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryTransaction - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryTransaction - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.Close();
            }

            return result;
        }

        /// <summary>
        /// 단일 쿼리 실행 메서드 - 결과를 받아올 필요가 없을 때 (UPDATE, INSERT, DELETE...)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="query">실행할 쿼리</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">쿼리 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static int ExecuteQuery(eDBType type, string query, SqlParameter parameter = null, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            int result = 0;
            try
            {
                var connected = Connect(type, out connection);
                if (connected)
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = timeout;

                        if (parameter != null)
                            command.Parameters.Add(parameter);

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        command.Prepare();
                        result = command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteSingleQuery - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteSingleQuery - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.Close();
            }

            return result;
        }

        /// <summary>
        /// 단일 쿼리 실행 메서드 - 결과를 받아올 필요가 없을 때 (UPDATE, INSERT, DELETE...)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="query">실행할 쿼리</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">쿼리 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static int ExecuteQuery(eDBType type, string query, IEnumerable<SqlParameter> parameters = null, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            int result = 0;
            try
            {
                var connected = Connect(type, out connection);
                if (connected)
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = timeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                                command.Parameters.Add(parameter);
                        }

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        command.Prepare();
                        result = command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteSingleQuery - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteSingleQuery - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.Close();
            }

            return result;
        }

        /// <summary>
        /// 단일 쿼리 실행 메서드 - 결과를 받아올 필요가 있을 때 (SELECT)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="query">실행할 쿼리</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">쿼리 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static int ExecuteQuery(eDBType type, string query, out DataTable records, SqlParameter parameter = null, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            records = null;
            int result = 0;
            try
            {
                var connected = Connect(type, out connection);
                if (connected)
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = timeout;

                        if (parameter != null)
                            command.Parameters.Add(parameter);

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            records = dt;
                        }

                        command.Prepare();
                        result = command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteQuery - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteQuery - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.Close();
            }

            return result;
        }

        /// <summary>
        /// 단일 쿼리 실행 메서드 - 결과를 받아올 필요가 있을 때 (SELECT)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="query">실행할 쿼리</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">쿼리 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static int ExecuteQuery(eDBType type, string query, out DataTable records, IEnumerable<SqlParameter> parameters = null, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            records = null; 
            int result = 0;
            try
            {
                var connected = Connect(type, out connection);
                if (connected)
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = timeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                                command.Parameters.Add(parameter);
                        }

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        using(var adapter = new SqlDataAdapter(command))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            records = dt;
                        }

                        command.Prepare();
                        result = command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteQuery - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteQuery - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.Close();
            }

            return result;
        }

        public static Tuple<bool, DataTable> ExecuteQueryReal(eDBType type, string query, IEnumerable<SqlParameter> parameters = null, SqlParameter outValue = null, int timeout = 30)
        {
            try
            {
                using(var connection = dbOpenSync(type))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = timeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                                command.Parameters.Add(parameter);
                        }

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        try
                        {
                            using (var adapter = new SqlDataAdapter(command))
                            {
                                DataTable dt = new DataTable();
                                adapter.Fill(dt);

                                command.Prepare();
                                command.ExecuteNonQuery();

                                return new Tuple<bool, DataTable>(true, dt);
                            }
                        }
                        catch (SqlException){throw;}
                        catch (Exception){throw;}
                        finally
                        {
                            if (command.Parameters.Count > 0)
                                command.Parameters.Clear();
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteQuery - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteQuery - {ex.Message} - {ex.StackTrace}");
            }

            return new Tuple<bool, DataTable>(false, null);
        }
        #endregion

        #region "query (async)"
        /// <summary>
        /// 단일 쿼리 실행 비동기 메서드(트랜잭션처리) - 결과를 받아올 필요가 없을 때 (UPDATE, INSERT, DELETE...)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="query">실행할 쿼리</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">쿼리 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static async Task<Tuple<bool, int>> ExecuteQueryTrNoReturnAsync(eDBType type, string query, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;
            int result = 0;
            try
            {
                var connected = await ConnectAsync(type, connection);
                if (connected)
                {
                    using (transaction = connection.BeginTransaction())
                    using (var command = new SqlCommand(query, connection, transaction))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = timeout;

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        await command.PrepareAsync();
                        result = await command.ExecuteNonQueryAsync();

                        transaction.Commit();

                        return new Tuple<bool, int>(true, result);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                transaction?.Rollback();
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryTrNoReturnAsync - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryTrNoReturnAsync - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.CloseAsync();
            }

            return new Tuple<bool, int>(false, result);
        }

        /// <summary>
        /// 단일 쿼리 실행 비동기 메서드(트랜잭션처리) - 결과를 받아올 필요가 없을 때 (UPDATE, INSERT, DELETE...)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="query">실행할 쿼리</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">쿼리 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static async Task<Tuple<bool, int>> ExecuteQueryTrNoReturnAsync(eDBType type, string query, SqlParameter parameter, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;
            int result = 0;
            try
            {
                var connected = await ConnectAsync(type, connection);
                if (connected)
                {
                    using (transaction = connection.BeginTransaction())
                    using (var command = new SqlCommand(query, connection, transaction))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = timeout;

                        if (parameter != null)
                            command.Parameters.Add(parameter);

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        await command.PrepareAsync();
                        result = await command.ExecuteNonQueryAsync();

                        transaction.Commit();

                        return new Tuple<bool, int>(true, result);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                transaction?.Rollback();
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryTrNoReturnAsync - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryTrNoReturnAsync - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.CloseAsync();
            }

            return new Tuple<bool, int>(false, result);
        }

        /// <summary>
        /// 단일 쿼리 실행 비동기 메서드(트랜잭션처리) - 결과를 받아올 필요가 없을 때 (UPDATE, INSERT, DELETE...)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="query">실행할 쿼리</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">쿼리 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static async Task<Tuple<bool, int>> ExecuteQueryTrNoReturnAsync(eDBType type, string query, IEnumerable<SqlParameter> parameters, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;
            int result = 0;
            try
            {
                var connected = await ConnectAsync(type, connection);
                if (connected)
                {
                    using (transaction = connection.BeginTransaction())
                    using (var command = new SqlCommand(query, connection, transaction))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = timeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                                command.Parameters.Add(parameter);
                        }

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        await command.PrepareAsync();
                        result = await command.ExecuteNonQueryAsync();

                        transaction.Commit();

                        return new Tuple<bool, int>(true, result);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                transaction?.Rollback();
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryTrNoReturnAsync - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryTrNoReturnAsync - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.CloseAsync();
            }

            return new Tuple<bool, int>(false, result);
        }

        /// <summary>
        /// 단일 쿼리 실행 비동기 메서드 - 결과를 받아올 필요가 없을 때 (UPDATE, INSERT, DELETE...)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="query">실행할 쿼리</param>
        /// <param name="outValue">쿼리 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static async Task<Tuple<bool, int>> ExecuteQueryNoReturnAsync(eDBType type, string query, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            int result = 0;
            try
            {
                var connected = await ConnectAsync(type, connection);
                if (connected)
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = timeout;

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        await command.PrepareAsync();
                        result = await command.ExecuteNonQueryAsync();

                        return new Tuple<bool, int>(true, result);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryNoReturnAsync - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryNoReturnAsync  - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.CloseAsync();
            }

            return new Tuple<bool, int>(false, result);
        }

        /// <summary>
        /// 단일 쿼리 실행 비동기 메서드 - 결과를 받아올 필요가 없을 때 (UPDATE, INSERT, DELETE...)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="query">실행할 쿼리</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">쿼리 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static async Task<Tuple<bool, int>> ExecuteQueryNoReturnAsync(eDBType type, string query, SqlParameter parameter, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            int result = 0;
            try
            {
                var connected = await ConnectAsync(type, connection);
                if (connected)
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = timeout;

                        if (parameter != null)
                            command.Parameters.Add(parameter);

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        await command.PrepareAsync();
                        result = await command.ExecuteNonQueryAsync();

                        return new Tuple<bool, int>(true, result);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryNoReturnAsync - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryNoReturnAsync  - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.CloseAsync();
            }

            return new Tuple<bool, int>(false, result);
        }

        /// <summary>
        /// 단일 쿼리 실행 비동기 메서드 - 결과를 받아올 필요가 없을 때 (UPDATE, INSERT, DELETE...)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="query">실행할 쿼리</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">쿼리 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static async Task<Tuple<bool, int>> ExecuteQueryNoReturnAsync(eDBType type, string query, IEnumerable<SqlParameter> parameters, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            int result = 0;
            try
            {
                var connected = await ConnectAsync(type, connection);
                if (connected)
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = timeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                                command.Parameters.Add(parameter);
                        }

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        await command.PrepareAsync();
                        result = await command.ExecuteNonQueryAsync();

                        return new Tuple<bool, int>(true, result);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryNoReturnAsync - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryNoReturnAsync - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.CloseAsync();
            }

            return new Tuple<bool, int>(false, result);
        }
        
        /// <summary>
        /// 단일 쿼리 실행 비동기 메서드 - 결과를 받아올 필요가 있을 때 (SELECT)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="query">실행할 쿼리</param>
        /// <param name="outValue">쿼리 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static async Task<Tuple<bool, DataTable>> ExecuteQueryReturnAsync(eDBType type, string query, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            try
            {
                var connected = await ConnectAsync(type, connection);
                if (connected)
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = timeout;

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            await command.PrepareAsync();
                            await command.ExecuteNonQueryAsync();

                            return new Tuple<bool, DataTable>(true, dt);
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in Simple.ExecuteQueryAsync - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryAsync - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.Close();
            }

            return new Tuple<bool, DataTable>(false, null);
        }

        /// <summary>
        /// 단일 쿼리 실행 비동기 메서드 - 결과를 받아올 필요가 있을 때 (SELECT)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="query">실행할 쿼리</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">쿼리 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static async Task<Tuple<bool, DataTable>> ExecuteQueryReturnAsync(eDBType type, string query, SqlParameter parameter, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            try
            {
                var connected = await ConnectAsync(type, connection);
                if (connected)
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = timeout;

                        if (parameter != null)
                            command.Parameters.Add(parameter);

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            await command.PrepareAsync();
                            await command.ExecuteNonQueryAsync();

                            return new Tuple<bool, DataTable>(true, dt);
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in Simple.ExecuteQueryAsync - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryAsync - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.Close();
            }

            return new Tuple<bool, DataTable>(false, null);
        }

        /// <summary>
        /// 단일 쿼리 실행 비동기 메서드 - 결과를 받아올 필요가 있을 때 (SELECT)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="query">실행할 쿼리</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">쿼리 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static async Task<Tuple<bool, DataTable>> ExecuteQueryReturnAsync(eDBType type, string query, IEnumerable<SqlParameter> parameters, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            try
            {
                var connected = await ConnectAsync(type, connection);
                if (connected)
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = timeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                                command.Parameters.Add(parameter);
                        }

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            await command.PrepareAsync();
                            await command.ExecuteNonQueryAsync();

                            return new Tuple<bool, DataTable>(true, dt);
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryAsync - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryAsync - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.CloseAsync();
            }

            return new Tuple<bool, DataTable>(false, null);
        }

        /// <summary>
        /// 단일 쿼리 실행 비동기 메서드 - 결과를 받아올 필요가 있을 때 (SELECT)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="query">실행할 쿼리</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">쿼리 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static async Task<Tuple<bool, DataTable>> ExecuteQueryReturnAsyncReal(eDBType type, string query, IEnumerable<SqlParameter> parameters, SqlParameter outValue = null, int timeout = 30)
        {
            try
            {
                using (var connection = await dbOpenAsync(type))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = timeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                                command.Parameters.Add(parameter);
                        }

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        try
                        {
                            using (var adapter = new SqlDataAdapter(command))
                            {
                                DataTable dt = new DataTable();
                                adapter.Fill(dt);

                                await command.PrepareAsync();
                                await command.ExecuteNonQueryAsync();

                                return new Tuple<bool, DataTable>(true, dt);
                            }
                        }
                        catch (SqlException) { throw; }
                        catch (Exception) { throw; }
                        finally
                        {
                            // 해당 작업을 하지 않을 경우 같은 Parameters Collection 내용을 중복사용하게 되면 익셉션이 발생한다
                            // SqlCommand의 Parameters Collection 내용을 Dispose 되기전에 미리 삭제하고 해당 객체를 폐기해야한다
                            if (command.Parameters.Count > 0)
                                command.Parameters.Clear();
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryAsync - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteQueryAsync - {ex.Message} - {ex.StackTrace}");
            }

            return new Tuple<bool, DataTable>(false, null);
        }
        #endregion

        #region "Stored Procedure (sync)"
        /// <summary>
        /// 프로시저 실행관련 메서드 - 결과 레코드를 받아올 필요가 없을 때 (UPDATE, INSERT, DELETE...)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="spName">실행할 프로시저 이름</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">프로시저 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static int ExecuteStoredProcedure(eDBType type, string spName, SqlParameter parameter = null, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            int result = 0;
            try
            {
                var connected = Connect(type, out connection);
                if (connected)
                {
                    using (var command = new SqlCommand(spName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = timeout;

                        if (parameter != null)
                            command.Parameters.Add(parameter);

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        result = command.ExecuteNonQuery();
                    }
                }

            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedure - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedure - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.Close();
            }

            return result;
        }

        /// <summary>
        /// 프로시저 실행관련 메서드 - 결과 레코드를 받아올 필요가 없을 때 (UPDATE, INSERT, DELETE...)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="spName">실행할 프로시저 이름</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">프로시저 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static int ExecuteStoredProcedure(eDBType type, string spName, IEnumerable<SqlParameter> parameters = null, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            int result = 0;
            try
            {
                var connected = Connect(type, out connection);
                if (connected)
                {
                    using(var command = new SqlCommand(spName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = timeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                                command.Parameters.Add(parameter);
                        }

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        result = command.ExecuteNonQuery();
                    }
                }

            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedure - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedure - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.Close();
            }

            return result;
        }

        /// <summary>
        /// 프로시저 실행관련 메서드 - 결과 레코드를 받아와야할 때 (SELECT)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="spName">실행할 프로시저 이름</param>
        /// <param name="records">결과를 받아올 객체</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">프로시저 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static int ExecuteStoredProcedure(eDBType type, string spName, out DataTable records, SqlParameter parameter = null, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            records = null;
            int result = 0;
            try
            {
                var connected = Connect(type, out connection);
                if (connected)
                {
                    using (var command = new SqlCommand(spName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = timeout;

                        if (parameter != null)
                            command.Parameters.Add(parameter);

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            records = dt;
                        }

                        result = command.ExecuteNonQuery();
                        //Task.Run(async () => { result = await command.ExecuteNonQueryAsync(); }).Wait();
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedure - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedure - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.Close();
            }

            return result;
        }

        /// <summary>
        /// 프로시저 실행관련 메서드 - 결과 레코드를 받아와야할 때 (SELECT)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="spName">실행할 프로시저 이름</param>
        /// <param name="records">결과를 받아올 객체</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">프로시저 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static int ExecuteStoredProcedure(eDBType type, string spName, out DataTable records, IEnumerable<SqlParameter> parameters = null, SqlParameter outValue = null, int timeout = 30)  
        {
            SqlConnection connection = null;
            records = null;
            int result = 0;
            try
            {
                var connected = Connect(type, out connection);
                if (connected)
                {
                    using (var command = new SqlCommand(spName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = timeout;

                        if (parameters != null)
                        {
                            foreach(var parameter in parameters)   
                                command.Parameters.Add(parameter);
                        }

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        using(var adapter = new SqlDataAdapter(command))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            records = dt;
                        }

                        result = command.ExecuteNonQuery();
                        //Task.Run(async () => { result = await command.ExecuteNonQueryAsync(); }).Wait();
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedure - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedure - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.Close();
            }

            return result;
        }
        #endregion

        #region "Stored Procedure (async)"
        /// <summary>
        /// 프로시저 실행관련 비동기 메서드 - 결과 레코드를 받아올 필요가 없을 때 (UPDATE, INSERT, DELETE...)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="spName">실행할 프로시저 이름</param>
        /// <param name="outValue">프로시저 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>    
        public static async Task<Tuple<bool, int>> ExecuteSPNoReturnAsync(eDBType type, string spName, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            int result = 0;
            try
            {
                var connected = await ConnectAsync(type, connection);
                if (connected)
                {
                    using (var command = new SqlCommand(spName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = timeout;

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }
                        
                        result = await command.ExecuteNonQueryAsync();
                        return new Tuple<bool, int>(true, result);
                    }
                }

            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedureAsync - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedureAsync - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.CloseAsync();
            }

            return new Tuple<bool, int>(false, result);
        }

        /// <summary>
        /// 프로시저 실행관련 비동기 메서드 - 결과 레코드를 받아올 필요가 없을 때 (UPDATE, INSERT, DELETE...)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="spName">실행할 프로시저 이름</param>
        /// <param name="parameter">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">프로시저 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static async Task<Tuple<bool, int>> ExecuteSPNoReturnAsync(eDBType type, string spName, SqlParameter parameter, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            int result = 0;
            try
            {
                var connected = await ConnectAsync(type, connection);
                if (connected)
                {
                    using (var command = new SqlCommand(spName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = timeout;

                        if (parameter != null)
                            command.Parameters.Add(parameter);

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        result = await command.ExecuteNonQueryAsync();
                        return new Tuple<bool, int>(true, result);
                    }
                }

            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedureAsync - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedureAsync - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.CloseAsync();
            }

            return new Tuple<bool, int>(false, result);
        }

        /// <summary>
        /// 프로시저 실행관련 비동기 메서드 - 결과 레코드를 받아올 필요가 없을 때 (UPDATE, INSERT, DELETE...)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="spName">실행할 프로시저 이름</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">프로시저 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static async Task<Tuple<bool, int>> ExecuteSPNoReturnAsync(eDBType type, string spName, IEnumerable<SqlParameter> parameters, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            int result = 0;
            try
            {
                var connected = await ConnectAsync(type, connection);
                if (connected)
                {
                    using (var command = new SqlCommand(spName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = timeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                                command.Parameters.Add(parameter);
                        }

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        result = await command.ExecuteNonQueryAsync();
                        return new Tuple<bool, int>(true, result);
                    }
                }

            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedureAsync - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedureAsync - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.CloseAsync();
            }

            return new Tuple<bool, int>(false, result);
        }

        /// <summary>
        /// 프로시저 실행관련 비동기 메서드 - 결과 레코드를 받아와야할 때 (SELECT)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="spName">실행할 프로시저 이름</param>
        /// <param name="outValue">프로시저 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static async Task<Tuple<bool, DataTable>> ExecuteSPReturnAsync(eDBType type, string spName, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            try
            {
                var connected = await ConnectAsync(type, connection);
                if (connected)
                {
                    using (var command = new SqlCommand(spName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = timeout;

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            await command.ExecuteNonQueryAsync();

                            return new Tuple<bool, DataTable>(true, dt);
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedureAsync - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedureAsync - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.CloseAsync();
            }

            return new Tuple<bool, DataTable>(false, null);
        }

        /// <summary>
        /// 프로시저 실행관련 비동기 메서드 - 결과 레코드를 받아와야할 때 (SELECT)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="spName">실행할 프로시저 이름</param>
        /// <param name="records">결과를 받아올 객체</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">프로시저 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static async Task<Tuple<bool, DataTable>> ExecuteSPReturnAsync(eDBType type, string spName, SqlParameter parameter, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            try
            {
                var connected = await ConnectAsync(type, connection);
                if (connected)
                {
                    using (var command = new SqlCommand(spName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = timeout;

                        if (parameter != null)
                            command.Parameters.Add(parameter);

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            await command.ExecuteNonQueryAsync();

                            return new Tuple<bool, DataTable>(true, dt);
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedureAsync - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedureAsync - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.CloseAsync();
            }

            return new Tuple<bool, DataTable>(false, null);
        }

        /// <summary>
        /// 프로시저 실행관련 비동기 메서드 - 결과 레코드를 받아와야할 때 (SELECT)
        /// </summary>
        /// <param name="type">프로시저를 실행할 DB 이름</param>
        /// <param name="spName">실행할 프로시저 이름</param>
        /// <param name="records">결과를 받아올 객체</param>
        /// <param name="parameters">프로시저에 사용될 파라미터</param>
        /// <param name="outValue">프로시저 Output</param>
        /// <param name="timeout">명령어 실행제한 시간</param>
        /// <returns></returns>
        public static async Task<Tuple<bool, DataTable>> ExecuteSPReturnAsync(eDBType type, string spName, IEnumerable<SqlParameter> parameters, SqlParameter outValue = null, int timeout = 30)
        {
            SqlConnection connection = null;
            try
            {
                var connected = await ConnectAsync(type, connection);
                if (connected)
                {
                    using (var command = new SqlCommand(spName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = timeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                                command.Parameters.Add(parameter);
                        }

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            await command.ExecuteNonQueryAsync();

                            return new Tuple<bool, DataTable>(true, dt);
                        }
                    }
                }

            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedureAsync - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedureAsync - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                connection?.CloseAsync();
            }

            return new Tuple<bool, DataTable>(false, null);
        }

        public static async Task<Tuple<bool, DataTable>> ExecuteSPReturnAsyncReal(eDBType type, string spName, IEnumerable<SqlParameter> parameters = null, SqlParameter outValue = null, int timeout = 30)
        {
            try
            {
                using (var connection = await dbOpenAsync(type))
                {                   
                    using (var command = new SqlCommand(spName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = timeout;

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                                command.Parameters.Add(parameter);
                        }

                        if (outValue != null)
                        {
                            outValue.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outValue);
                        }

                        try
                        {
                            using (var adapter = new SqlDataAdapter(command))
                            {
                                DataTable dt = new DataTable();
                                adapter.Fill(dt);

                                await command.ExecuteNonQueryAsync();
                                return new Tuple<bool, DataTable>(true, dt);
                            }
                        }
                        catch (SqlException) { throw; }
                        catch (Exception) { throw; }
                        finally
                        {
                            if (command.Parameters.Count > 0)
                                command.Parameters.Clear();
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedureAsync - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SimpleDB.ExecuteStoredProcedureAsync - {ex.Message} - {ex.StackTrace}");
            }

            return new Tuple<bool, DataTable>(false, null);
        }
        #endregion
    }

}
