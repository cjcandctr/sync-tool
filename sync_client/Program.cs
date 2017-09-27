using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using log4net;


namespace sync_client
{
    class Program
    {
        public static bool IsWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        public static bool IsLinux = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
        public static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Program));
        static void Main(string[] args)
        {
            AddLog4Net();            
            SyncClient sc = new SyncClient();
            logger.Info("Client Started");
            try{
                sc.Start();
            } catch (Exception ex)
            {
                logger.Error("Start Sync Client Error",ex);
            }
        }

        private static void AddLog4Net()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }
    }
}
