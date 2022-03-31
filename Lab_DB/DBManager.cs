using System.Data;
using System.Data.SqlClient;

namespace Lab_DB
{
    public abstract class DBFrameBase
    {
        public string mConnectionInfo { get; protected set; } = "";
        public SqlConnection mConnection { get; protected set; } = null;

        public bool IsOpen => mConnection != null && mConnection.State != ConnectionState.Closed;

        public bool IsClosed => mConnection == null || mConnection.State == ConnectionState.Closed;

        protected void SetConnectionInfo(string name, string ip, string port, string loginID, string loginPW)
        {
            // SQL Server 인증방식으로 db 연결을 진행하기 위해서 id, pw 를 전달받음
            // 1.Data Source = 연결할 DB 서버 IP, PORT 정보
            // 2.Initial Catalog = 연결할 DB 서버이름 
            // 3.User ID = (SQL Server 인증) 접속 유저 아이디
            // 4.Password = (SQL Server 인증) 접속 유저 비밀번호
            mConnectionInfo = $"Data Source={ip},{port};Initial Catalog={name};User ID={loginID};Password={loginPW}";
        }

        public virtual bool Connect()
        {
            if (string.IsNullOrEmpty(mConnectionInfo))
            {
                Console.WriteLine("DB ConnectionInfo didn't set yet!!!");
                return false;
            }

            mConnection = mConnection ?? new SqlConnection(mConnectionInfo);
            mConnection.Open();

            return true;
        }

        /// <summary>
        /// 프로시저 실행관련 메서드 - 결과를 받아올 필요가 없고 반환 값도 필요없을 때 (UPDATE, INSERT, DELETE)
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="items"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        protected int DoQuerySP(string spName, List<SqlParameter> items = null, int timeout = 30)
        {
            using(var command = new SqlCommand(spName, mConnection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = timeout;

                if (items != null)
                {
                    foreach(var item in items)
                        command.Parameters.Add(item);
                }

                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 프로시저 실행관련 메서드 - 결과를 받아올 필요가 없을 때 (UPDATE, INSERT, DELETE)
        /// </summary>
        /// <param name="spName">실행할 프로시저 이름</param>
        /// <param name="items">프로시저에 전달할 파라미터</param>
        /// <param name="returnValue">프로시저의 실행결과 값</param>
        /// <param name="timeout">명령어 실행 제한시간</param>
        /// <returns></returns>
        protected int DoQuerySP(string spName, List<SqlParameter> items = null, SqlParameter returnValue = null, int timeout = 30)
        {
            using(var command = new SqlCommand(spName, mConnection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = timeout;

                if (items != null)
                {
                    foreach (var item in items)
                        command.Parameters.Add(item);
                }

                if (returnValue != null)
                {
                    returnValue.Direction = ParameterDirection.ReturnValue;
                    command.Parameters.Add(returnValue);
                }

                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 프로시저 실행관련 메서드 - 결과를 받아와야할 때 (SELECT)
        /// </summary>
        /// <param name="spName">실행할 프로시저 이름</param>
        /// <param name="items">프로시저에 전달할 파라미터</param>
        /// <param name="returnValue">프로시저의 실행결과 값</param>
        /// <param name="resultTable">프로시저 실행으로 얻은 레코드 값</param>
        /// <param name="timeout">명령어 실행 제한시간</param>
        /// <returns></returns>
        protected int DoQuerySP(string spName, out DataTable resultTable, List<SqlParameter> items = null, SqlParameter returnValue = null, int timeout = 30)
        {
            using(var command = new SqlCommand(spName, mConnection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = timeout;

                if (items != null)
                {
                    foreach (var item in items)
                        command.Parameters.Add(item);
                }

                if (returnValue != null)
                {
                    returnValue.Direction = ParameterDirection.ReturnValue;
                    command.Parameters.Add(returnValue);
                }

                using(var adapter = new SqlDataAdapter(command))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    resultTable = dt;
                }

                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 단일 쿼리 실행 메서드 
        /// </summary>
        /// <param name="query">실행할 쿼리</param>
        /// <param name="items">쿼리에 전달될 파라미터</param>
        /// <param name="timeout">명령어 실행 제한시간</param>
        /// <returns></returns>
        protected int DoQuery(string query, List<SqlParameter> items = null, int timeout = 30)
        {
            using(var command = new SqlCommand(query, mConnection))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = timeout;

                if (items != null)
                {
                    foreach (var item in items)
                        command.Parameters.Add(item);
                }

                command.Prepare();
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 단일 쿼리 실행 메서드 (트랜잭션 사용) 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected int DoQueryTransaction(string query, List<SqlParameter> items = null, int tiemout = 30)
        {
            using(var tran = mConnection.BeginTransaction())
            {
                using (var command = new SqlCommand(query, mConnection, tran)) 
                {
                    try
                    {
                        if (items != null)
                        {
                            foreach (var item in items)
                                command.Parameters.Add(item);
                        }

                        command.Prepare();
                        var result = command.ExecuteNonQuery();

                        tran.Commit();
                        return result;
                    }
                    catch (SqlException)
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 대량의 데이터 Insert 진행 시 사용 (트랜잭션 사용)
        /// </summary>
        /// <param name="dt">데이터 테이블</param>
        /// <param name="tableName">테이블 이름</param>
        /// <param name="timeout">명령어 실행 제한시간</param>
        protected void DoBulkInsertTransaction(DataTable dt, string tableName, int timeout = 30)
        {
            using(var tran = mConnection.BeginTransaction())
            {
                using(var bulkInsert = new SqlBulkCopy(mConnection))
                {
                    try
                    {
                        var dataRows = dt.Select();
                        bulkInsert.DestinationTableName = tableName;
                        Task.Run(async () => { await bulkInsert.WriteToServerAsync(dataRows); }).Wait();

                        bulkInsert.Close();
                        tran.Commit();
                    }
                    catch (SqlException)
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        protected DataTable ExcuteDatatable(string spName, int timeout = 30, params SqlParameter[] items)
        {
            using(var command = new SqlCommand(spName, mConnection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = timeout;
                using (var adapter = new SqlDataAdapter(command))
                {
                    if (items != null)
                        adapter.SelectCommand.Parameters.AddRange(items);

                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    return dt;
                }
            }
        }

        public virtual void Close()
        {
            if (!IsClosed)
                return;

            mConnection?.Close();
        }
    }

    public class DBFrame : DBFrameBase
    { 
        public void Initialize(string name, string ip, string port)
        {
            SetConnectionInfo(name, ip, port, "sa", "1234");
        }

        public void Initialize(string name, string ip, string port, string loginID, string loginPW)
        {
            SetConnectionInfo(name, ip, port, loginID, loginPW);
        }

        #region "DB StoredProcedure"
        public bool TryDoQuerySP(string spName, out int affected, List<SqlParameter> items = null, int timeout = 30)
        {
            bool dbOpenState = false;
            affected = 0;

            try
            {
                dbOpenState = IsOpen == true ? true : Connect();

                if (dbOpenState)
                    affected = DoQuerySP(spName, items, timeout);
            }
            catch(SqlException sqlEx)
            {
                Console.WriteLine($"Exception in DBFrame.TryDoQuerySP - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DBFrame.TryDoQuerySP - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                if (dbOpenState)
                    Close();
            }

            return dbOpenState;
        }

        public bool TryDoQuerySP(string spName, out int affected, List<SqlParameter> items = null, SqlParameter returnValue = null, int timeout = 30)
        {
            bool dbOpenState = false;
            affected = 0;

            try
            {
                dbOpenState = IsOpen == true ? true : Connect();

                if (dbOpenState)
                    affected = DoQuerySP(spName, items, returnValue, timeout);
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in DBFrame.TryDoQuerySP - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DBFrame.TryDoQuerySP - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                if (dbOpenState)
                    Close();
            }

            return dbOpenState;
        }

        public bool TryDoQuerySP(string spName, out int affected, out DataTable resultTable, List<SqlParameter> items = null, SqlParameter returnValue = null, int timeout = 30)
        {
            bool dbOpenState = false;
            affected = 0;
            resultTable = null;

            try
            {
                dbOpenState = IsOpen == true ? true : Connect();
                
                if (dbOpenState)
                    affected = DoQuerySP(spName, out resultTable, items, returnValue, timeout);
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in DBFrame.TryDoQuerySP - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DBFrame.TryDoQuerySP - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                if (dbOpenState)
                    Close();
            }

            return dbOpenState;
        }
        #endregion

        #region "DB T-SQL"
        public bool TryDoQuery(string query, out int affected, List<SqlParameter> items = null)
        {
            bool dbOpenState = false;
            affected = 0;

            try
            {
                dbOpenState = IsOpen == true ? true : Connect();

                if (dbOpenState)
                    affected = DoQuery(query, items);
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in DBFrame.TryDoQuery - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DBFrame.TryDoQuery - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                if (dbOpenState)
                    Close();
            }

            return dbOpenState;
        }
        #endregion


        public bool TryExcuteDatatable(string spName, out DataTable resultTable, int timeout = 30, params SqlParameter[] items)
        {
            bool dbOpenState = false;
            resultTable = null;

            try
            {
                if (!IsOpen)
                    dbOpenState = Connect();

                if (dbOpenState)
                    resultTable = ExcuteDatatable(spName, timeout, items);
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Exception in DBFrame.TryExcuteDatatable - {sqlEx.Message} - {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DBFrame.TryExcuteDatatable - {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                if (dbOpenState)
                    Close();
            }

            return dbOpenState;
        }
    }

    public class DBManager
    {
        private DBFrame mDBFrame;

        private static readonly DBManager mInstance = new DBManager();
        public static DBManager Instance => mInstance;
        private DBManager() { }

        public void Initialize(string name, string ip, string port)
        {
            mDBFrame = mDBFrame ?? new DBFrame();
            mDBFrame.Initialize(name, ip, port);
        }

        public void Initialize(string name, string ip, string port, string loginID, string loginPW)
        {
            mDBFrame = mDBFrame ?? new DBFrame();
            mDBFrame.Initialize(name, ip, port, loginID, loginPW);
        }

        public bool TryDoQuerySP(string spName, out int affected, List<SqlParameter> items = null, int timeout = 30)
        {
            if (mDBFrame == null)
            {
                affected = 0;
                return false;
            }

            return mDBFrame.TryDoQuerySP(spName, out affected, items, timeout);
        }

        public bool TryDoQuerySP(string spName, out int affected, List<SqlParameter> items = null, SqlParameter returnValue = null, int timeout = 30)
        {
            if (mDBFrame == null)
            {
                affected = 0;
                return false;
            }

            return mDBFrame.TryDoQuerySP(spName, out affected, items, returnValue, timeout);
        }

        public bool TryDoQuerySP(string spName, out int affected, out DataTable resultTable, List<SqlParameter> items = null, SqlParameter returnValue = null, int timeout = 30)
        {
            if (mDBFrame == null)
            {
                affected = 0;
                resultTable = null;
                return false;
            }

            return mDBFrame.TryDoQuerySP(spName, out affected, out resultTable, items, returnValue, timeout);
        }

        public bool TryExcuteDatatable(string spName, out DataTable resultTable, int timeout = 30, params SqlParameter[] items)
        {
            if (mDBFrame == null)
            {
                resultTable = null;
                return false;
            }

            return mDBFrame.TryExcuteDatatable(spName, out resultTable, timeout, items);
        }
    }
}
