using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using System.Linq;

namespace Thunder
{
    public class Journal
    {
        private Journal() { }

        int counter;
        /// <summary>
        /// Find the current Dynamo version
        /// </summary>
        /// <param name="revitVersion">The version number of Revit (e.g. 2017).</param>
        /// <returns>The first three digits of the active Dynamo version.</returns>
        private static string GetDynamoVersion(dynamic revitVersion)
        {
            string dynVersion;
            if (revitVersion > 2016)
            {
                dynVersion = "";
            }
            else
            {
                // Finding the installed version of Dynamo
                Assembly dynamoCore = Assembly.Load("DynamoCore");
                dynVersion = dynamoCore.GetName().Version.ToString().Substring(0, 3);
            }
            return dynVersion;
        }

        /// <summary>
        /// Returns the Revit version (entered as string, double or integer) as an integer
        /// </summary>
        /// <param name="revitVersion">The version number of Revit (e.g. 2017).</param>
        /// <returns>The Revit version as an integer</returns>
        private static int RevitVersionAsInt(dynamic revitVersion)
        {
            
            int revitVersionAsInt;
            Type t = revitVersion.GetType();
            if (t.Equals(typeof(int)))
            {
                revitVersionAsInt = revitVersion;
            }
            else if (t.Equals(typeof(double)))
            {
                revitVersionAsInt = Convert.ToInt32(revitVersion);
            }
            else if (t.Equals(typeof(string)))
            {
                revitVersionAsInt = int.Parse(revitVersion);
            }
            else
            {
                throw new ArgumentException("Revit Version could not be derived from input", "revitVersion");
            }
            return revitVersionAsInt;
        }

        /// <summary>
        /// Verifies that the given file path is a valid path for usage in journal files
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <param name="paramName">The name of the parameter for the path</param>
        /// <returns>A boolean to indicate success/failure</returns>
        private static bool CheckFilePath(string filePath, string paramName)
        {
            // This regular expression finds all but the following characters:
            // 0-1 a-z A-Z (no accented characters like Umlaute)
            // also _ - + { } ( ) [ ] . : \ , °
            string regexpat = "[^0-9\\u0041-\\u005A\\u0061-\\u007A-_+,°:{}()\\[\\]\\.\\\\ ]";
            Regex regexp = new Regex(regexpat);
            MatchCollection matches;
            matches = regexp.Matches(filePath);
            if (matches.Count > 0)
            {
                List<string> str_matches = new List<string>();
                for (int i = 0; i < matches.Count; i++)
                {
                    if (!str_matches.Contains(matches[i].Value))
                    {
                        str_matches.Add(matches[i].Value);
                    }
                }
                string exmsg = String.Format("The file path [{0}] is not valid because it contains characters not compatible with Revit journal files: ", filePath);
                for (int i = 0; i < str_matches.Count; i++)
                {
                    exmsg += "[" + str_matches[i] + "], ";
                }
                exmsg = exmsg.Replace("[ ]", "[WHITESPACE]");
                exmsg = exmsg.Substring(0, exmsg.Length - 2);
                return false;
                throw new ArgumentException(exmsg, paramName);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Builds the first part of the journal string
        /// </summary>
        /// <param name="debugMode">Should the journal file be run in debug mode?</param>
        /// <returns>The first part of the journal string.</returns>
        private static string BuildJournalStart(bool debugMode = false)
        {
            string journalStart = "'" +
                "Dim Jrn \n" +
                "Set Jrn = CrsJournalScript \n";
            // These two directives can make things easier if needed
            if (debugMode)
            {
                journalStart += "Jrn.Directive \"DebugMode\", \"PerformAutomaticActionInErrorDialog\", 1 \n";
                journalStart += "Jrn.Directive \"DebugMode\", \"PermissiveJournal\", 1 \n";
            }
            return journalStart;
        }

        /// <summary>
        /// Builds the part of the journal string responsible for opening a project
        /// </summary>
        /// <param name="revitFilePath">The path to the Revit file. This can be an .rvt or .rfa file.</param>
        /// <param name="circumventPerspectiveViews">Should the document switch to the default 3D view?</param>
        /// <param name="workshareScenario">Check if in a Worksharing scenario.</param>
        /// <param name="detach">Detach</param>
        /// <param name="openCentral">Open Central Model</param>
        /// <returns>The part of the journal string responsible for opening a project.</returns>
        private static string BuildProjectOpen(string revitFilePath, bool circumventPerspectiveViews = false, bool workshareScenario = false, bool detach = false, bool openCentral = false)
        {
            //IdentifyYear.GetRevitYear()
            //CheckFilePath(revitFilePath, "revitFilePath");
            // Exception if the rvt/rfa file isn't found
            //if (!File.Exists(revitFilePath))
            //{
            //    throw new System.IO.FileNotFoundException();
            //}
            string projectOpen = String.Format("");
            if (!workshareScenario)
            {
                projectOpen += "Jrn.Command \"StartupPage\" , \"Open this project , ID_FILE_MRU_FIRST\" \n" +
                                              "Jrn.Data \"MRUFileName\" , \"{0}\" \n ," + revitFilePath;
            }
            else
            {
                projectOpen += "Jrn.Command \"Ribbon\" , \"Open an existing project, ID_REVIT_FILE_OPEN\" \n";
                if(!openCentral)
                {
                    projectOpen += "Jrn.Data \"FileOpenSubDialog\" , \"OpenAsLocalCheckBox\", \"True\" \n";
                }
                if (detach && !openCentral)
                {
                    projectOpen += "Jrn.Data \"FileOpenSubDialog\" , \"DetachCheckBox\", \"True\" \n";
                }
                if (openCentral && !detach)
                {
                    projectOpen += "Jrn.Data \"FileOpenSubDialog\" , \"OpenAsLocalCheckBox\" , \"False\" \n";
                }
                projectOpen += "Jrn.Data \"FileOpenSubDialog\" , \"AuditCheckBox\" , \"True\" \n" +
                    "Jrn.Data \"TaskDialogResult\" , \"This operation can take a long time. Recommended use includes periodic maintenance of large files and preparation for upgrading to a new release. Do you want to continue?\", \"Yes\", \"IDYES\" \n" +
                    "Jrn.Data \"File Name\" , \"IDOK\", \"" + revitFilePath + "\" \n" +
                    "Jrn.Data \"WorksetConfig\" , \"All\", 0  \n";
                if (detach)
                {
                    projectOpen += "Jrn.Data \"TaskDialogResult\" , \"Detaching this model will create an independent model. You will be unable to synchronize your changes with the original central model.\" & vbLf & \"What do you want to do?\", \"Detach and preserve worksets\", \"1001\" \n";
                }
                projectOpen += "Jrn.Data \"TaskDialogResult\" , \"Revit could not find or read 1 references.\" & vbLf & \"What do you want to do?\" , \"Ignore and continue opening the project\" , \"1002\" \n";
                //"Jrn.Directive \"DocSymbol\" , \"[]\";
            }
            if (circumventPerspectiveViews)
            {
                projectOpen += "Jrn.Command \"Ribbon\" , \"Create a default 3D orthographic view. , ID_VIEW_DEFAULT_3DVIEW\" \n";
            }
            return projectOpen;
        }
        

        /// <summary>
        /// Builds the part of the journal string responsible for launching DynamoRevit
        /// </summary>
        /// <param name="workspacePath">The path to the Dynamo workspace.</param>
        /// <param name="revitVersion">The version number of Revit (e.g. 2017).</param>
        /// <param name="dynVersion">The version number of Dynamo (e.g. 1.3).</param>
        /// <returns>The part of the journal string responsible for launching DynamoRevit.</returns>
        private static string BuildDynamoLaunch(string workspacePath, dynamic revitVersion, string dynVersion)
        {
            CheckFilePath(workspacePath, "workspacePath");
            // Exception if the dyn file isn't found
            if (!File.Exists(workspacePath))
            {
                throw new FileNotFoundException();
            }
            string launchDynamo = "";
            // Launch command and journal keys differ between pre-2017 and post-2016 Revit
            if (revitVersion > 2016)
            {
                // Doesn't work with Dynamo 1.0
                launchDynamo += String.Format("Jrn.Command \"Ribbon\" , \"Launch Dynamo , ID_VISUAL_PROGRAMMING_DYNAMO\" \n" +
                    "Jrn.Data \"APIStringStringMapJournalData\", 5, \"dynPath\", \"{0}\", \"dynShowUI\", \"false\", \"dynAutomation\", \"false\", \"dynPathExecute\", \"true\", \"dynModelShutDown\", \"true\" \n", workspacePath);
            }
            else
            {
                launchDynamo += String.Format("Jrn.RibbonEvent \"Execute external command:CustomCtrl_%CustomCtrl_%Add-Ins%Visual Programming%Dynamo {0}:Dynamo.Applications.DynamoRevit\" \n" +
                    "Jrn.Data \"APIStringStringMapJournalData\", 3, \"dynPath\", \"{1}\", \"dynShowUI\", \"false\", \"dynAutomation\", \"true\" \n", dynVersion, workspacePath);
                // This command must only be called in pre-2017 Revit versions
                launchDynamo += "Jrn.Command \"Internal\" , \"Flush undo and redo stacks , ID_FLUSH_UNDO\" \n";
            }
            return launchDynamo;
        }

        /// <summary>
        /// Builds the part of the journal string responsible for purging the model
        /// </summary>
        /// <returns>The part of the journal string responsible for purging the model.</returns>
        private static string BuildProjectPurge()
        {
            string projectPurge = "";
            // Execute Purge three times in a row to make sure that *everything* has been purged
            for (int i = 0; i < 3; i += 1)
            {
                projectPurge += "Jrn.Command \"Ribbon\" , \"Purge(delete) unused families and types, ID_PURGE_UNUSED\" \n";
                projectPurge += "Jrn.PushButton \"Modal , Purge unused, Dialog_Revit_PurgeUnusedTree\" , \"OK, IDOK\" \n";
                projectPurge += "Jrn.Data \"Transaction Successful\" , \"Purge unused\" \n";
            }
            return projectPurge;
        }

        /// <summary>
        /// Builds the part of the journal string responsible for saving a project
        /// </summary>
        /// <returns>The part of the journal string responsible for saving a project.</returns>
        private static string BuildProjectSave()
        {
            string projectSave = "Jrn.Command \"Ribbon\" , \"Save the active project , ID_REVIT_FILE_SAVE\" \n";
            return projectSave;
        }

        /// <summary>
        /// Builds the part of the journal string responsible for closing a project
        /// </summary>
        /// <returns>The part of the journal string responsible for closing a project.</returns>
        private static string BuildProjectClose()
        {
            string projectClose = "Jrn.Command \"Internal\" , \"Close the active project , ID_REVIT_FILE_CLOSE\" \n";
            return projectClose;
        }

        /// <summary>
        /// Builds the last part of the journal string
        /// </summary>
        /// <returns>The last part of the journal string.</returns>
        private static string BuildJournalEnd()
        {
            string journalEnd = "Jrn.Command \"SystemMenu\" , \"Quit the application; prompts to save projects , ID_APP_EXIT\"";
            return journalEnd;
        }

        /// <summary>
        /// Deletes the journal file if it already exists.
        /// </summary>
        /// <param name="journalFilePath">The path of the generated journal file.</param>
        private static void DeleteJournalFile(string journalFilePath)
        {
            if (File.Exists(journalFilePath))
            {
                File.Delete(journalFilePath);
            }
            return;
        }

        /// <summary>
        /// Writes the journal file.
        /// </summary>
        /// <param name="journalFilePath">The path of the generated journal file.</param>
        /// <param name="journalString">The string for the journal file.</param>
        /// <param name="modelYear">The Year of Revit the Jounral File will open.</param>
        /// <param name="prjNum">The Project Number will be written to the Journal File Name.</param>
        private static string WriteJournalFile(string journalFilePath, string journalString, string prjNum = null, int modelYear = 2018)
        {
            string FilePath;
            var time = DateTime.UtcNow.Date.ToString("yyyy.MM.dd");
            char last = journalFilePath[journalFilePath.Length - 1];
            string pathWithTrailingSlash = journalFilePath.EndsWith(@"\") ? journalFilePath : journalFilePath + @"\";
            //if (!last.Equals(@"\\"))
            //{
            //    journalFilePath = journalFilePath + @"\";
            //}
            Directory.CreateDirectory(journalFilePath + time + @"\" + modelYear);
            File.Copy(@"C:\ProgramData\Autodesk\Revit\Addins\" + modelYear + @"\Dynamo.addin", journalFilePath + time + @"\" + modelYear + @"\Dynamo.addin", true);
            if (string.IsNullOrEmpty(journalFilePath))
                throw new FileNotFoundException("No Journal Path was added to place the Journal Files.");
            else
            {
                var tempFilePath = ""; 
                int count = 0;
                FilePath = journalFilePath + time + @"\" + modelYear + @"\" + prjNum + "_" + count + @".txt";
                if (File.Exists(FilePath))
                {
                    do
                    {
                        count++;
                        tempFilePath = journalFilePath + time + @"\" + modelYear + @"\" + prjNum + "_" + count + @".txt";
                    }
                    while (File.Exists(tempFilePath));
                    FilePath = tempFilePath;
                    
                }
                
            }
            var tw = new StreamWriter(FilePath, true);
            tw.Write(journalString);
            tw.Flush();
            return FilePath;
        }

        /// <summary>
        /// Create a journal file for purging and subsequently saving a Revit file.
        /// </summary>
        /// <param name="revitFilePath">The path to the Revit file. This can be an .rvt or .rfa file. The path may not contain whitespace or accented characters.</param>
        /// <param name="journalFilePath">The path of the generated journal file.</param>
        /// <returns>The path of the generated journal file.</returns>
        /// <search>purge, model, central, local, clean</search>
        [NodeCategory("Action")]
        public static string PurgeModel(string revitFilePath, string journalFilePath)
        {
            DeleteJournalFile(journalFilePath);
            // Create journal string
            // Journal needs to be in debug mode. 
            // Otherwise the journal playback may stop if there is nothing to purge.
            string journalString = BuildJournalStart(true);
            journalString += BuildProjectOpen(revitFilePath);
            journalString += BuildProjectPurge();
            journalString += BuildProjectSave();
            journalString += BuildProjectClose();
            journalString += BuildJournalEnd();
            // Create journal file
            WriteJournalFile(journalFilePath, journalString);
            return journalFilePath;
        }

        /// <summary>
        /// Create a journal file for purging and subsequently saving multiple Revit files in a single Revit session.
        /// 
        /// </summary>
        /// <param name="revitFilePaths">The paths to the Revit files. These can be .rvt or .rfa files. The paths may not contain whitespace or accented characters.</param>
        /// <param name="journalFilePath">The path of the generated journal file.</param>
        /// <returns>The path of the generated journal file.</returns>
        /// <search>purge, model, central, local, clean</search>
        [NodeCategory("Action")]
        public static string PurgeModels(List<string> revitFilePaths, string journalFilePath)
        {
            DeleteJournalFile(journalFilePath);
            // Create journal string
            string journalString = BuildJournalStart(true);
            // Open, purge, save and close all models
            foreach (string revitFilePath in revitFilePaths)
            {
                journalString += BuildProjectOpen(revitFilePath);
                journalString += BuildProjectPurge();
                journalString += BuildProjectSave();
                journalString += BuildProjectClose();
            }
            journalString += BuildJournalEnd();
            // Create journal file
            WriteJournalFile(journalFilePath, journalString);
            return journalFilePath;
        }
        internal static List<string> YearSwitch(long key, int year, string process)
        {
            List<string> yearList = new List<string>();
            
            string revitEx = @"C:\Program Files\Autodesk\";
            
            switch (year)
            {
                case 2016:
                    revitEx += "Revit Architecture 2016";
                    break;
                case 2017:
                    revitEx += "Revit 2017";
                    break;
                case 2018:
                    revitEx += "Revit 2018";
                    break;
                default:
                    revitEx += "Revit 2018";
                    break;
            }

            revitEx += @"\Revit.exe";

            string processPath = revitEx;
            string args = process;
            //string args = process; " /language ENU " + + " /nosplash"
            yearList.Add(processPath);
            yearList.Add(args);

            return yearList;
        }

        private static string ContentProcess(int i, int prjKey, string Year, List<string> Holding, string CentralPath, string newConn, string SQLSelect)
        {
            string processPath;
            string args;
            //int key = ring + i;
            int year = Convert.ToInt32(Year);
            //int year = Year;
            string journal = Holding[0];
            List<string> temp = YearSwitch(i, year, journal);
            processPath = temp[0];
            args = temp[1];
            DateTime dateEx = DateTime.Now;
            List<int> sqlEarly = new List<int>();
            if(newConn != "" && newConn != null)
            {
                sqlEarly = TidalWave.SQL.InsertInto(newConn, Holding[1], dateEx);
            }
            //returns the runID and the projectTableID to send to the .csv
            double fileSize = IdentifyYear.FileSize(CentralPath);
            string csvEarly = CSVPath(CentralPath);
            if (File.Exists(csvEarly))
                File.Delete(csvEarly);
            if (newConn != "" && newConn != null)
            {
                string sub = CSVWriter(sqlEarly, csvEarly, fileSize, CentralPath);
            }
            else
            {
                List<int> vs = new List<int>();
                vs.Add(0);
                vs.Add(1);
                string sub = CSVWriter(vs, csvEarly, fileSize, CentralPath);
            }
            int exitCode = Process.ByPathAndArguments(processPath, args); 
            //int send = sqlEarly[0];
            if(newConn != "" && newConn != null)
            {
                string select = SQLSelect + sqlEarly[0];
                List<dynamic> SelectComp = TidalWave.SQL.Select(newConn, select);
                if (SelectComp[4] == null || SelectComp[4] == "")
                {
                    TidalWave.SQL.Update(dateEx, sqlEarly[0], newConn, 2);
                    return "Failure";
                }
                else if (SelectComp[4] = true)
                {
                    TidalWave.SQL.Update(dateEx, sqlEarly[0], newConn, 1);
                    return "Success";
                }
                else
                {
                    TidalWave.SQL.Update(dateEx, sqlEarly[0], newConn, 0);
                    return "Failure";
                };
            }
            else
            {
                return exitCode.ToString();
            };
        }
        internal static string CSVPath(string CentralPath)
        {
            string path = Path.GetDirectoryName(CentralPath);
            string result = Path.GetFileNameWithoutExtension(CentralPath);
            string sub = path + @"\" + result + ".csv";
            return sub;
        }
        internal static string CSVWriter(List<int> data, string sub, double fileSize, string filePath)
        {
            using (TextWriter sw = new StreamWriter(sub))
            {
                int rsKey = data[0];
                int psKey = data[1];
                sw.WriteLine("{0},{1},{2},{3}", rsKey, psKey,fileSize, filePath);
                sw.Flush();
            };
            
            return sub;
        }

        internal static List<dynamic> FileConfirmation(List<dynamic> fileName, string workspacePath, string projectNumber, string saveLocation = "", bool debugMode = true, bool circumventPerspectiveViews = false, bool detach = false, bool openCentral = false)
        {
            List<dynamic> Central = new List<dynamic>();
            List<dynamic> Holding = new List<dynamic>();
            List<dynamic> Failed = new List<dynamic>();
            //List<dynamic> YearList = new List<dynamic>();
            List<dynamic> container = new List<dynamic>();
            if (fileName.Count > 0 && fileName[0] != null)
            {
                //fileName should be returned in a List of List, nested list containing the Central Model Path and the Project Number
                foreach (var d in fileName)
                {
                    //var d below is a nested list containing the Central Model Path and the Project Number
                    foreach (var c in d)
                    {
                        //set workshare to bool, this was removed from the Dynamo Automation nodes, so forcing it to false.
                        bool workshare = false;
                        //give a clean list to insert content
                        List<dynamic> tempPrj = new List<dynamic>();
                        List<dynamic> YearList = new List<dynamic>();
                        //YearList.Clear();

                        try
                        {
                            //get the 0 instance of what is in the list, should be the Revit Year
                            string quasi = c[0];
                            //Attempt to parse the string of Year to an Integer
                            int RevitYear;
                            int.TryParse(quasi, out RevitYear);
                            //get the 1 instance of what is in the list, should be the Central Model Path
                            int finding = c[1].IndexOf(":");
                            //document it
                            string path = c[1].Substring(finding + 2);
                            

                            //check if the file is younger than two weeks
                            DateTime modDate = File.GetLastWriteTime(path);
                            //below is to change it from two weeks.
                            DateTime mod1Date = DateTime.Now.AddDays(-14);
                            //compare the dates, older than two weeks returns -1.
                            int compareDate = DateTime.Compare(modDate, mod1Date);
                            //check if the File Path could be found in the files Structured Storage can be found.
                            if (CheckFilePath(path, "revitFilePath") == false && compareDate > 0)
                            {
                                Failed.Add("No File Path was found for: " + path);
                            }
                            //check if the File Path exists and can be accessed
                            else if (!File.Exists(path) && compareDate > 0)
                            {
                                Failed.Add("The File does not exist for: " + path);
                            }
                            //check if the file is older than two weeks
                            else if (compareDate < 0)
                            {
                                Failed.Add("The File is older than the time allotment. " + path);
                            }
                            //if the Central File Path already exists, do nothing
                            else if (Central.Contains(path))
                            {
                                Failed.Add("Duplicate Found. Omitting from Sequence. " + path);
                            }
                            else if (RevitYear < 2017)
                            {
                                Failed.Add("Revit 2016 file that will not run Dynamo 2.0");
                            }
                            //all is true, lets move forward
                            else
                            {
                                //add the Central model path to a List
                                YearList.Add(path);
                                //run a method
                                int modelYear = RevitVersionAsInt(RevitYear);
                                //add the model year to a List
                                YearList.Add(modelYear);
                                //match that with the Dynamo Year
                                string dynVersion = GetDynamoVersion(RevitYear);
                                // Create journal string
                                string journalString = BuildJournalStart(debugMode);
                                //if the path cannot be found, attempt working on a local model
                                if (path == "")
                                    journalString += BuildProjectOpen(c[2], circumventPerspectiveViews, workshare);
                                //else fetch the central model
                                else
                                {
                                    workshare = true;
                                    journalString += BuildProjectOpen(path, circumventPerspectiveViews, workshare, detach, openCentral);
                                }
                                //Add the Dyanmo Build to the Journal Squence with our Workspace we want to run
                                journalString += BuildDynamoLaunch(workspacePath, RevitYear, dynVersion);
                                // In newer Revit versions the slave graph will only run if there are no journal commands after launching Dynamo.
                                // The slave graph will then need to terminte itself.
                                if (RevitYear < 2017)
                                {
                                    journalString += BuildProjectClose();
                                    journalString += BuildJournalEnd();
                                }
                                // Create journal file
                                string journaling = WriteJournalFile(saveLocation, journalString, projectNumber, modelYear);
                                tempPrj.Add(journaling);
                                Debug.WriteLine(journaling);
                                tempPrj.Add(projectNumber);
                                Debug.WriteLine(projectNumber);
                                //Add the journal file to a list for us to work over later out of this loop
                                if(tempPrj != null && YearList != null)
                                {
                                    Holding.Add(tempPrj);
                                    Central.Add(YearList);
                                }
                            }

                        }
                        //something failed miserably. Do not stop, log it and move on to the next item.
                        catch
                        {
                            if(c[1] == null)
                            {
                                string exmsg = String.Format("The file path is null. Cannot operate over null Central Path: Journal 628");
                                Failed.Add(exmsg);
                            }
                            else
                            {
                                string exmsg = String.Format("The file path [{0}] received an error where it could not find the last modified date and time of the file: Signficant Error: ", c[1]);
                                Failed.Add(exmsg);
                            }
                        }
                    }
                }
            }
            //larger methods failed. Log and move on.
            else
            {
                if(fileName.Count == 0 || fileName[0] == null)
                {
                    string exmsg = String.Format("Error in finding folder for File. File was null.");
                    Failed.Add(exmsg);
                }
                else
                {
                    string exmsg = String.Format("Error in finding folder for File [{0}]", fileName[0]);
                    Failed.Add(exmsg);
                };
            };
            container.Add(Central);
            container.Add(Holding);
            container.Add(Failed);
            return container;
        }

        /// <summary>
        /// This will execute a given journal file that was created from other Lightning nodes.
        /// </summary>
        /// <param name="CentralFilePath">A list of Central File Path of the Central Files to operate.</param>
        /// <param name="JournalFilePath">The folder location where the journal file is saved.</param>
        /// <param name="prjKey">If you have a SQLConnection</param>
        /// <param name="SQLConnectionString">A connection string to write run values to a database. This is not required.</param>
        /// <param name="SQLSelect">A selection string for the key.</param>
        /// <param name="multithreading">Pass in the number of concurrent applications to run. Defaults to not run concurrently to save on resources.</param>
        /// <returns name="Status">What the status was when attempting to run the process. If not using SQL, status will return as the exit code from the application, i.e. 0 for successful transaction.</returns>
        /// <returns name="Failed">Attempting to log if there was a failure.</returns>
        /// <search>run, journal, automation, model</search>
        [NodeCategory("Action")]
        [MultiReturn(new[] {"Status", "Failed"})]
        public static Dictionary<string, List<dynamic>> ExecuteJournal(List<List<dynamic>> CentralFilePath, List<List<string>> JournalFilePath, int prjKey = 0, string SQLConnectionString = "", string SQLSelect = "", int multithreading = 0)
        {
            List<dynamic> Status = new List<dynamic>();
            List<dynamic> Failed = new List<dynamic>();

            try
            {
                //If the user selected multithreading and there are Central Files
                if (multithreading >= 1 && CentralFilePath.Count > 0)
                {
                    //Set the number of Processes you want to run parallel
                    ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = multithreading };
                    try
                    {
                        string status;
                        //generate content for the process
                        Parallel.ForEach(CentralFilePath, options, (x, y, i) =>
                        {
                            int key = unchecked((int)i);
                            status = ContentProcess(key, prjKey, CentralFilePath[key][1], JournalFilePath[key], CentralFilePath[key][0], SQLConnectionString, SQLSelect);
                            Status.Add(status);
                        });
                        
                    }
                    catch (Exception ex)
                    {
                        //Failed.Add(Holding[i] + "threw an " + e);
                        throw ex;
                    }
                }
                //then multithreading is not true
                else if (multithreading < 1 && CentralFilePath.Count > 0)
                {
                    for (int i = 0; i < CentralFilePath.Count; i++)
                    {
                        string status = ContentProcess(prjKey, prjKey, CentralFilePath[i][1], JournalFilePath[i], CentralFilePath[i][0], SQLConnectionString, SQLSelect);
                        Status.Add(status);
                    };
                }
                else
                {
                    string exmsg = "The List 'Central' contained no content and is not Valid. Terminating Process";
                    Failed.Add(exmsg);
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new Dictionary<string, List<dynamic>>
            {
                {"Status", Status},
                {"Failed", Failed}
            };
        }
        
        /// <summary>
        /// Create a journal file for executing a Dynamo workspace on a single Revit file.
        /// 
        /// This journal file uses several keys specifically for the purpose of automating Dynamo.
        /// Dynamo is never run in the idle loop. The workspace is executed immediately, and control is returned to the DynamoRevit
        /// external application.
        /// </summary>
        /// <param name="revitFilePath">The path to the Revit file. This can be an .rvt or .rfa file. The path may not contain whitespace or accented characters.</param>
        /// <param name="workspacePath">The path to the Dynamo workspace. The path may not contain whitespace or accented characters.</param>
        /// <param name="journalFilePath">The path of the generated journal file.</param>
        /// <param name="revitVersion">The version number of Revit (e.g. 2017).</param>
        /// <param name="debugMode">Should the journal file be run in debug mode? Set this to true if you expect models to have warnings (e.g. missing links etc.).</param>
        /// <param name="circumventPerspectiveViews">Should the document switch to the default 3D view? Set this to true if you expect models will open with a perspective view as last saved view / starting view.</param>
        /// <param name="workshareScenario">Check if in a Revit Worksharing scenario.</param>
        /// <returns>The path of the generated journal file.</returns>
        /// <search>create, journal, automation, single, path, assessment, test</search>
        [NodeCategory("Create")]
        public static string BySinglePath(string revitFilePath, string workspacePath, dynamic revitVersion, string journalFilePath = "", bool debugMode = false, bool circumventPerspectiveViews = false, bool workshareScenario = false)
        {
            DeleteJournalFile(journalFilePath);
            int revitVersionInt = RevitVersionAsInt(revitVersion);
            string dynVersion = GetDynamoVersion(revitVersionInt);
            // Create journal string
            string journalString = BuildJournalStart(debugMode);
            journalString += BuildProjectOpen(revitFilePath, circumventPerspectiveViews, workshareScenario);
            journalString += BuildDynamoLaunch(workspacePath, revitVersionInt, dynVersion);
            // In newer Revit versions the slave graph will only run if there are no journal commands after launching Dynamo.
            // The slave graph will then need to terminte itself.
            if (revitVersionInt < 2017)
            {
                journalString += BuildProjectClose();
                journalString += BuildJournalEnd();
            }
            // Create journal file
            WriteJournalFile(journalFilePath, journalString);
            return journalFilePath;
        }

        /// <summary>
        /// Create a journal file for executing a Dynamo workspace on a Revit file through a list of files.
        /// 
        /// This journal file uses several keys specifically for the purpose of automating Dynamo.
        /// Dynamo is never run in the idle loop. The workspace is executed immediately, and control is returned to the DynamoRevit
        /// external application. This process will find the Revit Version to open for the particular file.
        /// </summary>
        /// <param name="workspacePath">The path to the Dynamo workspace. The path may not contain whitespace or accented characters.</param>
        /// <param name="FolderLocations">The level of Folder Locations to loop through to find the Revit Files.</param>
        /// <param name="saveLocation">The path where the journal file will be generated.</param>
        /// <param name="debugMode">Should the journal file be run in debug mode? Set this to true if you expect models to have warnings (e.g. missing links etc.).</param>
        /// <param name="circumventPerspectiveViews">Should the document switch to the default 3D view? Set this to true if you expect models will open with a perspective view as last saved view / starting view.</param>
        /// <returns name="Central File Path">A completed process. Returns a Lists of Central File Path, Journal File Path, Time to Find Files, and Failed Descriptions if files are not found.</returns>
        /// <returns name="Journal File Path">A list of Journal file paths that were created in this node.</returns>
        /// <returns name="Find Time">The time elapsed in finding these files.</returns>
        /// <returns name="Failed Message">If there was Failures, logging them for use later.</returns>
        /// <search>journal, file, automation, assessment, test, automatic</search>
        [NodeCategory("Create")]
        [MultiReturn(new[] {"Central File Path","Journal File Path","Find Time","Failed Message"})]
        public static Dictionary<string, List<dynamic>> ByFolderPath(string workspacePath, List<List<string>> FolderLocations, string saveLocation, bool debugMode = true, bool circumventPerspectiveViews = false)
        {
            Stopwatch counter = Stopwatch.StartNew();
            List<dynamic> Central = new List<dynamic>();
            List<dynamic> Holding = new List<dynamic>();
            List<dynamic> Failed = new List<dynamic>();
            List<dynamic> container = new List<dynamic>();
            DeleteJournalFile(saveLocation);
            
            foreach (var item in FolderLocations)
            {
                //Next Line will loop through a folder location and return the Revit Files that exists there.
                //It returns the Year(Build) the file is saved in and also if it is connected to a Central Model.
                var fileName = IdentifyYear.GetRevitYearSingle(item[0],item[1]);
                List<dynamic> cont = new List<dynamic>(FileConfirmation(fileName, workspacePath, item[1], saveLocation, debugMode, circumventPerspectiveViews));
                Central.Add(cont[0]);
                Holding.Add(cont[1]);
                Failed.Add(cont[2]);
            }
            counter.Stop();
            TimeSpan ct = counter.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ct.Hours, ct.Minutes, ct.Seconds, ct.Milliseconds / 10);
            Debug.WriteLine(elapsedTime);
            //List of Central Files
            //foreach (var k in Holding)
            //    Proj.Add(k[0]);
           
            List<dynamic> Find = new List<dynamic>();
            Find.Add(elapsedTime);
            //Return to Dynamo the Container of Everything that Happened.
            return new Dictionary<string, List<dynamic>>
            {
                { "Central File Path",  Central},
                { "Journal File Path", Holding},
                { "Find Time", Find},
                { "Failed Message", Failed}
            };
        }

        /// <summary>
        /// In a parent graph, prepare a journal file for detaching a central model.
        /// </summary>
        /// <param name="workspacePath">The path to the Dynamo workspace. The path may not contain whitespace or accented characters.</param>
        /// <param name="FolderLocations">The level of Folder Locations to loop through to find the Revit Files.</param>
        /// <param name="saveLocation">The Save Location of the Journal files created.</param>
        /// <param name="detach">Whether the file should be detached.</param>
        /// <param name="openCentral">Whether the central model should be opened directly.</param>
        /// <param name="debugMode">Should the journal file be run in debug mode? Set this to true if you expect models to have warnings (e.g. missing links etc.).</param>
        /// <param name="circumventPerspectiveViews">Should the document switch to the default 3D view? Set this to true if you expect models will open with a perspective view as last saved view / starting view.</param>
        /// <returns name="Central File Path">A list of Central Files Found during this run.</returns>
        /// <returns name="Journal File Path">A list of Journal Files created during this run.</returns>
        /// <returns name="Run Time">The time necessary to complete this node.</returns>
        /// <returns name="Failed Message">A list of Failure Messages hopefully to help you identify why that file did not complete.</returns>
        /// <search>run, journal, test, detach, model, central</search>
        [NodeCategory("Create")]
        [MultiReturn(new[] {"Central File Path","Journal File Path","Run Time", "Failed Message"})]
        public static Dictionary<string, List<dynamic>> DetachModel(string workspacePath, List<List<string>> FolderLocations, string saveLocation, bool detach, bool openCentral, bool debugMode = true, bool circumventPerspectiveViews = true)
        {
            Stopwatch counter = Stopwatch.StartNew();
            List<dynamic> Central = new List<dynamic>();
            List<dynamic> Holding = new List<dynamic>();
            List<dynamic> Failed = new List<dynamic>();
            List<dynamic> container = new List<dynamic>();
            DeleteJournalFile(saveLocation);
            counter.Start();
            foreach(var item in FolderLocations)
            {
                var fileName = IdentifyYear.GetRevitYearSingle(item[0],item[1]);
                List<dynamic> cont = new List<dynamic>(FileConfirmation(fileName, workspacePath, item[1], saveLocation, debugMode, circumventPerspectiveViews, detach, openCentral));
                Central.Add(cont[0]);
                Holding.Add(cont[1]);
                Failed.Add(cont[2]);
            }
            counter.Stop();
            TimeSpan ct = counter.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ct.Hours, ct.Minutes, ct.Seconds, ct.Milliseconds / 10);
            List<dynamic> Run = new List<dynamic>
            {
                elapsedTime
            };
            //Return to Dynamo the Container of Everything that Happened.
            return new Dictionary<string, List<dynamic>>
            {
                { "Central File Path", Central},
                { "Journal File Path", Holding},
                { "Run Time", Run},
                { "Failed Message", Failed}
            };
        }
    }
}
