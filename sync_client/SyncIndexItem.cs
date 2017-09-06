using System;

namespace sync_client
{
    public class SyncIndexItem
    {
        public string Path {get; set;}
        public DateTime UpdateTime {get;set;}
        public bool IsChanged {get;set;}
        public string FileHash {get;set;}
    }
}