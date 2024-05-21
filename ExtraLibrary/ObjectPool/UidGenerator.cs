using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class UUidGenerator
    {
        public static string GetUUid()
        {
            return Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// 콘텐츠 타입별 UID 제너레이터
    /// </summary>
    public class AUidGenerator
    {
        public enum eGeneratorType : int
        {
            none = 0,
            session = 1,
            max = 2
        }

        private string m_basekey = "";
        private long m_uid = 0;

        public AUidGenerator() { }

        public bool Initialize(int server_gid, int server_index, eGeneratorType type)
        {
            bool init_result = false;

            // base_key : server_gid + server_index + 생성시간(유닉스타임)
            m_basekey = string.Format($"{server_gid}{server_index}{(int)type}{DateTime.UtcNow.ToUnixTime()}000000");

            init_result = long.TryParse(m_basekey, out var basekey_conv);
            if (init_result) 
            {
                m_uid = basekey_conv;
                return true;
            }

            return false;
        }

        public long Get()
        {
            return Interlocked.Increment(ref m_uid);
        }

    }
}
