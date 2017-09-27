

using log4net;

namespace sync_server
{
    class Program
    {
        //socket example from https://www.codeproject.com/Articles/10649/An-Introduction-to-Socket-Programming-in-NET-using
        //try Socket here: https://docs.microsoft.com/en-us/dotnet/framework/network-programming/socket-code-examples

        public static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Program));
        public static void Main(string[] args)
        {
            AddLog4Net();            
            SyncServer srv = new SyncServer();
            srv.StartServer();
            //StartListener();
        }

        private static void AddLog4Net()
        {
            var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
            log4net.Config.XmlConfigurator.Configure(logRepository, new System.IO.FileInfo("log4net.config"));
        }

    }
}
