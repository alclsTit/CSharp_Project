using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Logger.Logger
{
    /// <summary>
    /// Text 로그 관련 클래스
    /// LogFactory 디자인 패턴에의해 생성되는 실제 객체
    /// </summary>
    public sealed class CTextLog : CLogger
    {
        public CTextLog(string name) : base(name) { }

        public override void Debug(object message)
        {
            if (base.IsDebugEnabled)
                base.Debug(message);
        }

        public override void Debug(object message, Exception exception)
        {
            if (base.IsDebugEnabled)
                base.Debug(message, exception);
        }

        public override void Info(object message)
        {
            if (base.IsInfoEnabled)
                base.Info(message);
        }

        public override void Info(object message, Exception exception)
        {
            if (base.IsInfoEnabled)
                base.Info(message, exception);
        }

        public override void Warn(object message)
        {
            if (base.IsWarnEnabled)
                base.Warn(message);
        }

        public override void Warn(object message, Exception exception)
        {
            if (base.IsWarnEnabled)
                base.Warn(message, exception);
        }

        public override void Error(object message)
        {
            if (base.IsErrorEnabled)
                base.Error(message);
        }

        public override void Error(object message, Exception exception)
        {
            if (base.IsErrorEnabled)
                base.Error(message, exception);
        }

        public override void Fatal(object message)
        {
            if (base.IsFatalEnabled)
                base.Fatal(message);
        }

        public override void Fatal(object message, Exception exception)
        {
            if (base.IsFatalEnabled)
                base.Fatal(message, exception);
        }

    }

    /// <summary>
    /// LogFactory 객체 생성 구현 파생클래스 
    /// </summary>
    public class CTextLogFactory : ILogFactory
    {
        public CLogger GetLogger()
        {
            return new CTextLog(nameof(CTextLog));
        }
    }
}
