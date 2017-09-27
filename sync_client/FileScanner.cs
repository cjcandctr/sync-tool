using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace sync_client
{
    //TODO use FileSystemWatcher to listen file change
    //TODO Serialize index
    public class FileScanner
    {
        public Dictionary<string, IndexItem> FullIndex = new Dictionary<string, IndexItem>();
        public Dictionary<string, IndexItem> UpdatedIndex = null;
        List<string> scanBase;
        List<string> ignoredPath;        
        int sizeLimit;
        public String ServerStorageBase = "";
        public FileScanner(List<string> scanBase, List<string> ignoredPath, int sizeLimit)
        {
            this.scanBase = scanBase;
            if(ignoredPath == null)
            {
                this.ignoredPath = new List<string>();
            } 
            else
            {
                this.ignoredPath = ignoredPath;
            }
            
            this.sizeLimit = sizeLimit;     
        }

        internal FileScanner()
        {
        }

        public Dictionary<string, IndexItem> Scan()
        {
            
            UpdatedIndex = UpdateIndex(FullIndex, scanBase, ignoredPath);            
            return FullIndex;
            
        }

        public Dictionary<string, IndexItem> MockServerIndex()
        {
            var serverIndex = new Dictionary<string, IndexItem>();      
            IndexItem item = new IndexItem();
            string folder = @"C:\Temp\delete2";
            string file = folder + "/HAPSA Issues Log Files.zip";
            item.ClientScanBase = folder;
            item.Name = file.Replace(folder, ".").Replace(@"\","/");
            item.FileHash = GetHash(folder, file);
            item.IsChanged = false;
            item.IsFolder = File.GetAttributes(file).HasFlag(FileAttributes.Directory);
            item.IsEmpty = item.IsFolder && IsDirectoryEmpty(file);
            item.UpdateTime = DateTime.Now;            
            serverIndex.Add(item.Name,item);
            item = new IndexItem();
            file = folder + "/mmexport1505092189115.jpg";
            item.ClientScanBase = folder;
            item.Name = file.Replace(folder, ".").Replace(@"\","/");
            item.FileHash = GetHash(folder, file);
            item.IsChanged = false;
            item.IsFolder = File.GetAttributes(file).HasFlag(FileAttributes.Directory);
            item.IsEmpty = item.IsFolder && IsDirectoryEmpty(file);
            item.UpdateTime = DateTime.Now;  
            serverIndex.Add(item.Name,item);
            
            
            return serverIndex;
        }
        public Dictionary<string, IndexItem> MockServerindexUpdate()
        {
            var updateIndex = new Dictionary<string, IndexItem>();      
            IndexItem item = new IndexItem();
            string folder = @"C:\Temp\delete2";
            string file = folder + "/mmexport1505092189115.jpg";
            item.ClientScanBase = folder;
            item.Name = file.Replace(folder, ".").Replace(@"\","/");
            item.FileHash = GetHash(folder, file);
            item.IsChanged = true;
            item.IsFolder = File.GetAttributes(file).HasFlag(FileAttributes.Directory);
            item.IsEmpty = item.IsFolder && IsDirectoryEmpty(file);
            item.UpdateTime = DateTime.Now;  
            updateIndex.Add(item.Name,item);            
            
            return updateIndex;
        }

        private Dictionary<string, IndexItem> UpdateIndex(Dictionary<string, IndexItem> index, List<string> scanBase, List<string> excludeFile)
        {                        
            Program.logger.Info("Scanning local file");
            System.Threading.Tasks.Parallel.ForEach(scanBase, folder=>{
                if(!Directory.Exists(folder)) return;    
                UpdatedIndex = new Dictionary<string, IndexItem>();
                string[] files = Directory.GetFileSystemEntries(folder,"*.*" ,SearchOption.AllDirectories); 
                foreach(string file in files)
                {                    
                    if(ExcludeFile(file)) continue;
                    if(File.GetAttributes(file).HasFlag(FileAttributes.Directory)) continue;
                    var unifiledPath = file.Replace(@"\","/");
                    var key = unifiledPath;
                    if(!string.IsNullOrEmpty(ServerStorageBase)) //server scan logic
                    {                        
                        key = unifiledPath.Replace(ServerStorageBase, "");
                        if(key.StartsWith(@"_root_/"))
                        {
                            key = key.Replace(@"_root_/", "/");
                        }
                        else
                        { 
                            key = key.Insert(1,":");                            
                        }
                    }
                    
                    if(index.ContainsKey(key)) 
                    {
                        var item = index.GetValueOrDefault(key);
                        DateTime lastMod = File.GetLastWriteTime(file);
                        var hash = GetHash("", file);
                        if (hash != item.FileHash)
                        {
                            item.IsChanged = true;
                            UpdatedIndex.TryAdd(key,item);                       
                            item.UpdateTime = lastMod;
                            item.FileHash = hash;
                        }
                    }
                    else
                    {
                        IndexItem item = new IndexItem();
                        item.ClientScanBase = folder;
                        if(file.StartsWith('/'))
                        {
                            item.PathInServer= @"_root_/" +folder.Substring(1);
                        }
                        else
                        {
                            item.PathInServer = folder.Replace(":","");
                        }
                        item.RealPath = file;
                        item.Name = unifiledPath.Replace(folder, "");
                        item.FileHash = GetHash(folder, file);
                        item.IsChanged = false;
                        item.IsFolder = File.GetAttributes(file).HasFlag(FileAttributes.Directory);
                        item.IsEmpty = item.IsFolder && IsDirectoryEmpty(file);
                        item.UpdateTime = File.GetLastWriteTime(file);
                        
                        index.TryAdd(key, item);                            
                        UpdatedIndex.TryAdd(key,item);   
                        
                    }                    
                }
                
            } );
            return UpdatedIndex;
        }

        private bool ExcludeFile(string file)
        {            
            if(!File.GetAttributes(file).HasFlag(FileAttributes.Directory) && sizeLimit>0)
            {
                long size = new FileInfo(file).Length;
                if (size > sizeLimit * 1024 * 1024) return true;
            }

            if(ignoredPath.Count ==0) return false;
            foreach(var exc in ignoredPath)
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
                        return BitConverter.ToString(md5.ComputeHash(stream));
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