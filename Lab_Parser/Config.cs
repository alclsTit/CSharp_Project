using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Lab_Parser
{
    // 1. config는 각 프로세스마다 고유하게 존재해야한다 *해당 프로세스의 속성을 외부에서 컨트롤
    internal interface IConfig
    {

    }

    internal struct sSocketConfig
    {
        public readonly int m_recv_buff_size { get; }
        public readonly int m_send_buff_size { get; }
        public readonly int m_recv_timeout { get; }
        public readonly int m_send_timeout { get; }
        public readonly int m_heartbeat_check_time { get; }
        public readonly byte m_heartbeat_count { get; }

        public sSocketConfig(int recv_buff_size, int send_buff_size,
                             int recv_timeout, int send_timeout,
                             int heartbeat_check_time, byte heartbeat_count) 
        {
            m_recv_buff_size = recv_buff_size;
            m_send_buff_size = send_buff_size;
            m_recv_timeout = recv_timeout;
            m_send_timeout = send_timeout;
            m_heartbeat_check_time = heartbeat_check_time;
            m_heartbeat_count = heartbeat_count;
        }
    }

    // 외부 config 로더
    internal class ConfigLoader<T>
    {
        private bool Find(string file_name, out string result_file_path)
        {
            var root_path = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName);
            if (string.IsNullOrEmpty(root_path))
            {
                result_file_path = "";
                return false;
            }

            var data_path = Path.Combine(root_path, "\\", "data");
            var file_path = Path.Combine(data_path, "\\", file_name);

            if (!File.Exists(file_path)) 
            {
                result_file_path = "";
                return false;
            }

            result_file_path = file_path;
            return true;
        }

        public T LoadJson(string file_name)
        {
            if (!Find(file_name, out string result_file_path))
            {
                return default(T);
            }

            var load_target = $"{result_file_path}.json";
        }
    }
}
