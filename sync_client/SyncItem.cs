using System;

namespace sync_client
{
    [Serializable]
    public class SyncItem
    {
        private IndexItem serverItem;

        public SyncItem(IndexItem serverItem)
        {
            this.serverItem = serverItem;
        }
        public SyncChangeType ChangeType {get;set;} 
    }

    public enum SyncChangeType 
    {
        Create,
        Update,
        Delete,
        Conflict        
    }
    
}