using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DL91
{
    public interface ILogTool
    {
        /// <summary>
        /// 写入错误日志
        /// </summary>
        /// <param name="errorInfo">错误日志内容</param>
        public void Error(string errorInfo);

        public void Error(string exInfo, Exception e);

        /// <summary>
        /// 写入消息日志
        /// </summary>
        /// <param name="info">消息日志内容</param>
        public void Info(string info);

        /// <summary>
        /// 写入调试日志
        /// </summary>
        /// <param name="debugInfo">调试日志内容</param>
        public void Debug(string debugInfo);
    }
}
