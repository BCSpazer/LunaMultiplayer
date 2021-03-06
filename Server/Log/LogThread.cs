﻿using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LunaCommon.Time;
using Server.Context;

namespace Server.Log
{
    public class LogThread
    {
        private static long _lastLogExpiredCheck;
        private static long _lastDayCheck;

        public static async void RunLogThread()
        {
            while (ServerContext.ServerRunning)
            {
                //Run the log expire function every 10 minutes
                if (ServerContext.ServerClock.ElapsedMilliseconds - _lastLogExpiredCheck > 600000)
                {
                    _lastLogExpiredCheck = ServerContext.ServerClock.ElapsedMilliseconds;
                    LogExpire.ExpireLogs();
                }

                // Check if the day has changed, every minute
                if (ServerContext.ServerClock.ElapsedMilliseconds - _lastDayCheck > 60000)
                {
                    _lastDayCheck = ServerContext.ServerClock.ElapsedMilliseconds;
                    if (ServerContext.Day != LunaTime.Now.Day)
                    {
                        LunaLog.LogFilename = Path.Combine(LunaLog.LogFolder, $"lmpserver {LunaTime.Now:yyyy-MM-dd HH-mm-ss}.log");
                        LunaLog.WriteToLog($"Continued from logfile {LunaTime.Now:yyyy-MM-dd HH-mm-ss}.log");
                        ServerContext.Day = LunaTime.Now.Day;
                    }
                }

                await Task.Delay(250);
            }
        }
    }
}
