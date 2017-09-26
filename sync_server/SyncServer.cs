
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using System.Collections.Generic;
using System.Linq;
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
            List<string> scanbase = new List<string>();
            scanbase.Add(Conf.StorageLocation);
            fc = new FileScanner(scanbase, null, 0);
            fc.ServerStorageBase = Conf.StorageLocation;
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
                    case CommandEnum.get_index_update:
                        SendIndexUpdate(socon);
                        break;
                    case CommandEnum.request_server_file:
                        SendRequestFile(socon);
                        break;
                    case CommandEnum.create_file:
                        ReveiveAndSave(socon);
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

        private void SendIndexUpdate(SocketConnector socon)
        {
            fc.Scan();            
            socon.SendIndexDict(fc.UpdatedIndex);            
            socon.SendACK(CommandEnum.get_index_update.ToString());
        }

        private void ReveiveAndSave(SocketConnector socon)
        {
            socon.ReveiveAndSave();
            socon.SendACK(CommandEnum.create_file.ToString());
        }

        private void SendRequestFile(SocketConnector socon)
        {
            socon.SendRequestFile();   
            socon.SendACK(CommandEnum.request_server_file.ToString());         
        }

        private void SendFullIndex(SocketConnector socon)
        {
            var index = fc.Scan();
            
            socon.SendIndexDict(index);
            socon.SendACK(CommandEnum.get_server_index.ToString());
        }

        // public static void UpdateKey<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey fromKey, TKey toKey)
        // {
        //     TValue value = dic[fromKey];
        //     dic.Remove(fromKey);
        //     dic[toKey] = value;
        // }

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
            item.ClientScanBase = folder;
            item.Name = file.Replace(folder, ".").Replace(@"\","/");
            //item.FileHash = GetHash(folder, file);
            item.IsChanged = false;
            item.IsFolder = File.GetAttributes(file).HasFlag(FileAttributes.Directory);
            //item.IsEmpty = item.IsFolder && IsDirectoryEmpty(file);
            item.UpdateTime = DateTime.Now;            
            serverIndex.Add(item.Name,item);
            item = new IndexItem();
            file = folder + "/btest.doc";
            item.ClientScanBase = folder;
            item.Name = file.Replace(folder, ".").Replace(@"\","/");
            //item.FileHash = GetHash(folder, file);
            item.IsChanged = false;
            item.IsFolder = File.GetAttributes(file).HasFlag(FileAttributes.Directory);
            //item.IsEmpty = item.IsFolder && IsDirectoryEmpty(file);
            item.UpdateTime = DateTime.Now;  
            serverIndex.Add(item.Name,item);
            
            
            return serverIndex;
        }

    }
}