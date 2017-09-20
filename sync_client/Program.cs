using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace sync_client
{
    class Program
    {
        static void Main(string[] args)
        {
            SyncClient sc = new SyncClient();
            sc.Start();
        }

    }
}
