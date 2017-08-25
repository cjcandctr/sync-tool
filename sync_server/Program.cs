using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace sync_server
{
    class Program
    {
        //socket example from https://www.codeproject.com/Articles/10649/An-Introduction-to-Socket-Programming-in-NET-using
        //try Socket here: https://docs.microsoft.com/en-us/dotnet/framework/network-programming/socket-code-examples
        static TcpListener listener;
        const int ClientLIMIT = 5; //5 concurrent clients

        public static void Main(string[] args)
        {
            IPAddress localAddr = IPAddress.Parse("0.0.0.0");
            int port = 8001;
            listener = new TcpListener(localAddr, port);
            listener.Start();
            //LOG
            for (int i = 0; i < ClientLIMIT; i++)
            {
                Thread t = new Thread(new ThreadStart(Service));
                t.Start();
            }
        }
        public static void Service()
        {
            while (true)
            {
                Socket soc = listener.AcceptSocket();
                //soc.SetSocketOption(SocketOptionLevel.Socket,
                //        SocketOptionName.ReceiveTimeout,10000);
                //log
                try
                {
                    Stream s = new NetworkStream(soc);
                    StreamReader sr = new StreamReader(s);
                    StreamWriter sw = new StreamWriter(s);
                    sw.AutoFlush = true; // enable automatic flushing
                    sw.WriteLine("service is available");
                    while (true)
                    {
                        string name = sr.ReadLine();
                        if (name == "" || name == null) break;                        
                        sw.WriteLine("message recieved" + name);
                    }
                    s.Close();
                }
                catch (Exception )
                {
                    //Log
                }
                //log
                soc.Close();
            }
        }

    }
}
