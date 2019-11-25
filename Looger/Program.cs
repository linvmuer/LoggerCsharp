using System;
using System.Diagnostics;
using System.IO;
namespace Looger
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            LogHelper.Log(LogTarget.File,"Hello  sdfjsdfjsdjfsdf dfjsdfjdsfjdsf dsfjsdfjdsjfds fdfjdjfdjf",Level.Exception);
        }
        public  enum LogTarget
        {
            File,Database,EventLog
        }

        public enum Level
        {
            Normal,Exception
        }
        public abstract class LogBase
        {
            protected readonly object lockObj=new object();
            public abstract void Log(string message,Level level);
        }
        public class FileLogger:LogBase
        {
            private string filePath;

            public FileLogger(string filePath)
            {
                this.filePath= filePath;
            }
            public string FILEPATH {
                get
                {
                    return filePath;
                }
                set
                {
                    if (Directory.Exists(Path.Combine(value)))
                    {
                        FILEPATH = value;
                    }
                }
            }

            public override void Log(string message,Level level)
            {
                lock (lockObj)
                {
                    //we can have error when trying to write to a directory
                    if (level == Level.Normal)
                    {
                        
                        using (StreamWriter streamWriter=new StreamWriter(FILEPATH+@"\log.txt",true))
                        {   DateTime now=DateTime.Now;
                            var preparedMessage = now + "::>>" + message + "\n";
                            streamWriter.WriteLine(preparedMessage);
                            streamWriter.Close();
                        }
                    }

                    if (level==Level.Exception)
                    {
                        
                    using (StreamWriter streamWriter=new StreamWriter(FILEPATH+@"\log.txt",true))
                    {   DateTime now=DateTime.Now;
                        var stackTrace = Environment.StackTrace;
                        var preparedMessage = now + "An Exception Occured::>>" + message + "\n::>>Here is the stack Trace"+stackTrace+"\n";
                        streamWriter.WriteLine(preparedMessage);
                        streamWriter.Close();
                    }
                    }
                }
                
            }
        }

        public class DBLogger : LogBase
        {
            private string connectionString = string.Empty;
            public override void Log(string message, Level level)
            {
                lock (lockObj)
                {
                    throw new NotImplementedException();
                }
            }
        }

        public class EventLogger:LogBase
        {
            public override void Log(string message, Level level)
            {
                lock (lockObj)
                {
                    EventLog eventLog=new EventLog("");
                    eventLog.Source = "IDGEventLog";
                    var preparedMessage=DateTime.Now+"::>>>"+message+"\n";
                    eventLog.WriteEntry(preparedMessage);
                }
            }
        }

        public static class LogHelper
        {
            private static LogBase logger = null;

            public static void Log(LogTarget target, string message,Level level)
            {
                switch(target)
                {
                    case LogTarget.File:
                        logger = new FileLogger(@"C:\Users\accountant.AHM\Desktop\testsig\pdfin");
                        logger.Log(message,level);
                        break;
                    case LogTarget.Database:
                        logger = new DBLogger();
                        logger.Log(message, level);
                        break;
                    case LogTarget.EventLog:
                        logger = new EventLogger();
                        logger.Log(message, level);
                        break;
                    default:
                        return;
                }
            }
        }
    }
}


