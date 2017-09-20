
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using System.Collections.Generic;
using sync_client;
using System.Diagnostics;

namespace sync_server
{
    public class SyncServer
    {                
        DateTime LatestUpdateTime;
        FileScanner fc;
        static TcpListener listener;
        public static ConfigManager Conf = new ConfigManager();
        public static bool IsStart = true;

        public void StartServer()
        {            
            fc = new FileScanner(Conf.StorageLocation, null, 0);
            fc.Scan();
            Debug.Print("HOHO: server file index setup");
            StartListener();
        }

        private void StartListener()
        {
            IPAddress localAddr = IPAddress.Parse("0.0.0.0");

            listener = new TcpListener(localAddr, Conf.Port);
            listener.Start();
            for (int i = 0; i < Conf.ConnectionLIMIT; i++)
            {
                Thread t = new Thread(new ThreadStart(Service));
                t.Start();                
            }
        }
        public void StopServer()
        {
            IsStart=false;
        }

        public void Service()
        {
            Debug.Print("HOHO: server is listening " + Conf.Port + " port in thread ID " + Thread.CurrentThread.ManagedThreadId);
            SocketConnector socon = new SocketConnector(listener);
            while (IsStart)
            {
                string commandstr = socon.WaitforCommand();
                CommandEnum command ;
                Enum.TryParse<CommandEnum>( commandstr, true, out command);// TODO Exception
                switch(command)
                {
                    case CommandEnum.get_server_index:
                        SendFullIndex(socon);
                        break;
                    case CommandEnum.request_server_file:
                        SendRequestFile(socon);
                        break;
                    default:
                        break;
                }
                if(command == CommandEnum.idle_timeout_command)
                {
                    Thread t = new Thread(new ThreadStart(Service));
                    t.Start();
                    break;
                }                
            }
        }

        private void SendRequestFile(SocketConnector socon)
        {
            socon.GetRequestFileName();            
        }

        private void SendFullIndex(SocketConnector socon)
        {
            var index = fc.Scan();
            socon.SendFullIndex(index);
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