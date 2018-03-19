using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Dynamo.Graph.Nodes;

namespace Thunder
{
    /// <summary>
    /// Working with Application's Processes.
    /// </summary>
    public class Process
    {
        private Process() { }
        /// <summary>
        /// Run a process and return the exit code. Used internally as part of the larger Lightning.ByFolderPath node.
        /// </summary>
        /// <param name="processPath">The path to the process to execute.</param>
        /// <param name="args">The command line arguments to the process.</param>
        /// <returns>The exit code for the process.</returns>
        [NodeCategory("Create")]
        public static int ByPathAndArguments(string processPath, string args)
        {
            if (!File.Exists(processPath))
            {
                throw new FileNotFoundException();
            }
            Stopwatch counter = Stopwatch.StartNew();
            counter.Start();
            //var process = new Process();
            //var process = new System.Diagnostics.Process { StartInfo = new ProcessStartInfo(processPath, args) };
            ProcessStartInfo startInfo = new ProcessStartInfo(processPath);
            startInfo.Arguments = args;
            var process = System.Diagnostics.Process.Start(startInfo);
            bool exit = process.WaitForExit(1800000);
            if(exit)
            {
                counter.Stop();
                return 0;
            }
            else
            {
                process.Kill();
                counter.Stop();
                return 0;
            }
            // get highest KEY from tables
            //store that and start counter for Date Executed in RunStat table.
            
            //examine timer and/or terminate process
            //return process.ExitCode;
        }

        /// <summary>
        /// Kill the current process and return the exit code.
        /// ATTENTION: Use carefully! If run in DynamoSandbox this node will kill the DynamoSandbox.exe process, if run in DynamoRevit it will kill the Revit.exe process!
        /// </summary>
        /// <param name="toggle">Should the current process be terminted?</param>
        /// <returns>The exit code for the process.</returns>
        [NodeCategory("Action")]
        public static int KillCurrentProcess(bool toggle = false)
        {
            if (toggle)
            {
                System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                currentProcess.Kill();
                return currentProcess.ExitCode;
            }
            else
            {
                return 0;
            }
        }
    }
}
