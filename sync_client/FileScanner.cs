using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace sync_client
{
    public class FileScanner
    {
        Dictionary<string, IndexItem> localIndex = null;
        Dictionary<string, IndexItem> serverIndex = null;
        List<string> scanBase = null;
        List<string> excludeFile = null;
        public FileScanner()
        {
            scanBase = new List<string>();
            scanBase.Add(@"C:\Temp\delete");
            excludeFile = new List<string>();
            // excludeFile.Add(@"C:\sLeonFiles\PersonalInfo\ML");
            // excludeFile.Add(@"C:\sLeonFiles\PersonalInfo\MyPic");
            // excludeFile.Add(@"C:\sLeonFiles\PersonalInfo\Podcast");
            excludeFile.Add(@"C:\Temp\delete\PropertyInfo_dev_20170707.5");

        }

        public Tuple<List<SyncItem>,List<SyncItem>> Scan()
        {
            localIndex = UpdateIndex(localIndex, scanBase, excludeFile);
            serverIndex = GetServerIndex();
            return BuildSyncItem(localIndex, serverIndex);
        }

        private Tuple<List<SyncItem>,List<SyncItem>> BuildSyncItem(Dictionary<string, IndexItem> localIndex, Dictionary<string, IndexItem> serverIndex)
        {
            if(localIndex == null) throw new Exception("cannot build local file index");
            if(serverIndex == null) throw new Exception("cannot reviceve remote file index");

            List<SyncItem> syncsLocal = new List<SyncItem>();
            List<SyncItem> syncsServer = new List<SyncItem>();
            foreach(var localPair in localIndex)
            {       
                if(serverIndex.ContainsKey(localPair.Key))         
                {
                    var localItem = localPair.Value;
                    var serverItem = serverIndex.GetValueOrDefault(localPair.Key);
                    if(localItem.IsChanged && !serverItem.IsChanged)
                    {
                        SyncItem si = new SyncItem();
                        
                        syncsServer.Add(si);
                    }
                    if(!localItem.IsChanged && serverItem.IsChanged)
                    {
                        SyncItem si = new SyncItem();
                        syncsLocal.Add(si);
                    }
                    if(localItem.IsChanged && serverItem.IsChanged)
                    {
                        
                    }
                }
                
            }
            
            return new Tuple<List<SyncItem>, List<SyncItem>>(syncsLocal,syncsServer);
        }

        private Dictionary<string, IndexItem> GetServerIndex()
        {
            return new Dictionary<string, IndexItem>();
        }

        private Dictionary<string, IndexItem> UpdateIndex(Dictionary<string, IndexItem> index, List<string> scanBase, List<string> excludeFile)
        {
            
            if(index == null) index = new Dictionary<string, IndexItem>();
            System.Threading.Tasks.Parallel.ForEach(scanBase, folder=>{ 
                string[] files = Directory.GetFileSystemEntries(folder,"*.*" ,SearchOption.AllDirectories); 
                foreach(string file in files)
                {                    
                    if(InExcludeFolder(excludeFile, file)) continue;
                    Debug.Print(file + " \n" );
                    if(index.ContainsKey(file)) 
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
                        item.Path = file;
                        item.FileHash = GetHash(file);
                        item.IsChanged = false;
                        item.IsFolder = File.GetAttributes(file).HasFlag(FileAttributes.Directory);
                        item.IsEmpty = item.IsFolder && IsDirectoryEmpty(file);
                        item.UpdateTime = DateTime.Now;
                        index.Add(file, item);
                    }                    
                }
                
            } );
            return index;
        }

        private bool InExcludeFolder(List<string> excludes, string file)
        {
            foreach(var exc in excludes)
            {
                if(file.Contains(exc)) return true;                
            }
            return false;
        }

        private string GetHash(string file)
        {
            string seperator = "::";
            if(File.GetAttributes(file).HasFlag(FileAttributes.Directory))
            {
                return file + seperator +file.GetHashCode().ToString();
            }
            else
            {
                using (var md5 =  System.Security.Cryptography.MD5.Create())
                {
                    using (var stream = File.OpenRead(file))
                    {
                        return file + seperator + md5.ComputeHash(stream).ToString();
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