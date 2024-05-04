using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DL91
{
    public class LogTool : ILogTool
    {
        private readonly ILog log = LogManager.GetLogger("LogRepository", typeof(LogTool));

        private static LogTool _logTool;
        public static LogTool Instance 
        { 
            get 
            { 
                if(_logTool == null)
                    _logTool = new LogTool();
                return _logTool;
            }
        }

        /// <summary>
        /// 写入错误日志
        /// </summary>
        /// <param name="errorInfo">错误日志内容</param>
        public void Error(string errorInfo)
        {
            log.Error(errorInfo);
        }

        public void Error(string exInfo, Exception e)
        {
            var msg = exInfo + ":" + e.Message + "\r\n" + e.StackTrace;
            Error(msg);
        }

        /// <summary>
        /// 写入消息日志
        /// </summary>
        /// <param name="info">消息日志内容</param>
        public void Info(string info)
        {
            log.Info(info);
        }

        /// <summary>
        /// 写入调试日志
        /// </summary>
        /// <param name="debugInfo">调试日志内容</param>
        public void Debug(string debugInfo)
        {
            log.Debug(debugInfo);
        }
    }
}
