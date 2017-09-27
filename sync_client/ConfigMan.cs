using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace sync_client
{
    public class ConfigMan
    {
        public List<string> ScanBase {get; set;}
        public List<string> IgnoredPath{get;set;}
        public int SizeLimit {get;set;}
        public List<string> ExcludeType {get;set;}
        public string ServerAddress {get;set;}
        public int ServerPort {get;set;}
        public bool IsSSL {get;set;}
        public int IntervalSec {get;set;}


        public ConfigMan()
        {
            this.ScanBase = new List<string>();
            this.IgnoredPath = new List<string>();
            this.SizeLimit = -1;
            this.ExcludeType = new List<string>();
            ServerAddress ="127.0.0.1";
            ServerPort = 8001;
            IsSSL = false;
            IntervalSec = 30;
            try 
            {
                JObject jso = JObject.Parse(File.ReadAllText(@"./clientConfig.json"));

                AddToList(ScanBase, jso["scan_base"]);
                AddToList(IgnoredPath, jso["ignored_path"]);
                AddToList(ExcludeType, jso["exclude_type"]);
                SizeLimit = jso["size_limit_MB"].ToObject<int>();
                ServerAddress = jso["server_address"].ToObject<string>();
                ServerPort = jso["server_port"].ToObject<int>();      
                IsSSL = jso["use_ssl"].ToObject<bool>();
                IntervalSec = jso["interval_second"].ToObject<int>();
            }
            catch(System.Exception ex)
            {
                Program.logger.Fatal("Error loading config file", ex);
                if(ScanBase.Count==0)
                    ScanBase.Add(@"./");                
            }
        }

        private void AddToList(List<string> list, JToken jToken)
        {
            var jArr = JArray.Parse(jToken.ToString());
            foreach(var path in jArr)
            {
                list.Add(path.ToString());          
            }
        }
    }
}