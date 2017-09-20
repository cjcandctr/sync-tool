using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace sync_client
{
    //TODO: connector instance management
    public class SocketConnector
    {
        private static TcpClient client;        
        public static ConfigMan conf = null;
        StreamWriter sw;
        public SocketConnector()
        {
            conf = new ConfigMan();
            client = new TcpClient(conf.ServerAddress, conf.ServerPort);
            sw = new StreamWriter(client.GetStream());
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
        internal Dictionary<string, IndexItem> GetServerIndex()
        {
            try
            {
                    
                sw.WriteLine("get_server_index"); // TODO add enum in both server side and client side
                sw.Flush();
                var serializedJson = ReadString();
                var dict = JsonConvert.DeserializeObject<Dictionary<string, IndexItem> >(serializedJson);    
                return dict;
            }
            catch(Exception ex)
            {                
                Debug.Print(ex.Message);
                return null;
            }
        }

        internal Dictionary<string, IndexItem> UpdateServerIndex()
        {
            try
            {                    
                sw.WriteLine("update_server_index"); // TODO add enum in both server side and client side
                sw.Flush();
                var serializedJson = ReadString();
                var dict = JsonConvert.DeserializeObject<Dictionary<string, IndexItem> >(serializedJson);    
                return dict;
            }
            catch(Exception ex)
            {                
                Debug.Print(ex.Message);
                return null;
            }
        }

        private string ReadString()
        {
            var ns =client.GetStream();
            int a = client.Available;
            byte[] lenByte = new byte[4];
            ns.Read(lenByte,0,4);
            var len = BitConverter.ToInt32(lenByte,0);

            byte[] buf = new byte[len];
            ns.Read(buf,0,len);
            var reslut = Encoding.UTF8.GetString(buf);
            return reslut;
        }

        internal void DownloadTo(SyncItem item)
        {
            try
            {                    
                sw.WriteLine("request_server_file"); // TODO add enum in both server side and client side
                sw.Flush();
                byte[] lenByte = new byte[4];
                client.GetStream().Read(lenByte,0,4);
                var len = BitConverter.ToInt32(lenByte,0);
                
                byte[] buf = new byte[len];
                client.GetStream().Read(buf,0,len);
                item.Data=buf;                
            }
            catch(Exception ex)
            {                
                Debug.Print(ex.Message);                
            }
        }
    }
}