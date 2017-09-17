using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;
using sync_client;
//??? 一模一样的类 在客户端和服务端写两遍, 使用Binary序列化的结果会不会一样? 猜测是一样的...
namespace sync_server
{
    public class SocketConnector
    {
        private static TcpClient client;        
        public static ConfigManager conf = null;
        public SocketConnector()
        {
            //conf = new ConfigMan();
            //client = new TcpClient(conf.ServerAddress, conf.ServerPort);
        }
        ~SocketConnector()
        {
            // if(client != null)
            //     try 
            //     {
            //         client.Close();
            //     }
            //     catch(Exception)
            //     {
            //         Debug.Print("close tcp client exception");
            //     }
        }

        internal static void UpdateIndex(TcpListener listener)
        {            
            Dictionary<string, IndexItem> dic = LoadMockupDic();
            
            //var json = JsonConvert.SerializeObject(dic, Formatting.Indented);
            //var dic1 = JsonConvert.DeserializeObject<Dictionary<string, IndexItem> >(json1);    
            Socket soc = listener.AcceptSocket();
                        
            //soc.SetSocketOption(SocketOptionLevel.Socket,
            //        SocketOptionName.ReceiveTimeout,10000);
            //log
            try
            {
                Stream s = new NetworkStream(soc);
                StreamReader sr = new StreamReader(s);
                StreamWriter sw = new StreamWriter(s);
                //sw.AutoFlush = true; // enable automatic flushing
                sw.Write(JsonConvert.SerializeObject(dic, Formatting.Indented));
                // while (true)
                // {
                //     string name = sr.ReadLine();
                //     if (name == "" || name == null) break;
                //     sw.WriteLine("message recieved" + name);
                // }
                sw.Flush();
                s.Close();
            }
            catch (Exception)
            {
                //Log
            }
            //log
            soc.Close();



            // Socket soc = listener.AcceptSocket();
            // //soc.SetSocketOption(SocketOptionLevel.Socket,
            // //        SocketOptionName.ReceiveTimeout,10000);
            // //log
            // try
            // {
            //     Stream s = new NetworkStream(soc);
            //     StreamReader sr = new StreamReader(s);
            //     StreamWriter sw = new StreamWriter(s);
            //     sw.AutoFlush = true; // enable automatic flushing
            //     sw.WriteLine("service is available");
            //     while (true)
            //     {
            //         string name = sr.ReadLine();
            //         if (name == "" || name == null) break;
            //         sw.WriteLine("message recieved" + name);
            //     }
            //     s.Close();
            // }
            // catch (Exception)
            // {
            //     //Log
            // }
            // //log
            // soc.Close();
        }

        private static Dictionary<string, IndexItem> LoadMockupDic()
        {
            Dictionary<string, IndexItem> serverIndex = new Dictionary<string, IndexItem>();      
            IndexItem item = new IndexItem();
            string folder = @"./mock-data";
            string file = folder + "/aText.txt";
            item.Base = folder;
            item.Path = file.Replace(folder, ".").Replace(@"\","/");
            //item.FileHash = GetHash(folder, file);
            item.IsChanged = false;
            item.IsFolder = File.GetAttributes(file).HasFlag(FileAttributes.Directory);
            //item.IsEmpty = item.IsFolder && IsDirectoryEmpty(file);
            item.UpdateTime = DateTime.Now;            
            serverIndex.Add(item.Path,item);
            item = new IndexItem();
            file = folder + "/btest.doc";
            item.Base = folder;
            item.Path = file.Replace(folder, ".").Replace(@"\","/");
            //item.FileHash = GetHash(folder, file);
            item.IsChanged = false;
            item.IsFolder = File.GetAttributes(file).HasFlag(FileAttributes.Directory);
            //item.IsEmpty = item.IsFolder && IsDirectoryEmpty(file);
            item.UpdateTime = DateTime.Now;  
            serverIndex.Add(item.Path,item);
            
            
            return serverIndex;
        }
    }
}