using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;

namespace sync_client
{
    public class SocketConnector
    {
        private static TcpClient client;        
        public static ConfigMan conf = null;
        public SocketConnector()
        {
            conf = new ConfigMan();
            client = new TcpClient(conf.ServerAddress, conf.ServerPort);
        }
        ~SocketConnector()
        {
            if(client != null)
                try 
                {
                    client.Close();
                }
                catch(Exception)
                {
                    Debug.Print("close tcp client exception");
                }
        }
        internal static void UpdateServerIndex(Dictionary<string, IndexItem> serverIndex)
        {
            try
            {
                Stream s = client.GetStream();
                StreamReader sr = new StreamReader(s);
                StreamWriter sw = new StreamWriter(s);
                sw.AutoFlush = true;
                Console.WriteLine(sr.ReadLine());
                while (true)
                {
                    Console.Write("Name: ");
                    string name = Console.ReadLine();
                    sw.WriteLine(name);
                    if (name == "") break;
                    Console.WriteLine(sr.ReadLine());
                }
                s.Close();
            }
            finally
            {
                // code in finally block is guranteed 
                // to execute irrespective of 
                // whether any exception occurs or does 
                // not occur in the try block
                client.Close();
            }
        }

    }
}