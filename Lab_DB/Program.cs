using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Lab_DB
{ 
    internal static class DBPerformanceCheck
    {
        public static async Task RunTest(int clientCount, int dbCallTime)
        {
            var sw = new Stopwatch();

            Console.WriteLine($"Testing {clientCount} clients operated!!!");

            var onOff = 1;

            switch (onOff)
            {
                case 0:
                    sw.Restart();
                    {
                        Task.WaitAll(
                            Enumerable.Range(0, clientCount).AsParallel().Select(item => Task.Run(() => ExecuteSyncTest(item, dbCallTime))).ToArray());

                        Console.WriteLine($"-> ExecuteSyncTest returned in {sw.Elapsed}");
                    }

                    sw.Restart();
                    {
                        Task.WaitAll(
                            Enumerable.Range(0, clientCount).AsParallel().Select(_ => ExecuteAsyncTest(dbCallTime)).ToArray());

                        Console.WriteLine($"-> ExecuteAsyncTest returned in {sw.Elapsed}");
                    }
                    break;

                case 1:
                    /*sw.Restart();
                    {
                        var aTasks = Enumerable.Range(0, clientCount).Select(_ => ExecuteWrapperASync());
                        await Task.WhenAll(aTasks);
                        Console.WriteLine($"-> ExecuteWrapperAsync returned in {sw.Elapsed}");
                    }*/

                    /*sw.Restart();
                    {
                        var aTasks = Enumerable.Range(0, clientCount).Select(_ => ExecuteAsync());
                        await Task.WhenAll(aTasks);
                        int count = 0;
                        foreach(var item in aTasks.ToList())
                        {
                            ++count;
                            if (item.IsCompletedSuccessfully)
                                Console.WriteLine($"{item.Result}");
                        }
                        Console.WriteLine($"-> ExecuteNativeAsync returned in {sw.Elapsed} // Jobs = {count}");
                    }*/

                    sw.Restart();
                    {
                        var aTasks = Enumerable.Range(0, clientCount).Select(_ => ExecuteSPAsync());
                        await Task.WhenAll(aTasks);
                        Console.WriteLine($"-> ExecuteNativeStoredProcedureAsync returned in {sw.Elapsed}");
                    }
                    break;

                case 2:
                    sw.Restart();
                    {
                        bool flag = false;
                        Parallel.ForEach(Enumerable.Range(0, clientCount), async (num) => {
                            flag = await ExecuteSPAsync();
                            Console.WriteLine($"Job Done = {flag}");
                        });
                        Console.WriteLine($"-> Parallel ExecuteSPAsync returned in {sw.Elapsed} // Jobs = {flag}");
                    }             
                    break;

                case 3:
                    sw.Restart();
                    {
                        List<Task<bool>> enumList = new List<Task<bool>>();
                        Parallel.ForEach(Enumerable.Range(0, 8),
                            async (item) => {
                                enumList = Enumerable.Range(0, clientCount).Select(_ => ExecuteAsync()).ToList();
                                await Task.WhenAll(enumList);                          
                            });

                        int count = 0;
                        foreach(var item in enumList)
                        {
                            ++count;
                            if (item.IsCompletedSuccessfully)
                                Console.WriteLine($"{item.Result}");
                        }
                        Console.WriteLine($"-> Parallel ExecuteSyncQuery returned in {sw.Elapsed} // Jobs = {count}");
                    }
                    break;

                case 4:
                    // bulk insert Test
                    sw.Restart();
                    {
                        await ExecuteDBBulkInsert();
                    }
                    break;

                default:
                    break;
            }

            Console.WriteLine("\n\n");
        }

        private static async Task ExecuteDBBulkInsert()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("serial", typeof(int));
            dataTable.Columns.Add("uid", typeof(string));
            dataTable.Columns.Add("name", typeof(string)); 

            DataRow dtRows = dataTable.NewRow();
            dtRows["uid"] = "gusdl576@naver.com";
            dtRows["name"] = "최규화";

            dataTable.Rows.Add(dtRows); 
            await SimpleDB.DoBulkInsertTransaction(eDBType.LAB_GAME01, dataTable, "dbo.tbl_user_account");
        }

        private static void ExecuteSyncTest(int num, int dbCallTime)
        {
            //Console.WriteLine($"[{num}] This is Thread num = {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(dbCallTime);
            //Console.WriteLine($"[{num}] This is Thread num = {Thread.CurrentThread.ManagedThreadId}");
        }

        private static async Task ExecuteAsyncTest(int dbCallTime)
        {
            await Task.Delay(dbCallTime);
        }

        private static Task<bool> ExecuteWrapperASync()
        {
            var query = "SELECT * FROM dbo.tbl_dummy WHERE name = @name";
            SqlParameter myParam = new SqlParameter("@name", SqlDbType.NVarChar, 10);
            myParam.Value = "sudo9";

            var myParams = new List<SqlParameter>(1);
            myParams.Add(myParam);

            return Task.Run(() =>
            {
                var result = SimpleDB.ExecuteQueryReal(eDBType.LAB_GAME01, query, myParams);
                return result.Item1;
            });
        }

        private static async Task<bool> ExecuteAsync()
        {
            var query = "SELECT * FROM dbo.tbl_dummy WHERE name = @name";
            SqlParameter myParam = new SqlParameter("@name", SqlDbType.NVarChar, 10);
            myParam.Value = "sudo9";

            var myParams = new List<SqlParameter>(1);
            myParams.Add(myParam);

            var result = await SimpleDB.ExecuteQueryReturnAsyncReal(eDBType.LAB_GAME01, query, myParams);
            return result.Item1;
        }

        private static async Task<bool> ExecuteSPAsync()
        {
            SqlParameter myParam = new SqlParameter("@name", SqlDbType.NVarChar, 10);
            myParam.Value = "sudo9";

            SqlParameter myParamOutput = new SqlParameter(@"outvalue", SqlDbType.Int);
            //myParamOutput.Value = 1;

            var myParams = new List<SqlParameter>(1);
            myParams.Add(myParam);

            var result = await SimpleDB.ExecuteSPReturnAsyncReal(eDBType.LAB_GAME01, "osp_dummy_select", myParams, myParamOutput);
            if (result.success)
            {
                Console.WriteLine($"Result = {result.success}, Output = {result.output}");
                for(var idx = 0; idx < result.selectTable.Rows.Count; ++idx)
                    Console.WriteLine($"Data = {result.selectTable.Rows[idx][0]} - {result.selectTable.Rows[idx][1]} - {result.selectTable.Rows[idx][2]}");
            }

            return result.success;
        }

        private static void PerformanceLoop(int count)
        {
            var query = "SELECT * FROM dbo.tbl_dummy WHERE name = @name";
            SqlParameter myParam = new SqlParameter("@name", SqlDbType.NVarChar, 10);
            myParam.Value = "sudo9";

            var myParams = new List<SqlParameter>(1);
            myParams.Add(myParam);

            var totalCount = count;
            while(totalCount > 0)
            {
                try
                {
                    var result = SimpleDB.ExecuteQueryReal(eDBType.LAB_GAME01, query, myParams);
                    if (!result.Item1)
                        break;
                    --totalCount;

                    if (totalCount % 1000 == 0)
                        Console.WriteLine($"Left Count = {totalCount}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in DBPerformanceCheck.PerformanceLoop - {ex.Message} - {ex.StackTrace}");
                }
            }

            Console.WriteLine($"Job Completed[sync] = {totalCount}/{count}");
        }

        public static void PerformanceCheck(int loopCount)
        {
            var checker = new Stopwatch();
            checker.Start();
            PerformanceLoop(loopCount);
            checker.Stop();
            Console.WriteLine($"TimeElasped: {checker.Elapsed.TotalSeconds}");
        }


        private static async Task PerformanceLoopAsync(int count)
        {
            var query = "SELECT * FROM dbo.tbl_dummy WHERE name = @name";
            SqlParameter myParam = new SqlParameter("@name", SqlDbType.NVarChar, 10);
            myParam.Value = "sudo9";

            var myParams = new List<SqlParameter>(1);
            myParams.Add(myParam);

            var totalCount = count;
            while(totalCount > 0)
            {
                var result = await SimpleDB.ExecuteQueryReturnAsyncReal(eDBType.LAB_GAME01, query, myParams);
                //if (!result.Item1)
                //    break;
                --totalCount;
            }

            Console.WriteLine($"Job Completed[async] = {totalCount}/{count}");
        }

        public static async Task PerformanceCheckAsync(int loopCount)
        {
            var checker = new Stopwatch();
            checker.Start();
            await PerformanceLoopAsync(loopCount);
            checker.Stop();
            Console.WriteLine($"TimeElasped: {checker.Elapsed.TotalSeconds}");        
        }

        private static async Task PerformanceLoopSPAsync(int count)
        {
            SqlParameter myParam = new SqlParameter("@name", SqlDbType.NVarChar, 10);
            myParam.Value = "sudo9";

            var myParams = new List<SqlParameter>(1);
            myParams.Add(myParam);

            var totalCount = count;
            while (totalCount > 0)
            {
                var result = await SimpleDB.ExecuteSPReturnAsyncReal(eDBType.LAB_GAME01, "osp_dummy_select", myParams);
                //if (!result.Item1)
                //    break;
                --totalCount;
            }

            Console.WriteLine($"Job Completed[async] = {totalCount}/{count}");
        }

        public static async Task PerformanceCheckSPAsync(int loopCount)
        {
            var checker = new Stopwatch();
            checker.Start();
            await PerformanceLoopSPAsync(loopCount);
            checker.Stop();
            Console.WriteLine($"TimeElasped: {checker.Elapsed.TotalSeconds}");
        }
    }


    internal class Program
    {
        static async Task Main(string[] args)
        {
            DBConfigList.Instance.TryAdd(eDBType.LAB_GAME01, new DBConfig(eDBType.LAB_GAME01, "192.168.0.12", "1433", "alclsTit", "c975813"));
            DBConfigList.Instance.TryAdd(eDBType.LAB_GAME02, new DBConfig(eDBType.LAB_GAME02, "192.168.0.12", "1433", "alclsTit", "c975813"));

            SimpleDB.Initialize(DBConfigList.Instance.mDBConfigMap);

            var input = 1;
            var dbCallTime = 1000;
            await DBPerformanceCheck.RunTest(input, dbCallTime);

            /*DBPerformanceCheck.PerformanceCheck(input);

            Console.WriteLine("-----------------------------------------------\n\n\n");

            await DBPerformanceCheck.PerformanceCheckAsync(input);

            Console.WriteLine("-----------------------------------------------\n\n\n");

            await DBPerformanceCheck.PerformanceCheckSPAsync(input);

            SqlParameter param = new SqlParameter("@name", SqlDbType.NVarChar, 10);
            param.Value = "sudo3";
       

            var result = SimpleDB.ExecuteStoredProcedure(eDBType.LAB_GAME01, "osp_dummy_select", out var records, param);
            for (var idx = 0; idx < records.Rows.Count; ++idx)
            {
                Console.WriteLine($"Serial = {records.Rows[idx][0]}, name = {records.Rows[idx][1]}, logdate = {records.Rows[idx][2]}");
            }
            

            Tuple<bool, DataTable> dt = await SimpleDB.ExecuteSPReturnAsync(eDBType.LAB_GAME01, "osp_dummy_select", param, null, 30); 
            if (dt.Item1)
            {
                for (var idx = 0; idx < dt.Item2.Rows.Count; ++idx)
                {
                    Console.WriteLine($"Serial = { dt.Item2.Rows[idx][0]}, name = { dt.Item2.Rows[idx][1]}, logdate = { dt.Item2.Rows[idx][2]}");
                }
            }
            

            List<SqlParameter> myParams = new List<SqlParameter>(1);
            myParams.Add(new SqlParameter("@name", SqlDbType.NVarChar, 10));
            myParams[0].Value = "sudo7";
            var dt2 = await SimpleDB.ExecuteSPReturnAsyncReal(eDBType.LAB_GAME01, "osp_dummy_select", myParams);
            if (dt2.Item1)
            {
                for(var idx = 0; idx < dt2.Item2.Rows.Count; ++idx)
                    Console.WriteLine($"Serial = { dt2.Item2.Rows[idx][0]}, name = { dt2.Item2.Rows[idx][1]}, logdate = { dt2.Item2.Rows[idx][2]}");
            }

            /*
            DBManager.Instance.Initialize("LAB_GAME01", "192.168.0.12", "1433", "alclsTit", "c975813");

            SqlParameter param = new SqlParameter("@name", SqlDbType.NVarChar, 10);
            param.Value = "sudo5";

            var result = DBManager.Instance.TryExcuteDatatable("osp_dummy_select", out var resultTable, 30, param );
            for (var idx = 0; idx < resultTable.Rows.Count; ++idx)
            {
                Console.WriteLine($"serial = {resultTable.Rows[idx][0]}, name = {resultTable.Rows[idx][1]}, logdate = {resultTable.Rows[idx][2]}");
            }

            Console.WriteLine("-------------------------------------------------------------------------------------\n\n\n");

            var result2 = DBManager.Instance.TryDoQuerySP("osp_dummy_data_all_select", out var affected, out var resultTable2);
            for(var idx = 0; idx < resultTable2.Rows.Count; ++idx)
            {
                Console.WriteLine($"Serial = {resultTable2.Rows[idx][0]}, name = {resultTable2.Rows[idx][1]}, logdate = {resultTable2.Rows[idx][2]}");
            }
            */
        }
    }
}
