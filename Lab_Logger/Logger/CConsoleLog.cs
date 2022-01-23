using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Logger.Logger
{
    /// <summary>
    /// Console 창에 찍히는 로그 관련 클래스
    /// LogFactory 디자인 패턴에의해 생성되는 실제 객체
    /// </summary>
    public class CConsoleLog : CLogger
    {
        public CConsoleLog(string name) : base(name) { }

        public override void Error(object message)
        {
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Red;
            ILogger.Error(message);
            Console.ResetColor();
        }

        public override void Error(object message, Exception exception)
        {
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Red;
            ILogger.Error(message, exception);
            Console.ResetColor();
        }

    }

    /// <summary>
    /// LogFactory 객체 생성 구현 파생클래스 
    /// </summary>
    public class CConsoleLogFactory : ILogFactory
    {
        public CLogger GetLogger()
        {
            return new CConsoleLog(nameof(CConsoleLog));
        }
    }
}
