using System;

namespace sync_client
{
    [Serializable]
    public class SyncItem
    {
        private IndexItem indexItem;

        public SyncItem(IndexItem item)
        {
            this.indexItem = item;
        }
        public SyncChangeType ChangeType {get;set;} 
        public byte[] Data {get;set;}
    }

    public enum SyncChangeType 
    {
        Create,
        Update,
        Delete,
        Conflict        
    }
    
}