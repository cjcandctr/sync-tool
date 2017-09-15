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
            FileScanner scn = new FileScanner();
            var tuple = scn.Scan();
            UpdateLocal(tuple.Item1);
            UpdateServer(tuple.Item2);
            return;
                        
        }

        private static async void UpdateServer(List<SyncItem> item2)
        {
            //throw new NotImplementedException();
        }

        private static async void UpdateLocal(List<SyncItem> item1)
        {

            //throw new NotImplementedException();
        }

    }
}
