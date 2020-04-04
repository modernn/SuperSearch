using System.Collections.Generic;
using System.IO;

namespace SuperSearchinator
{
    public class FileCollector
    {
        public List<FileInfo> Files { get; set; }
        public List<string> FilePaths { get; set; }
        public FileCollector(string path)
        {
            Files = new List<FileInfo>();
            FilePaths = new List<string>();
            AddFiles(path);
        }
        public void AddFiles(string path)//gets the paths for all the files in the specified directory
        {
            if (File.Exists(path))
            {
                if(!FilePaths.Contains(path))
                    FilePaths.Add(path);
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
