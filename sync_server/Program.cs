

namespace sync_server
{
    class Program
    {
        //socket example from https://www.codeproject.com/Articles/10649/An-Introduction-to-Socket-Programming-in-NET-using
        //try Socket here: https://docs.microsoft.com/en-us/dotnet/framework/network-programming/socket-code-examples
        public static void Main(string[] args)
        {
            SyncServer srv = new SyncServer();
            srv.StartServer();
            //StartListener();
        }

    }
}
