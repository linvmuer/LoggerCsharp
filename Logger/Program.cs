using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SQLite;
namespace Logger
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            LogHelper.Log(LogTarget.File,"Hello  sdfjsdfjsdjfsdf dfjsdfjdsfjdsf dsfjsdfjdsjfds fdfjdjfdjf",Level.Exception);
            LogHelper.Log(LogTarget.Database,"hello into database",Level.Exception);
            
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
                    //we can have error when trying to write to a directory make sure to always write to files
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
            static  string  datapath=@"URI=file:C:\Users\path_to_db";
            private string connectionString = string.Empty;
            private static bool TableExists (String tableName, SQLiteConnection connection)
            { 
                var cmd=new SQLiteCommand(connection);
                cmd.CommandText = "SELECT * FROM sqlite_master WHERE type = 'table' AND name = @name";
                cmd.Parameters.Add("@name", DbType.String).Value = tableName;
                return (cmd.ExecuteScalar() != null);
            }
            public override void Log(string message, Level level)
            {
                lock (lockObj)
                {
                    
                    var con=new SQLiteConnection(datapath);
                    con.Open();

                    string stm = @"CREATE TABLE IF NOT EXISTS log_data(id INTEGER PRIMARY KEY ,
                                                                      type TEXT,
                                                                      time TEXT,
                                                                      message TEXT)";
                    var cmd=new SQLiteCommand(stm,con);
                    var result=cmd.ExecuteNonQuery();
                    if (result>0)
                    {
                        cmd.CommandText= @"INSERT INTO log_data(type,time,message) VALUES(@type,@time,@message)";
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@type", $"{level}");
                        cmd.Parameters.AddWithValue("@time", DateTime.Now.ToString());
                        cmd.Parameters.AddWithValue("@message", message);
                        cmd.ExecuteNonQuery();
                        con.Close();

                    }

                    else if (TableExists("log_data", con))
                    {
                        cmd.CommandText= @"INSERT INTO log_data(type,time,message) VALUES(@type,@time,@message)";
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@type", $"{level}");
                        cmd.Parameters.AddWithValue("@time", DateTime.Now.ToString());
                        cmd.Parameters.AddWithValue("@message", message);
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
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


