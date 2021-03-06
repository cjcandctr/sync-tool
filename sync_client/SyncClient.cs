using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace sync_client
{
    public class SyncClient
    {
        //TODO Serialize Index...
        internal static ConfigMan conf = new ConfigMan();
        Dictionary<string, IndexItem> serverIndex = null;
        FileScanner scn;
        SocketConnector conn;
        int intervalInSecond = 30;
        bool IsStart = true;
        internal void Start()
        {                        
            scn = new FileScanner(conf.ScanBase, conf.IgnoredPath, conf.SizeLimit);
            intervalInSecond = conf.IntervalSec;
            while(IsStart)
            {
                var localIndex = scn.Scan();
                serverIndex = UpdateServerIndex(serverIndex);
                var tuple = BuildSyncItem(localIndex, serverIndex);
                SyncLocal(tuple.Item1);
                SyncServer(tuple.Item2);                            
                Thread.Sleep(intervalInSecond * 1000);
            }
            
        }

        private void LogSyncItems(List<SyncItem> items)
        {
            if(items!=null)
            foreach(var item in items)
            {
                Program.logger.Debug(item.ChangeType + ": " + item.IndexItem.RealPath);
            }
        }

        private Dictionary<string, IndexItem> UpdateServerIndex(Dictionary<string, IndexItem> serverIndex)
        {
            if(conn == null) conn = new SocketConnector(); 
            if(serverIndex == null) 
            {
                serverIndex = conn.GetServerIndex();                
                LogIndex(serverIndex);
                return serverIndex;        
            }
            else
            {
                var updated = conn.UpdateServerIndex();
                if(updated == null) return serverIndex;
                LogIndex(updated);
                foreach(var pair in updated)
                {
                    if(serverIndex.ContainsKey(pair.Key))
                    {
                        serverIndex.Remove(pair.Key);                        
                    }
                    serverIndex.Add(pair.Key,pair.Value);
                }
            }
            
            return serverIndex;
        }
        
        private void LogIndex(Dictionary<string, IndexItem> updatedIndex)
        {
            if (updatedIndex != null)
                foreach(var pair in updatedIndex)
                {
                    Program.logger.Debug(pair.Key);
                }
        }

        private Tuple<List<SyncItem>,List<SyncItem>> BuildSyncItem(Dictionary<string, IndexItem> localIndex, Dictionary<string, IndexItem> serverIndex)
        {
            if(localIndex == null) throw new Exception("cannot build local file index");
            if(serverIndex == null) throw new Exception("cannot reviceve remote file index");

            List<SyncItem> syncsLocal = new List<SyncItem>();
            List<SyncItem> syncsServer = new List<SyncItem>();
            
            //var notInServer = localIndex.Keys.Except(serverIndex.Keys);            
            
            foreach(var localPair in localIndex)
            {       
                if(serverIndex.ContainsKey(localPair.Key))         
                {
                    var localItem = localPair.Value;
                    var serverItem = serverIndex.GetValueOrDefault(localPair.Key);
                    if(localItem.FileHash == serverItem.FileHash) continue;
                    //TODO Server delete the file
                    if(localItem.IsChanged && !serverItem.IsChanged)
                    {
                        SyncItem si = new SyncItem(localItem);
                        si.ChangeType = SyncChangeType.Update;
                        syncsServer.Add(si);
                    }
                    if(!localItem.IsChanged && serverItem.IsChanged)
                    {
                        SyncItem si = new SyncItem(serverItem);
                        si.ChangeType = SyncChangeType.Update;
                        syncsLocal.Add(si);
                    }
                    if(localItem.IsChanged && serverItem.IsChanged)
                    {
                        SyncItem si = new SyncItem(serverItem);
                        si.ChangeType = SyncChangeType.Conflict;
                        syncsLocal.Add(si);
                        si = new SyncItem(localItem);
                        si.ChangeType = SyncChangeType.Conflict;
                        syncsServer.Add(si);
                    }
                    //TODO local delete the file
                }
                else
                {
                    SyncItem si = new SyncItem(localPair.Value);
                    si.ChangeType = SyncChangeType.Create;
                    syncsServer.Add(si);
                }                
            }

            var notInLocal = serverIndex.Keys.Except(localIndex.Keys);
            foreach(var key in notInLocal)
            {
                SyncItem si = new SyncItem(serverIndex.GetValueOrDefault(key));
                si.ChangeType = SyncChangeType.Create;
                syncsLocal.Add(si);
            }
            return new Tuple<List<SyncItem>, List<SyncItem>>(syncsLocal,syncsServer);
        }

        private async void SyncServer(List<SyncItem> syncItems)
        {
            LogSyncItems(syncItems);
            if (syncItems.Count<=0) return ;
            foreach(var item in syncItems)
            {
                switch(item.ChangeType)
                {
                    case SyncChangeType.Create:
                        CreateServerFile(item);
                        break;
                    case SyncChangeType.Delete:
                        break;
                    case SyncChangeType.Update:
                        UpdateServerFile(item);
                        break;
                    case SyncChangeType.Conflict:
                        break;
                }
            }
            //throw new NotImplementedException();
        }

        private void UpdateServerFile(SyncItem item)
        {
            CreateServerFile(item);
        }

        private void CreateServerFile(SyncItem item)
        {            
            var name = item.IndexItem.ClientScanBase + item.IndexItem.Name;            
            item.Data = File.ReadAllBytes(name);
            conn.Upload(item);                
        
        }

        private async void SyncLocal(List<SyncItem> syncItems)
        {            
            LogSyncItems(syncItems);
            if (syncItems.Count<=0) return ;
            foreach(var item in syncItems)
            {
                switch(item.ChangeType)
                {
                    case SyncChangeType.Create:
                        CreateFile(item);
                        break;
                    case SyncChangeType.Delete:
                        break;
                    case SyncChangeType.Update:
                        UpdateFile(item);
                        break;
                    case SyncChangeType.Conflict:
                        break;
                }
            }
            //throw new NotImplementedException();
        }

        private void UpdateFile(SyncItem item)
        {
            conn.DownloadTo(item);
            var name = "";
            if(item.IndexItem.Name.StartsWith(@"_root_"))
            {
                name = item.IndexItem.Name.Replace(@"_root_", "");
            }
            else 
            {
                name = item.IndexItem.Name.Insert(1,":");
            }

            (new FileInfo(name)).Directory.Create();
            File.WriteAllBytes(name, item.Data);
        }

        private void CreateFile(SyncItem item)
        {
            conn.DownloadTo(item);
            var name = "";
            if(item.IndexItem.Name.StartsWith(@"_root_"))
            {
                if(Program.IsLinux)
                    name = item.IndexItem.Name.Replace(@"_root_", "");
                else if(Program.IsWindows)
                    name = item.IndexItem.Name.Replace(@"_root_", "c:");
            }
            else 
            {
                if(Program.IsLinux)
                    name = item.IndexItem.Name.Insert(0,"/");
                else if(Program.IsWindows)
                    name = item.IndexItem.Name.Insert(1,":");
            }

            (new FileInfo(name)).Directory.Create();
            File.WriteAllBytes(name, item.Data);
        }
    }
}