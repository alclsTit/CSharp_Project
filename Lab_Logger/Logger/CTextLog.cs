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
    public class CTextLog : CLogger
    {
        public CTextLog(string name) : base(name) { }

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
