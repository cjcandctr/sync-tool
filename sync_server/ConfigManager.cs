using System;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace sync_server
{
    public class ConfigManager
    {
        public bool SSL {get; private set;}
        public int Port {get; private set;}
        public List<string> StorageLocation {get; private set;}
        public string User {get; private set;}
        public string Pass {get; private set;}
        public int ConnectionLIMIT { get; internal set; }

        public ConfigManager()
        {
            StorageLocation = new List<string>();
            ReadConfig();
        }

        private void ReadConfig()
        {
            this.SSL = false;
            Port = 8001;
            StorageLocation.Add("./mock-data");
            User = "admin";
            Pass = "admin";
            ConnectionLIMIT = 1;
        }
    }
}