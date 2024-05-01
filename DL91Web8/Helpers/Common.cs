using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DL91Web.Helpers
{
    public class Common
    {
        public static DateTime CompileTime
        {
            get
            {
                if (RunningModeIsDebug)
                    return DateTime.Now;

                System.Version MyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                DateTime compileTime = new DateTime(2000, 1, 1).AddDays(MyVersion.Build).AddSeconds(MyVersion.Revision * 2);
                return compileTime;
            }
        }

        public static bool RunningModeIsDebug
        {
            get
            {

                var assebly = Assembly.GetEntryAssembly();
                if (assebly == null)
                {
                    assebly = new StackTrace().GetFrames().Last().GetMethod().Module.Assembly;
                }

                var debugableAttribute = assebly.GetCustomAttribute<DebuggableAttribute>();
                var isdebug = debugableAttribute.DebuggingFlags.HasFlag(DebuggableAttribute.DebuggingModes.EnableEditAndContinue);

                return isdebug;
            }
        }
    }
}
