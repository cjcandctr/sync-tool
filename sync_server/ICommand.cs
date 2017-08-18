namespace sync_server
{
    interface ICommand 
    {
        string Name{get;set;}
        void Execute();

        System.DateTime OperationStamp {get; }
    }
}