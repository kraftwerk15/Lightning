using System;
using System.IO;
using System.Reflection;
using System.IO.Packaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dynamo.Graph.Nodes;

namespace Thunder
{
    /// <summary>
    /// Container for Finding the Revit Version of a Revit File.
    /// </summary>
    public static class IdentifyYear
    {
        /// <summary>
        /// Used to get the Year Version of a Single Revit File. This takes a list of possible folder locations and a project number, or searching criteria, to identify.
        /// </summary>
        /// <param name="FilePath">An array (list) of folders where the Revit files may be located. Attempt to scope to the lowest folders, i.e. do not enter c:/</param>
        /// <param name="prjNum">The nomenclature of what to look for. Example: 14565_ Revit File_ R18.rvt, you should insert 14565 as a string.</param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static List<List<string>> GetRevitYearList(string[] FilePath, string prjNum)
        {
            List<List<string>> cont = new List<List<string>>();
            List<List<string>> BuildYear = new List<List<string>>();
                       
            foreach (var k in FilePath)
            {
                var fileName = GetFilesinFolders(k, prjNum);
                //cont.Add(fileName);
            }
            var fileNames = cont.SelectMany(d => d).ToList();
            foreach (var i in fileNames)
            {
                List<string> Holding = new List<string>();
                List<string> catch1 = BasicFileInfo.Main(i.ToString());
                IEnumerable<string> catch2 = catch1.Where(s => s.StartsWith("Revit Build"));
                IEnumerable<string> catch4 = catch1.Where(s => s.StartsWith("Central Model Path"));
                var catch3 = catch2.Cast<string>().ToList().FirstOrDefault<string>();
                var catch5 = catch4.Cast<string>().ToList().FirstOrDefault<string>();

                Holding.Add(catch3);
                Holding.Add(catch5);

                BuildYear.Add(Holding);
            }

            return BuildYear;
        }

        /// <summary>
        /// Used to get the Year Version of a Single Revit File. This node is used under the larger performing ByFolderPath node as part of the Lightning package.
        /// </summary>
        /// <param name="FilePath">Insert the File Path under which the Revit file is located. This should not include the File Name, just the File Path.</param>
        /// <param name="prjNum">The nomenclature of what to look for. Example: 14565_ Revit File_ R18.rvt, you should insert 14565 as a string.</param>
        /// <returns>This returns a list of successful Single Revit File Year Version.</returns>
        [NodeCategory("Query")]
        public static List<dynamic> GetRevitYearSingle(string FilePath, string prjNum)
        {
            List<dynamic> cont = new List<dynamic>();
            List<dynamic> BuildYear = new List<dynamic>();

            var fileName = GetFilesinFolders(FilePath, prjNum);
            //var flat = fileName.SelectMany(i => i);
            foreach(var item in fileName)
            {
                if (item[0] != null)
                {
                    //var result = item.SelectMany(i => i.ToString()).ToList();
                    var revitInfo = GetRevitInfo(item);
                    cont.Add(revitInfo);
                }
                else
                {
                    cont.Add(null);
                }
            }
            return cont;
        }
            

        internal static List<List<string>> GetFilesinFolders(string FilePath, string prjNum)
        {
            List<List<string>> cont = new List<List<string>>();
            var stop = FilePath.ToString();
            string filePath = "";
            string matchPattern = "";
            //If the string comes in with a slash at the end, assume it is a folder.
            //remove the last 
            if (!stop.EndsWith(@"\"))
            {
                filePath = stop + @"\";
            }
            else
            {
                filePath = stop;
            }
            //if the project number was null, then return error.
            if (string.IsNullOrEmpty(prjNum))
            {
                List<string> n = null;
                cont.Add(n);
            }
            else
            {
                matchPattern = prjNum + "*";
            }
            string ext = ".rvt";
            //var pattern = @"/(Q)([\d]{4})+/gm";
            //var regular = @"/([/d]{5})+/gm";
            //MatchCollection match = Regex.Matches(temp, pattern);
            string path;
            try
            {
                List<string> small = new List<string>();
                List<string> parent = Directory.EnumerateDirectories(filePath, matchPattern).ToList();
                foreach (string item in parent)
                {
                    if (prjNum.StartsWith("Q"))
                    {
                        path = item + @"\";
                    }
                    else if(prjNum.StartsWith("18"))
                    {
                        path = item + @"\" + prjNum + @"_BIM\";
                    }
                    else
                    //else the file is not on the Q: drive and folder structure changed in 2018
                    {
                        path = item + @"\" + prjNum + @"_BIM\" + prjNum + @"_Central";
                    }
                    //List<string> loopFiles = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(s => ext.Contains(Path.GetExtension(s))).ToList();
                    List<string> loopFiles = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(s => ext.Equals(Path.GetExtension(s))).ToList();
                    if (loopFiles.Count > 0)
                    {
                        foreach (var a in loopFiles)
                        {
                            string filename;
                            int position = a.LastIndexOf('.');
                            filename = a.Remove(position);
                            string pattern = @"(\.[0-9]{4}$)";
                            bool m = Regex.IsMatch(filename, pattern);
                            string recovery = @"(\(Recovery\))";
                            bool n = Regex.IsMatch(filename, recovery);
                            if(!m && !n && !string.IsNullOrEmpty(a))
                            {
                                small.Add(a);
                                System.Diagnostics.Debug.WriteLine("Basic File Info 146 : " + a.ToString());
                            }
                        };
                    }
                    else
                    {
                        small.Add(null);
                    };
                    cont.Add(small);

                    break;
                }
                //return cont;
            }
            catch
            {
                List<string> nullFile = new List<string>();
                nullFile.Add(null);
                cont.Add(nullFile);
                //throw new Exception();
            }

            return cont;
        }

        internal static List<dynamic> GetRevitInfo(List<string> FileName)
        {
            List<dynamic> BuildYear = new List<dynamic>();

            foreach (var i in FileName)
            {
                List<string> Holding = new List<string>();
                List<string> catch1 = BasicFileInfo.Main(i.ToString());
                if (catch1 != null)
                {
                    IEnumerable<string> build = catch1.Where(s => s != null && s.StartsWith("Revit Build"));
                    System.Diagnostics.Debug.WriteLine("Find Revit Build: " + build);
                    IEnumerable<string> path = catch1.Where(s => s != null && s.StartsWith("Central Model Path"));
                    System.Diagnostics.Debug.WriteLine("Find Central Path: " + path);
                    var buildExpand = build.Cast<string>().ToList().FirstOrDefault<string>();
                    var cenMod = path.Cast<string>().ToList().FirstOrDefault<string>();
                    //Revit 2014 (and potentially earlier) do not have a Revit Build parameter, returning 2018.
                    buildExpand = ExtractYear(buildExpand);

                    Holding.Add(buildExpand);
                    Holding.Add(cenMod);

                    BuildYear.Add(Holding);
                }
                else
                {
                    BuildYear.Add(null);
                };
            };
            
            return BuildYear;
        }

        private static string ExtractYear(string orgText)
        {
            //set the result to null and catch errors in this method
            Match result = null;
            string soft;
            //check if incoming text is a null
            if (!string.IsNullOrEmpty(orgText))
            {
                string pattern = @"\d+";
                //if not a null, change to correct year
                result = Regex.Match(orgText, pattern);
                soft = result.ToString();
                System.Diagnostics.Debug.WriteLine(soft);
                return soft;
            }
            else
            {
                return "2018";
            }
            
        }
        /// <summary>
        /// Providing a Central File Path, retrieve the File Size of the File.
        /// </summary>
        /// <param name="centralPath">Central File Path.</param>
        /// <returns>File Size in Kilobytes.</returns>
        [NodeCategory("Query")]
        public static double FileSize(string centralPath)
        {
            long bytes = 0;
            double kilobytes = 0;
            FileInfo fileInfo = new FileInfo(centralPath);
            if (fileInfo.Exists)
            {
                bytes = fileInfo.Length;
                kilobytes = (double)bytes / 1024;
                return kilobytes;
            }
            else
                return 0;
        }
    }

    internal class BasicFileInfo
    {
        private const string StreamName = "BasicFileInfo";

        internal static List<string> Main(string args)
        {
            string pathToRevitFile = args;
            System.Diagnostics.Debug.WriteLine("Path: " + pathToRevitFile);
            List<string> infoData = new List<string>();
            if (StructuredStorageUtils.IsFileStucturedStorage(pathToRevitFile))
            {
                var rawData = GetRawBasicFileInfo(pathToRevitFile);
                //testing here
                if (rawData != null)
                {
                    
                    var rawString = System.Text.Encoding.Unicode.GetString(
                      rawData);

                    var fileInfoData = rawString.Split(
                      new string[] { "\0", "\r\n" },
                      StringSplitOptions.RemoveEmptyEntries);

                    foreach (var info in fileInfoData)
                    {
                        //Console.WriteLine(info);
                        infoData.Add(info);
                    };
                    return infoData;
                }
                else
                {
                    infoData.Add(null);
                    return infoData;
                };
            }
            else
            {
                infoData.Add(null);
                return infoData;
                //throw new NotSupportedException("File is not a structured storage file");
            };
        }

        private static byte[] GetRawBasicFileInfo(string revitFileName)
        {
            if (StructuredStorageUtils.IsFileStucturedStorage(revitFileName))
            {
                StructuredStorageRoot ssRoot = new StructuredStorageRoot(revitFileName);
                if (ssRoot.BaseRoot != null)
                {
                    if (ssRoot.BaseRoot.StreamExists(StreamName))
                    {
                        StreamInfo imageStreamInfo =
                        ssRoot.BaseRoot.GetStreamInfo(StreamName);

                        using (Stream stream = imageStreamInfo.GetStream(
                          FileMode.Open, FileAccess.Read))
                        {
                            byte[] buffer = new byte[stream.Length];
                            stream.Read(buffer, 0, buffer.Length);
                            return buffer;
                        }
                    }
                    else
                    {
                        return null;
                    };
                    //throw new NotSupportedException(string.Format("File doesn't contain {0} stream", StreamName));
                }
                else
                {
                    return null;
                };
                //will not run when someone has the file open.
                //using (StructuredStorageRoot ssRoot = new StructuredStorageRoot(revitFileName))
                //{
                    //if (ssRoot.BaseRoot.StreamExists(StreamName) && ssRoot.BaseRoot != null)
                    //{
                    //    StreamInfo imageStreamInfo =
                    //    ssRoot.BaseRoot.GetStreamInfo(StreamName);

                    //    using (Stream stream = imageStreamInfo.GetStream(
                    //      FileMode.Open, FileAccess.Read))
                    //    {
                    //        byte[] buffer = new byte[stream.Length];
                    //        stream.Read(buffer, 0, buffer.Length);
                    //        return buffer;
                    //    }
                    //}
                    //else
                    //{
                    //    return null;
                    //}
                        //throw new NotSupportedException(string.Format("File doesn't contain {0} stream", StreamName));
                        
            }
            else
            {
                throw new NotSupportedException(
                  "File is not a structured storage file");
            }

        }
    }


    internal static class StructuredStorageUtils
    {
        [DllImport("ole32.dll")]
        private static extern int StgIsStorageFile([MarshalAs( UnmanagedType.LPWStr )] string pwcsName);

        public static bool IsFileStucturedStorage(
          string fileName)
        {
            int res = StgIsStorageFile(fileName);
            System.Diagnostics.Debug.WriteLine(res);
            if (res == 0)
                return true;
            else if (res == 1)
                return false;
            else
                return false;

            throw new FileNotFoundException(
              "File not found", fileName);
        }
    }

    internal class StructuredStorageException : Exception
    {
        public StructuredStorageException()
        {
        }

        public StructuredStorageException(string message)
          : base(message)
        {
        }

        public StructuredStorageException(
          string message,
          Exception innerException)
          : base(message, innerException)
        {
        }
    }

    internal class StructuredStorageRoot : IDisposable
    {
        private StorageInfo _storageRoot;

        public StructuredStorageRoot(Stream stream)
        {
            try
            {
                _storageRoot
                  = (StorageInfo)InvokeStorageRootMethod(
                    null, "CreateOnStream", stream);
            }
            catch (Exception ex)
            {
                throw new StructuredStorageException(
                  "Cannot get StructuredStorageRoot", ex);
            }
        }

        public StructuredStorageRoot(string fileName)
        {
            try
            {
                _storageRoot
                  = (StorageInfo)InvokeStorageRootMethod(
                    null, "Open", fileName, FileMode.Open,
                    FileAccess.Read, FileShare.Read);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                //throw new StructuredStorageException("Cannot get StructuredStorageRoot", ex);
            }
        }

        private static object InvokeStorageRootMethod(
          StorageInfo storageRoot,
          string methodName,
          params object[] methodArgs)
        {
            Type storageRootType
              = typeof(StorageInfo).Assembly.GetType(
                "System.IO.Packaging.StorageRoot",
                true, false);

            object result = storageRootType.InvokeMember(
              methodName,
              BindingFlags.Static | BindingFlags.Instance
              | BindingFlags.Public | BindingFlags.NonPublic
              | BindingFlags.InvokeMethod,
              null, storageRoot, methodArgs);

            return result;
        }

        private void CloseStorageRoot()
        {
            InvokeStorageRootMethod(_storageRoot, "Close");
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            CloseStorageRoot();
        }

        #endregion

        public StorageInfo BaseRoot
        {
            get { return _storageRoot; }
        }
    }

}
