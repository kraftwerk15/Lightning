using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace Snow
{
    public static class Directory
    {
        public static List<string> GetFilesInThisFolder(string Directory, string Extension = "")
        {
            List<string> holding = new List<string>();
            if (Extension == "")
            {
                List<string> loopFiles = System.IO.Directory.EnumerateFiles(Directory, "*.*", SearchOption.TopDirectoryOnly).ToList();
                foreach (var k in loopFiles)
                {
                    holding.Add(k);
                }
            }
            else
            {
                List<string> loopFiles = System.IO.Directory.EnumerateFiles(Directory, "*.*", SearchOption.TopDirectoryOnly).Where(s => Extension.Equals(Path.GetExtension(s))).ToList();
                foreach (var k in loopFiles)
                    holding.Add(k);
            }
            return holding;
        }

        public static List<string> GetFilesInAllFolders(string Directory, string Extension = "")
        {
            List<string> holding = new List<string>();
            if (Extension == "")
            {
                List<string> loopFiles = System.IO.Directory.EnumerateFiles(Directory, "*.*", SearchOption.AllDirectories).ToList();
                foreach(var k in loopFiles)
                {
                    holding.Add(k);
                }
            }
            else
            {
                List<string> loopFiles = System.IO.Directory.EnumerateFiles(Directory, "*.*", SearchOption.AllDirectories).Where(s => Extension.Equals(Path.GetExtension(s))).ToList();
                foreach (var k in loopFiles)
                    holding.Add(k);
            }
            return holding;
        }

        public static List<string> GetFoldersinFolder(string Directory)
        {
            List<string> parent = System.IO.Directory.EnumerateDirectories(Directory).ToList();
            return parent;
        }
    }
}
