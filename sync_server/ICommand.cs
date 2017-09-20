namespace sync_server
{
    interface ICommand 
    {
        string Name{get;set;}
        void Execute();

        System.DateTime OperationStamp {get; }
    }
    public enum CommandEnum 
    {
        idle_timeout_command,
        get_server_index,
        update_server_index,
        request_server_file
        
    }
}