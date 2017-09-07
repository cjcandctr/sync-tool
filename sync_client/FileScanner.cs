using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace sync_client
{
    public class FileScanner
    {
        Dictionary<string, SyncIndexItem> localIndex = null;
        Dictionary<string, SyncIndexItem> serverIndex = null;
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

        public List<SyncItem> Scan()
        {
            localIndex = UpdateIndex(localIndex, scanBase, excludeFile);
            serverIndex = GetServerIndex();
            return BuildSyncItem(localIndex, serverIndex);
        }

        private List<SyncItem> BuildSyncItem(Dictionary<string, SyncIndexItem> localIndex, Dictionary<string, SyncIndexItem> serverIndex)
        {
            throw new NotImplementedException();
        }

        private Dictionary<string, SyncIndexItem> GetServerIndex()
        {
            throw new NotImplementedException();
        }

        private Dictionary<string, SyncIndexItem> UpdateIndex(Dictionary<string, SyncIndexItem> index, List<string> scanBase, List<string> excludeFile)
        {
            
            if(index == null) index = new Dictionary<string, SyncIndexItem>();
            System.Threading.Tasks.Parallel.ForEach(scanBase, folder=>{ 
                string[] files = Directory.GetFileSystemEntries(folder,"*.*" ,SearchOption.AllDirectories); 
                foreach(string file in files)
                {
                    if(excludeFile.Contains(file)) continue;
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
                        SyncIndexItem item = new SyncIndexItem();
                    }
                    //if(File.GetAttributes(file).HasFlag(FileAttributes.Directory) && !Directory.EnumerateFileSystemEntries(file).Any() )
                    Debug.Print(file + " " );
                }
                return;
            } );
            return null;
        }
    }
}