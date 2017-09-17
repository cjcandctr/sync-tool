using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace sync_client
{
    //TODO: connector instance management
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
        internal void UpdateServerIndex(Dictionary<string, IndexItem> serverIndex)
        {
            try
            {
                Stream s = client.GetStream();
                StreamReader sr = new StreamReader(s);
                StreamWriter sw = new StreamWriter(s);
                //sw.AutoFlush = true;
                var serializedJson = sr.ReadToEnd();
                var dic1 = JsonConvert.DeserializeObject<Dictionary<string, IndexItem> >(serializedJson);    
                // while (true)
                // {
                //     Console.Write("Name: ");
                //     string name = Console.ReadLine();
                //     sw.WriteLine(name);
                //     if (name == "") break;
                //     Console.WriteLine(sr.ReadLine());
                // }
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