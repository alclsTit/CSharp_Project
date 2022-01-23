using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lab_Logger.Logger;

namespace Lab_Logger
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 1.Log4Net을 이용한 LogFactory 테스트 
            List<ILogFactory> logFactoryList = new List<ILogFactory>();
         
            logFactoryList.Add(new CConsoleLogFactory());
            logFactoryList.Add(new CTextLogFactory());

            foreach (var obj in logFactoryList)
            {
                var targetObj = obj.GetLogger();
                targetObj.Error($"{targetObj.GetType().Name} is Called!!!");
            }

            // 2.Log4Net을 이용한 LogFactory를 멤버로 가진 클래스 예외처리 테스트 
            CExamSession consoleLogTest = new CExamSession(null);
            consoleLogTest.ExceptionLog();
            consoleLogTest.DebugLog("This is ConsoleLog Test...");

            CExamSession textLogTest = new CExamSession(new CTextLogFactory());
            textLogTest.InfoLog("This is TextLog Test...");
            textLogTest.ExceptionLog();
        }
    }
}
