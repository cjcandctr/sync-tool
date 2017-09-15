using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace sync_client
{
    public class FileScanner
    {
        Dictionary<string, IndexItem> localIndex = null;
        Dictionary<string, IndexItem> serverIndex = null;
        public static ConfigMan conf = null;
        public FileScanner()
        {
            if(conf == null) conf = new ConfigMan();        
        }

        public Tuple<List<SyncItem>,List<SyncItem>> Scan()
        {
            localIndex = UpdateIndex(localIndex, conf.ScanBase, conf.IgnoredPath);
            serverIndex = UpdateServerIndex();
            return BuildSyncItem(localIndex, serverIndex);
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

        private Dictionary<string, IndexItem> UpdateServerIndex()
        {

            //Mockup here:            
            SocketConnector.UpdateServerIndex(serverIndex);
            return serverIndex;
            return LoadMock();
            
            if(serverIndex == null) 
            {
                // TODO get from server
                serverIndex = new Dictionary<string, IndexItem>();                                
            }            
            return serverIndex;
        }

        private Dictionary<string, IndexItem> LoadMock()
        {
            serverIndex = new Dictionary<string, IndexItem>();      
            IndexItem item = new IndexItem();
            string folder = @"C:\Temp\delete2";
            string file = folder + "/HAPSA Issues Log Files.zip";
            item.Base = folder;
            item.Path = file.Replace(folder, ".").Replace(@"\","/");
            item.FileHash = GetHash(folder, file);
            item.IsChanged = false;
            item.IsFolder = File.GetAttributes(file).HasFlag(FileAttributes.Directory);
            item.IsEmpty = item.IsFolder && IsDirectoryEmpty(file);
            item.UpdateTime = DateTime.Now;            
            serverIndex.Add(item.Path,item);
            item = new IndexItem();
            file = folder + "/mmexport1505092189115.jpg";
            item.Base = folder;
            item.Path = file.Replace(folder, ".").Replace(@"\","/");
            item.FileHash = GetHash(folder, file);
            item.IsChanged = false;
            item.IsFolder = File.GetAttributes(file).HasFlag(FileAttributes.Directory);
            item.IsEmpty = item.IsFolder && IsDirectoryEmpty(file);
            item.UpdateTime = DateTime.Now;  
            serverIndex.Add(item.Path,item);
            
            
            return serverIndex;
        }

        private Dictionary<string, IndexItem> UpdateIndex(Dictionary<string, IndexItem> index, List<string> scanBase, List<string> excludeFile)
        {
            
            if(index == null) index = new Dictionary<string, IndexItem>();
            System.Threading.Tasks.Parallel.ForEach(scanBase, folder=>{
                if(!Directory.Exists(folder)) return; 
                string[] files = Directory.GetFileSystemEntries(folder,"*.*" ,SearchOption.AllDirectories); 
                foreach(string file in files)
                {                    
                    if(ExcludeFile(file)) continue;
                    Debug.Print(file + " \n" );
                    if(index.ContainsKey(file.Replace(folder, "."))) 
                    {
                        var item = index.GetValueOrDefault(file);
                        DateTime lastMod = File.GetLastWriteTime(file);
                        if (lastMod != item.UpdateTime)
                        {
                            item.IsChanged = true;                            
                        }
                    }
                    else
                    {
                        IndexItem item = new IndexItem();
                        item.Base = folder;
                        item.Path = file.Replace(folder, ".").Replace(@"\","/");
                        item.FileHash = GetHash(folder, file);
                        item.IsChanged = false;
                        item.IsFolder = File.GetAttributes(file).HasFlag(FileAttributes.Directory);
                        item.IsEmpty = item.IsFolder && IsDirectoryEmpty(file);
                        item.UpdateTime = DateTime.Now;
                        index.Add(item.Path, item);
                    }                    
                }
                
            } );
            return index;
        }

        private bool ExcludeFile(string file)
        {            
            if(!File.GetAttributes(file).HasFlag(FileAttributes.Directory))
            {
                long size = new FileInfo(file).Length;
                if (size > conf.SizeLimit * 1024 * 1024) return true;
            }

            if(conf.IgnoredPath.Count ==0) return false;
            foreach(var exc in conf.IgnoredPath)
            {
                if(string.IsNullOrEmpty(exc)) return false;                   
                if(file.Replace(@"\","/").Contains(exc)) return true;                
            }
            return false;
        }

        private string GetHash(string folder, string file)
        {
            
            if(File.GetAttributes(file).HasFlag(FileAttributes.Directory))
            {
                return file.Replace(folder, "").Replace(@"\","/").GetHashCode().ToString();
            }
            else
            {
                using (var md5 =  System.Security.Cryptography.MD5.Create())
                {
                    using (var stream = File.OpenRead(file))
                    {
                        return md5.ComputeHash(stream).ToString();
                    }
                }
            }
        }
        public bool IsDirectoryEmpty(string path)
        {
            //if(File.GetAttributes(file).HasFlag(FileAttributes.Directory) && !Directory.EnumerateFileSystemEntries(file).Any() )
            IEnumerable<string> items = Directory.EnumerateFileSystemEntries(path);
            using (IEnumerator<string> en = items.GetEnumerator())
            {
                return !en.MoveNext();
            }
            
        }
    }
}