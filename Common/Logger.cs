using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Logger
    {
        public static Serilog.Core.Logger coreLogger;

        private static Logger warpLogger;

        public static Serilog.Core.Logger Instance { get { return coreLogger; } }

        private Logger(string logFileName)
        {
            coreLogger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File($"logs/{logFileName}-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public static void Create(string logFileName)
        {
            string logDir = "logs";
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            warpLogger = (warpLogger == null)?new Logger(logFileName) : warpLogger;
        }


    }
}
