using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Graph.Nodes;

namespace Snow
{
    public static class Directory
    {
        /// <summary>
        /// Get a list of files in this folder.
        /// </summary>
        /// <param name="Directory">The directory to search through.</param>
        /// <param name="Extension">Filter by a specific extension.</param>
        /// <returns>A list of files conforming to the criteria provided.</returns>
        /// <search>folder, files, directory, file, enumerate, list</search>
        [NodeCategory("Query")]
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

        /// <summary>
        /// Get files in all folders. Use this if there are nested folders in the directory used and you want to access the files in the nested folders.
        /// </summary>
        /// <param name="Directory">The directory to search through.</param>
        /// <param name="Extension">Filter by a specific extension.</param>
        /// <returns>A list of files conforming to the criteria provided.</returns>
        /// <search>folder, files, directory, file, enumerate, list</search>
        [NodeCategory("Query")]
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

        /// <summary>
        /// Get a list of folders in this folder.
        /// </summary>
        /// <param name="Directory">The directory to search through.</param>
        /// <returns>A list of folders in this folder. This will not return any files.</returns>
        /// <search>folder, files, directory, file, enumerate, list</search>
        [NodeCategory("Query")]
        public static List<string> GetFoldersinFolder(string Directory)
        {
            List<string> parent = System.IO.Directory.EnumerateDirectories(Directory).ToList();
            return parent;
        }
    }
}
