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

        List<string> scanBase;
        List<string> ignoredPath;
         int sizeLimit;
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
            localIndex = UpdateIndex(localIndex, scanBase, ignoredPath);
            return localIndex;
            
        }

        

        public Dictionary<string, IndexItem> MockServerIndex()
        {
            var serverIndex = new Dictionary<string, IndexItem>();      
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
        public Dictionary<string, IndexItem> MockServerindexUpdate()
        {
            var updateIndex = new Dictionary<string, IndexItem>();      
            IndexItem item = new IndexItem();
            string folder = @"C:\Temp\delete2";
            string file = folder + "/mmexport1505092189115.jpg";
            item.Base = folder;
            item.Path = file.Replace(folder, ".").Replace(@"\","/");
            item.FileHash = GetHash(folder, file);
            item.IsChanged = true;
            item.IsFolder = File.GetAttributes(file).HasFlag(FileAttributes.Directory);
            item.IsEmpty = item.IsFolder && IsDirectoryEmpty(file);
            item.UpdateTime = DateTime.Now;  
            updateIndex.Add(item.Path,item);            
            
            return updateIndex;
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
                    if(index.ContainsKey(file.Replace(@"\","/"))) 
                    {
                        var item = index.GetValueOrDefault(file.Replace(@"\","/"));
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
                        item.ServerBase = folder.Replace(":","");
                        item.Path = file.Replace(folder, ".").Replace(@"\","/");
                        item.FileHash = GetHash(folder, file);
                        item.IsChanged = false;
                        item.IsFolder = File.GetAttributes(file).HasFlag(FileAttributes.Directory);
                        item.IsEmpty = item.IsFolder && IsDirectoryEmpty(file);
                        item.UpdateTime = DateTime.Now;
                        index.Add(file.Replace(@"\","/"), item);
                    }                    
                }
                
            } );
            return index;
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