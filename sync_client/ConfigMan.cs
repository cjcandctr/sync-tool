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


        public ConfigMan()
        {
            this.ScanBase = new List<string>();
            this.IgnoredPath = new List<string>();
            this.SizeLimit = -1;
            this.ExcludeType = new List<string>();

            try 
            {
                JObject jso = JObject.Parse(File.ReadAllText(@"./clientConfig.json"));

                AddToList(ScanBase, jso["scan_base"]);
                AddToList(IgnoredPath, jso["ignored_path"]);
                AddToList(ExcludeType, jso["exclude_type"]);
                SizeLimit = jso["size_limit_MB"].ToObject<int>();
                
            }
            catch(System.Exception)
            {
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