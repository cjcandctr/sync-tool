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
        NetworkStream nstream = null;
        public SocketConnector()
        {
            conf = new ConfigMan();
            client = new TcpClient(conf.ServerAddress, conf.ServerPort);
            nstream = client.GetStream();
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

                SendString(CommandEnum.get_server_index.ToString());                    
                var serializedJson = ReadString();
                ReadACK(CommandEnum.get_server_index.ToString());
                var dict = JsonConvert.DeserializeObject<Dictionary<string, IndexItem> >(serializedJson);    
                return dict;
            }
            catch(Exception ex)
            {                
                Debug.Print(ex.Message);
                return null;
            }
        }

        private void SendString(string strBlock)
        {            
            var buf = Encoding.UTF8.GetBytes(strBlock);                                        
            nstream.Write(BitConverter.GetBytes(buf.Length),0,4);
            nstream.Flush();
            nstream.Write(buf,0,buf.Length);
            nstream.Flush();            
        }
        
        private string ReadString()
        {            
            var len = ReadTargetByteInStream();
            byte[] buf = new byte[len];
            nstream.Read(buf,0,len);
            var reslut = Encoding.UTF8.GetString(buf);
            return reslut;
        }

        internal bool ReadACK(string command)
        {
            var buf = new byte[Encoding.UTF8.GetByteCount(command)];
            nstream.Read(buf,0,buf.Length);
            return command == Encoding.UTF8.GetString(buf);
        }        
        private int ReadTargetByteInStream()
        {
            byte[] lenByte = new byte[4];
            nstream.Read(lenByte,0,4);
            var len = BitConverter.ToInt32(lenByte,0);
            return len;
        }

        internal Dictionary<string, IndexItem> UpdateServerIndex()
        {
            try
            {                    
                SendString(CommandEnum.update_server_index.ToString()); 
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


        internal void DownloadTo(SyncItem item)
        {
            try
            {                    
                SendString(CommandEnum.request_server_file.ToString());
                
                int len = ReadTargetByteInStream();
                
                byte[] buf = new byte[len];
                nstream.Read(buf,0,len);
                item.Data=buf;                
            }
            catch(Exception ex)
            {                
                Debug.Print(ex.Message);                
            }
        }

        internal void Upload(SyncItem item)
        {
            try
            {
                SendString(CommandEnum.create_file.ToString()); 
                
                SendString(item.IndexItem.PathInServer + item.IndexItem.Name.Substring(1));                 
                nstream.Write(BitConverter.GetBytes(item.Data.Length),0,4);
                nstream.Write(item.Data,0,item.Data.Length); 
                nstream.Flush(); 
                ReadACK(CommandEnum.create_file.ToString());
            }
            catch(Exception ex)
            {                
                Debug.Print(ex.Message);                
            }
        }
    }
}