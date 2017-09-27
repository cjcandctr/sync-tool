using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace sync_server
{
    public class ConfigManager
    {
        public bool SSL {get; set;}
        public int Port {get; set;}
        public string StorageLocation {get; set;}
        public string User {get; set;}
        public string Pass {get;  set;}
        public int ConnectionLIMIT { get; set; }

        public ConfigManager()
        {            
            ReadConfig();
        }

        private void ReadConfig()
        {
            this.SSL = false;
            Port = 8001;
            StorageLocation ="./mock-data/";
            User = "admin";
            Pass = "admin";
            ConnectionLIMIT = 1;
            try 
            {
                JObject jso = JObject.Parse(System.IO.File.ReadAllText(@"./clientConfig.json"));

                SSL = jso["ssl"].ToObject<bool>();
                Port = jso["port"].ToObject<int>();
                StorageLocation = jso["StorageLocation"].ToObject<string>();
                User = jso["user"].ToObject<string>();
                Pass=jso["pass"].ToObject<string>();
                ConnectionLIMIT = jso["connection_limit"].ToObject<int>();                
            }
            catch(System.Exception ex)
            {                                
                Program.logger.Error("Error loading config file",ex);
            }
        }
    }
}