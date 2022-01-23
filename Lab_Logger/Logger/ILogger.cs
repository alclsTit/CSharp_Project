﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Lab_Logger.Logger
{
    /// <summary>
    /// Factory Method 패턴에서 사용할 객체 생성 추상클래스
    /// 해당 포맷을 기본으로 제공하는 로거 클래스(콘솔, 텍스트, DB 등...) 구현
    /// </summary>
    public abstract class CLogger
    {
        public string name { get; private set; }
        protected ILog ILogger { get; private set; }

        public bool IsDebugEnabled => ILogger.IsDebugEnabled;

        public bool IsInfoEnabled => ILogger.IsInfoEnabled;

        public bool IsWarnEnabled => ILogger.IsWarnEnabled;

        public bool IsErrorEnabled => ILogger.IsErrorEnabled;

        public bool IsFatalEnabled => ILogger.IsFatalEnabled;

        protected CLogger(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            this.name = name;
            ILogger = log4net.LogManager.GetLogger(name);
        }

        public virtual void Debug(object message) { ILogger.Debug(message); }

        public virtual void Debug(object message, Exception exception) { ILogger.Debug(message, exception); }

        public virtual void DebugFormat(string format, params object[] args) { ILogger.DebugFormat(format, args); }

        public virtual void DebugFormat(string format, object arg0) { ILogger.DebugFormat(format, arg0); }

        public virtual void DebugFormat(string format, object arg0, object arg1) { ILogger.DebugFormat(format, arg0, arg1); }

        public virtual void DebugFormat(string format, object arg0, object arg1, object arg2) { ILogger.DebugFormat(format, arg0, arg1, arg2); }

        public virtual void DebugFormat(IFormatProvider provider, string format, params object[] args) { ILogger.DebugFormat(provider, format, args); }

        public virtual void Info(object message) { ILogger.Info(message); }

        public virtual void Info(object message, Exception exception) { ILogger.Info(message, exception); }

        public virtual void InfoFormat(string format, params object[] args) { ILogger.InfoFormat(format, args); }

        public virtual void InfoFormat(string format, object arg0) { ILogger.InfoFormat(format, arg0); }

        public virtual void InfoFormat(string format, object arg0, object arg1) { ILogger.InfoFormat(format, arg0, arg1); }

        public virtual void InfoFormat(string format, object arg0, object arg1, object arg2) { ILogger.InfoFormat(format, arg0, arg1, arg2); }

        public virtual void InfoFormat(IFormatProvider provider, string format, params object[] args) { ILogger.InfoFormat(provider, format, args); }

        public virtual void Warn(object message) { ILogger.Warn(message); }

        public virtual void Warn(object message, Exception exception) { ILogger.Warn(message, exception); }

        public virtual void WarnFormat(string format, params object[] args) { ILogger.WarnFormat(format, args); }

        public virtual void WarnFormat(string format, object arg0) { ILogger.WarnFormat(format, arg0); }

        public virtual void WarnFormat(string format, object arg0, object arg1) { ILogger.WarnFormat(format, arg0, arg1); }

        public virtual void WarnFormat(string format, object arg0, object arg1, object arg2) { ILogger.WarnFormat(format, arg0, arg1, arg2); }

        public virtual void WarnFormat(IFormatProvider provider, string format, params object[] args) { ILogger.WarnFormat(provider, format, args); }

        public virtual void Error(object message) { ILogger.Error(message); }

        public virtual void Error(object message, Exception exception) { ILogger.Error(message, exception); }

        public virtual void ErrorFormat(string format, params object[] args) { ILogger.ErrorFormat(format, args); }

        public virtual void ErrorFormat(string format, object arg0) { ILogger.ErrorFormat(format, arg0); }

        public virtual void ErrorFormat(string format, object arg0, object arg1) { ILogger.ErrorFormat(format, arg0, arg1); }

        public virtual void ErrorFormat(string format, object arg0, object arg1, object arg2) { ILogger.ErrorFormat(format, arg0, arg1, arg2); }

        public virtual void ErrorFormat(IFormatProvider provider, string format, params object[] args) { ILogger.ErrorFormat(provider, format, args); }

        public virtual void Fatal(object message) { ILogger.Fatal(message); }

        public virtual void Fatal(object message, Exception exception) { ILogger.Fatal(message, exception); }

        public virtual void FatalFormat(string format, params object[] args) { ILogger.FatalFormat(format, args); }

        public virtual void FatalFormat(string format, object arg0) { ILogger.FatalFormat(format, arg0); }

        public virtual void FatalFormat(string format, object arg0, object arg1) { ILogger.FatalFormat(format, arg0, arg1); }

        public virtual void FatalFormat(string format, object arg0, object arg1, object arg2) { ILogger.FatalFormat(format, arg0, arg1, arg2); }

        public virtual void FatalFormat(IFormatProvider provider, string format, params object[] args) { ILogger.FatalFormat(provider, format, args); }
    }
}


/*public abstract class CLogger : ILog
 {
     protected string IName { get; set; }

     public log4net.Core.ILogger Logger { get; }

     public bool IsDebugEnabled { get; }
     public bool IsInfoEnabled { get; }
     public bool IsWarnEnabled { get; }
     public bool IsErrorEnabled { get; }
     public bool IsFatalEnabled { get; }

     public void Debug(object message) { Debug(message); }

     public void Debug(object message, Exception exception) { Debug(message, exception); }

     public void DebugFormat(string format, params object[] args) { DebugFormat(format, args); }

     public void DebugFormat(string format, object arg0) { DebugFormat(format, arg0); }

     public void DebugFormat(string format, object arg0, object arg1) { DebugFormat(format, arg0, arg1); }

     public void DebugFormat(string format, object arg0, object arg1, object arg2) { DebugFormat(format, arg0, arg1, arg2); }

     public void DebugFormat(IFormatProvider provider, string format, params object[] args) { DebugFormat(provider, format, args); }

     public void Info(object message) { Info(message); } 

     public void Info(object message, Exception exception) { Info(message, exception); }

     public void InfoFormat(string format, params object[] args) { InfoFormat(format, args); }

     public void InfoFormat(string format, object arg0) { InfoFormat(format, arg0); }

     public void InfoFormat(string format, object arg0, object arg1) { InfoFormat(format, arg0, arg1); }

     public void InfoFormat(string format, object arg0, object arg1, object arg2) { InfoFormat(format, arg0 , arg1, arg2); } 

     public void InfoFormat(IFormatProvider provider, string format, params object[] args) { InfoFormat(provider, format, args); }

     public void Warn(object message) { Warn(message);}

     public void Warn(object message, Exception exception) { Warn(message, exception); }

     public void WarnFormat(string format, params object[] args) { WarnFormat(format, args); }

     public void WarnFormat(string format, object arg0) { WarnFormat(format, arg0); }

     public void WarnFormat(string format, object arg0, object arg1) { WarnFormat(format, arg0, arg1); }

     public void WarnFormat(string format, object arg0, object arg1, object arg2) { WarnFormat(format, arg0, arg1, arg2); }

     public void WarnFormat(IFormatProvider provider, string format, params object[] args) { WarnFormat(provider, format, args); }

     public void Error(object message) { Error(message); }

     public void Error(object message, Exception exception) { Error(message, exception); }

     public void ErrorFormat(string format, params object[] args) { ErrorFormat(format, args); }

     public void ErrorFormat(string format, object arg0) { ErrorFormat(format, arg0); }

     public void ErrorFormat(string format, object arg0, object arg1) { ErrorFormat(format, arg0, arg1); }

     public void ErrorFormat(string format, object arg0, object arg1, object arg2) { ErrorFormat(format, arg0, arg1, arg2); }

     public void ErrorFormat(IFormatProvider provider, string format, params object[] args) { ErrorFormat(provider, format, args); }

     public void Fatal(object message) { Fatal(message); }

     public void Fatal(object message, Exception exception) { Fatal(message, exception); }

     public void FatalFormat(string format, params object[] args) { FatalFormat(format, args); }

     public void FatalFormat(string format, object arg0) { FatalFormat(format, arg0); }

     public void FatalFormat(string format, object arg0, object arg1) { FatalFormat(format, arg0, arg1); }

     public void FatalFormat(string format, object arg0, object arg1, object arg2) { FatalFormat(format, arg0, arg1, arg2); }

     public void FatalFormat(IFormatProvider provider, string format, params object[] args) { FatalFormat(provider, format, args); }
 }
 */


