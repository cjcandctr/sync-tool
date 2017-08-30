
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using System.Collections.Generic;

namespace sync_server
{
    public class SyncServer
    {

        void UpdateIndex() { }
        SortedSet<string> AllFileIndex { get; set; }
        DateTime LatestUpdateTime;

        static TcpListener listener;
        static ConfigManager conf = new ConfigManager();

        public void StartServer()
        {
            StartListener();
        }

        private static void StartListener()
        {
            IPAddress localAddr = IPAddress.Parse("0.0.0.0");

            listener = new TcpListener(localAddr, conf.Port);
            listener.Start();
            //LOG
            for (int i = 0; i < conf.ConnectionLIMIT; i++)
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
                catch (Exception)
                {
                    //Log
                }
                //log
                soc.Close();
            }
        }



        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

    }
}