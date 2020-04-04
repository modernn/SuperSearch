using System.Collections.Generic;
using System.IO;

namespace SuperSearchinator
{
    public class FileCollector
    {
        public List<FileInfo> Files { get; set; }
        public FileCollector(string path)
        {
            Files = new List<FileInfo>();
            AddFiles(path);
        }
        public void AddFiles(string path)//gets the paths for all the files in the specified directory
        {
            if (File.Exists(path))
            {
                FileInfo thisFile = new FileInfo(path);
                if(!Files.Contains(thisFile))
                    Files.Add(thisFile);
            }
            if (Directory.Exists(path))
            {
                foreach (var subpath in Directory.GetFiles(path))
                    AddFiles(subpath);
                foreach (var subpath in Directory.GetDirectories(path))
                    AddFiles(subpath);
            }
        }
    }
}
