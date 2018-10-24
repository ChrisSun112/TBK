using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tbk.Common
{
    public class LogHelper
    {
                
        public static void WriteLog(Type t, Exception ex)
        {
            ILog log = log4net.LogManager.GetLogger(t);

            log.Error(ex.Message + "\n" + ex.StackTrace);
        }

        public static void WriteLog(Type t, string message)
        {
            ILog log = log4net.LogManager.GetLogger(t);
            log.Error(message);
        }
    }
}
