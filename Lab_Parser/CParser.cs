using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lab_Parser
{
    internal class JsonParser
    {
        public void ConvertJsonToObject<T>() where T : new()
        {
            JsonSerializer.Deserialize<T>
        }
    }
}
