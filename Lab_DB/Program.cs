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
        private static void PerformanceLoop(int count)
        {
            var query = "SELECT * FROM dbo.tbl_dummy WHERE name = @name";
            SqlParameter myParam = new SqlParameter("@name", SqlDbType.NVarChar, 10);
            myParam.Value = "sudo9";

            var myParams = new List<SqlParameter>(1);
            myParams.Add(myParam);

            var totalCount = count;
            for (var idx = 0; idx < count; ++idx)
            {
                try
                {
                    var result = SimpleDB.ExecuteQueryReal(eDBType.LAB_GAME01, query, myParams);
                    //if (!result.Item1)
                    //    break;
                    --totalCount;
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
            while(true)
            {
                var result = await SimpleDB.ExecuteQueryReturnAsyncReal(eDBType.LAB_GAME01, query, myParams);
                if (!result.Item1)
                    break;
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
    }


    internal class Program
    {
        static async Task Main(string[] args)
        {
            DBConfigList.Instance.TryAdd(eDBType.LAB_GAME01, new DBConfig(eDBType.LAB_GAME01, "192.168.0.12", "1433", "alclsTit", "c975813"));
            DBConfigList.Instance.TryAdd(eDBType.LAB_GAME02, new DBConfig(eDBType.LAB_GAME02, "192.168.0.12", "1433", "alclsTit", "c975813"));

            SimpleDB.Initialize(DBConfigList.Instance.mDBConfigMap);

            DBPerformanceCheck.PerformanceCheck(10000);

            Console.WriteLine("-----------------------------------------------\n\n\n");

            await DBPerformanceCheck.PerformanceCheckAsync(10000);

            /*SqlParameter param = new SqlParameter("@name", SqlDbType.NVarChar, 10);
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
