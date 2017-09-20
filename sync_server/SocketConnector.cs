using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using sync_client;
//??? 一模一样的类 在客户端和服务端写两遍, 使用Binary序列化的结果会不会一样? 猜测是一样的...
namespace sync_server
{
    public class SocketConnector
    {
        private TcpClient tcp;                
        int timeout = 1 * 60 * 1000;
        StreamReader reader;
        StreamWriter writer;
        public SocketConnector(TcpListener listener)
        {                    
            tcp = listener.AcceptTcpClient();
            
            reader = new StreamReader(tcp.GetStream());
            writer = new StreamWriter(tcp.GetStream());
        }
        ~SocketConnector()
        {
            if(tcp != null)
                try 
                {
                    tcp.Close();
                }
                catch(Exception)
                {
                    Debug.Print("close tcp client exception");
                }
        }

        internal void SendFullIndex(Dictionary<string, IndexItem> dic)
        {                                    
            //var json = JsonConvert.SerializeObject(dic, Formatting.Indented);
            //var dic1 = JsonConvert.DeserializeObject<Dictionary<string, IndexItem> >(json1);    
            if(dic == null || dic.Count == 0)
            {

            }
                        
            //soc.SetSocketOption(SocketOptionLevel.Socket,
            //        SocketOptionName.ReceiveTimeout,10000);
            //log
            try
            {                
                

                var strBlock = JsonConvert.SerializeObject(dic, Formatting.Indented);
                sendString(strBlock);
                
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                //Log
            }
            //log
        
        }

        private void sendString(string strBlock)
        {
            
            var buf = Encoding.UTF8.GetBytes(strBlock);                            
            
            tcp.GetStream().Write(BitConverter.GetBytes(buf.Length),0,4);
            tcp.GetStream().Flush();
            tcp.GetStream().Write(buf,0,buf.Length);
            tcp.GetStream().Flush();            
        }

        internal string WaitforCommand()
        {
            try
            {                
                while(tcp.Available==0 && timeout >=0 && this.IsConnected())
                {
                    timeout -= 500;
                    Thread.Sleep(500);
                    Debug.Print("HOHO I'm sleeping \n");
                }
                if(timeout<0 || !this.IsConnected())                                
                {                    
                    tcp.Close();
                    return "";
                }
                var comStr = reader.ReadLine();
                //reader.Close();
                return comStr;                                             
            }
            catch (Exception ex)
            {
                tcp.Close();
                return "";
            }
        }

        internal void SendFileNameFowllowed()
        {
            var name = reader.ReadLine();
            
            // Buffer for reading data
            Byte[] buf = File.ReadAllBytes(name);
            tcp.GetStream().Write(BitConverter.GetBytes(buf.Length),0,4);
            tcp.GetStream().Flush();
            tcp.GetStream().Write(buf,0,buf.Length);
            tcp.GetStream().Flush();            
        }

        public bool IsConnected()
        {
            try
            {
                return !(tcp.Client.Poll(50, SelectMode.SelectRead) && tcp.Available == 0);
            }
            catch (SocketException) 
            { return false; }
        }
        
    }
}