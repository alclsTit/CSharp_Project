using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Lab_DB
{
    public enum eDBType
    {
        _MIN_NO_TYPE = 0,
        LAB_GAME01 = 1,
        LAB_GAME02 = 2,
        _MAX_NO_TYPE = 3
    }

    public sealed class DBConfigList
    {
        private static readonly DBConfigList mInstance = new DBConfigList();
        public static DBConfigList Instance => mInstance;
        private DBConfigList() { }

        public Dictionary<eDBType, DBConfig> mDBConfigMap { get; private set; } = new Dictionary<eDBType, DBConfig>();

        public bool IsExist(eDBType dbServerName) => mDBConfigMap.ContainsKey(dbServerName);

        public int Count => mDBConfigMap.Count;
        
        public bool TryAdd(eDBType type, DBConfig config)
        {
            return mDBConfigMap.TryAdd(type, config);
        }
        
        public bool TryRemove(eDBType type)
        {
            if (!IsExist(type))
                return false;

            mDBConfigMap.Remove(type);

            return true;
        }

        public string GetConnectionString(eDBType type)
        {
            if (type <= eDBType._MIN_NO_TYPE || type >= eDBType._MAX_NO_TYPE)
                return default(string);

            if (mDBConfigMap.TryGetValue(type, out var dbConfig))
                return dbConfig.GetConnectionString();
            else
                return default(string);
        }

        public IEnumerable<DBConfig> GetEnumerator()
        {
            using (var enumerator = mDBConfigMap.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
        }
    }

    public sealed class DBConfig 
    {
        public eDBType type { get; private set; }

        public string ip { get; private set; }

        public string port { get; private set; } 

        public string loginID { get; private set; }

        public string loginPW { get; private set; }

        private readonly string mConnectionString;

        public DBConfig(eDBType type, string ip, string port = "1433", string loginID = "sa", string loginPW = "sa")
        {
            if (type <= eDBType._MIN_NO_TYPE || type >= eDBType._MAX_NO_TYPE)
                throw new ArgumentException(nameof(type));

            if (string.IsNullOrEmpty(ip))
                throw new ArgumentNullException(nameof(ip));

            this.type = type;
            this.ip = ip;
            this.port = port;
            this.loginID = loginID;
            this.loginPW = loginPW;

            mConnectionString = $"Data Source={ip},{port};Initial Catalog={type};User ID={loginID};Password={loginPW}";
        }

        public string GetConnectionString()
        {
            return mConnectionString;
        }
      
    }
}
