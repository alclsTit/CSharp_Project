using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// 프로젝트에서 공용으로 사용하는 확장기능
    /// </summary>
    public static class ExtensionFunction
    {
        public static int ToUnixTime(this DateTime datetime)
        {
            TimeSpan interval = datetime - DateTime.UnixEpoch;
            return (int)interval.TotalSeconds;
        }
    }
}
