using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using sync_client;

namespace sync_server
{
    public class SocketConnector
    {
        private TcpClient tcp;                
        int timeout = 5 * 60 * 1000;
        NetworkStream nstream = null;
        ConfigManager conf = new ConfigManager();
        public SocketConnector(TcpListener listener)
        {                    
            tcp = listener.AcceptTcpClient();
            nstream = tcp.GetStream();
            nstream.ReadTimeout = 30000;
        }
        ~SocketConnector()
        {
            if(tcp != null)
                try 
                {
                    tcp.Close();
                }
                catch(Exception ex)
                {
                    Program.logger.Error("close tcp connection exception when shuttdown");
                }
        }
        

        internal void SendIndexDict(Dictionary<string, IndexItem> dic)
        {                                    
            if(dic == null || dic.Count == 0)
            {

            }
            try
            {                                
                var strBlock = JsonConvert.SerializeObject(dic, Formatting.Indented);
                SendString(strBlock);                
            }
            catch (Exception ex)
            {
                Program.logger.Warn("SendIndexDict", ex);                
            }            
        }

        internal string WaitforCommand()
        {
            try
            {                
                Program.logger.Info("HOHO I'm waiting for command.");
                while(tcp.Available==0 && timeout >=0 && this.IsConnected())
                {
                    timeout -= 500;
                    Thread.Sleep(500);                    
                }
                if(timeout<0 || !this.IsConnected())                 
                {                    
                    tcp.Close();
                    return "";
                }
                var comStr = ReadString();                

                return comStr;                                             
            }
            catch (Exception ex)
            {
                Program.logger.Warn("WaitforCommand Error", ex);
                tcp.Close();
                return "";
            }
        }


        internal string ReveiveAndSave()
        {
            var name = ReadString();
            var len = ReadTargetByteInStream();           
            byte[] buf = new byte[len];
            var total = 0;      
            var left = len;      
            
            while(total < len)
            {                                
                var size = Math.Min(left, tcp.ReceiveBufferSize);
                var received = nstream.Read(buf,total,size);
                total += received;
                left -= received;                
            }
            
            name = conf.StorageLocation + name;
            (new FileInfo(name)).Directory.Create();
            File.WriteAllBytes(name, buf);
            return name;
        }
        

        internal void SendRequestFile()
        {
            var name = ReadString();
            
            // Buffer for reading data
            Byte[] buf = File.ReadAllBytes(name);
            tcp.GetStream().Write(BitConverter.GetBytes(buf.Length),0,4);
            tcp.GetStream().Flush();
            tcp.GetStream().Write(buf,0,buf.Length);
            tcp.GetStream().Flush();            
        }

        private string ReadString()
        {            
            var len = ReadTargetByteInStream();
            byte[] buf = new byte[len];
            var total = 0;      
            var left = len;      
            
            while(total < len)
            {                                
                var size = Math.Min(left, tcp.ReceiveBufferSize);
                var received = nstream.Read(buf,total,size);
                total += received;
                left -= received;                
            }

            //nstream.Read(buf,0,len);
            var reslut = Encoding.UTF8.GetString(buf);
            return reslut;
        }
        private int ReadTargetByteInStream()
        {
            byte[] lenByte = new byte[4];
            nstream.Read(lenByte,0,4);
            var len = BitConverter.ToInt32(lenByte,0);
            return len;
        }

        internal void SendString(string strBlock)
        {
            
            var buf = Encoding.UTF8.GetBytes(strBlock);                            
            
            nstream.Write(BitConverter.GetBytes(buf.Length),0,4);
            nstream.Flush();
            nstream.Write(buf,0,buf.Length);
            nstream.Flush();            
        }
        
        internal void SendACK(string command)
        {
            var buf = Encoding.UTF8.GetBytes(command);            
            nstream.Write(buf,0,buf.Length);
            nstream.Flush();  
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