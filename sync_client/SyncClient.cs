using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace sync_client
{
    public class SyncClient
    {
        internal static ConfigMan conf = new ConfigMan();
        Dictionary<string, IndexItem> serverIndex = null;
        FileScanner scn;
        SocketConnector conn;
        internal void Start()
        {                        
            scn = new FileScanner(conf.ScanBase, conf.IgnoredPath, conf.SizeLimit);
            var localIndex = scn.Scan();
            serverIndex = UpdateServerIndex(serverIndex);
            var tuple = BuildSyncItem(localIndex, serverIndex);
            SyncLocal(tuple.Item1);
            SyncServer(tuple.Item2);
        }
        private Dictionary<string, IndexItem> UpdateServerIndex(Dictionary<string, IndexItem> serverIndex)
        {
            //Mockup here:            
            //FileScanner.MockServerIndex();
            if(conn == null) conn = new SocketConnector(); 
            if(serverIndex == null) 
            {                
                serverIndex = conn.GetServerIndex();    
                return serverIndex;        
            }
            

            //Dictionary<string, IndexItem> updateIndex = conn.UpdateServerIndex();
            FileScanner fs = new FileScanner();
            Dictionary<string, IndexItem> updateIndex = fs.MockServerindexUpdate();
            foreach(var pair in updateIndex)
            {
                if(serverIndex.ContainsKey(pair.Key))
                {
                    serverIndex.Remove(pair.Key);
                }
                serverIndex.Add(pair.Key,pair.Value);                                                
            }
            return serverIndex;
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
                        break;
                    case SyncChangeType.Conflict:
                        break;
                }
            }
            //throw new NotImplementedException();
        }

        private void CreateServerFile(SyncItem item)
        {
            throw new NotImplementedException();
        }

        private async void SyncLocal(List<SyncItem> syncItems)
        {
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
                        break;
                    case SyncChangeType.Conflict:
                        break;
                }
            }
            //throw new NotImplementedException();
        }

        private void CreateFile(SyncItem item)
        {
            conn.DownloadTo(item);

        }
    }
}