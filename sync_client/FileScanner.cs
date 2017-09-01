using System.Collections.Generic;

namespace sync_client
{
    public class FileScanner
    {
        public FileScanner()
        {
            List<string> scanBase = new List<string>();
            scanBase.Add(@"C:\sLeonFiles\PersonalInfo");
            List<string> excludeFile = new List<string>();
            excludeFile.Add(@"C:\sLeonFiles\PersonalInfo\ML");
            excludeFile.Add(@"C:\sLeonFiles\PersonalInfo\MyPic");
            excludeFile.Add(@"C:\sLeonFiles\PersonalInfo\Podcast");

            
        }
    }
}