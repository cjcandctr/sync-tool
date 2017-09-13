using System;

namespace sync_client
{
    [Serializable]
    public class IndexItem
    {
        public string Path {get; set;}
        public bool IsFolder{get;set;}
        public bool IsEmpty{get;set;}
        public DateTime UpdateTime {get;set;}
        public bool IsChanged {get;set;}
        public string FileHash {get;set;}
        public bool IsDeleted{get;set;}
        public string Base { get; internal set; }
    }
}