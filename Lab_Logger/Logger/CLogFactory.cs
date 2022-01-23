using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Logger.Logger
{ 
    /// <summary>
    /// Factory Method Design Pattern
    /// </summary>
    public interface ILogFactory
    {
        CLogger GetLogger();
    }
}
