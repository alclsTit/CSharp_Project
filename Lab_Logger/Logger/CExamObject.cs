using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Lab_Logger.Logger
{
    public class CExamSession
    {
        public ILogFactory mILogFactory { get; private set; }

        public CLogger mLogger { get; private set; }

        public CExamSession(ILogFactory logFactory)
        {
            if (logFactory == null)
                logFactory = new CConsoleLogFactory();

            mILogFactory = logFactory;
            mLogger = this.GetLogger();
        }

        protected virtual CLogger GetLogger()
        {
            return mILogFactory.GetLogger();
        }

        public void DebugLog(string message)
        {
            if (mLogger.IsDebugEnabled)
                mLogger.Debug(message);
        }

        public void InfoLog(string message)
        {
            mLogger.InfoFormat(string.Format("{0} InfoLog - {1} - {2}", nameof(CExamSession), "InfoLog", message), "TEST");
        }

        public void ExceptionLog()
        {
            try
            {
                int parent = 100000;
                var result = parent / 0;
            }
            catch (Exception ex)
            {
                mLogger.Error($"Exception in CExamSession.ExceptionLog - {ex.Message} - {ex.StackTrace}");
            }
        }
    }
}