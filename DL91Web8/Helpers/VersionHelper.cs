using System.Diagnostics;
using System.Reflection;

namespace DL91Web8.Helpers
{
    public class VersionHelper
    {
        public static DateTime CompileTime
        {
            get
            {
                if (RunningModeIsDebug)
                    return DateTime.Now;

                var MyVersion = Assembly.GetExecutingAssembly()?.GetName()?.Version;
                if (MyVersion == null)
                    return DateTime.Now;
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
