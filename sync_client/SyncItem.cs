using System;

namespace sync_client
{
    [Serializable]
    public class SyncItem
    {
        public IndexItem IndexItem;

        public SyncItem(IndexItem item)
        {
            this.IndexItem = item;
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

    public enum CommandEnum 
    {
        get_server_index,
        update_server_index,
        empty_command
    }
    
}