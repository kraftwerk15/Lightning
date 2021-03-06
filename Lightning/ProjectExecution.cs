﻿using System;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;

namespace Spring
{
    public static class Synchronization
    {
        /// <summary>
        /// Allows you to Synchronize to Central with all available options exposed.
        /// </summary>
        /// <param name="StandardWorksets">Any other type of Workset, i.e. Project Info.</param>
        /// <param name="ViewWorksets">Worksets in which 2D/3D views are placed.</param>
        /// <param name="FamilyWorksets">Worksets in which families are placed.</param>
        /// <param name="UserWorksets">Any worksets not automatically created by Revit.</param>
        /// <param name="CheckedOutElements">Any elements checked out during this session.</param>
        /// <param name="Compact">Compact the model size.</param>
        /// <param name="SaveLocalBefore">Save file before the Synchronize to Central transaction.</param>
        /// <param name="SaveLocalAfter">Save the file after the Synchronization has taken place.</param>
        /// <param name="Comment">Provide a custom message.</param>
        /// <param name="Transact">Should this process be completed? This is set by default not to run.</param>
        /// <returns>The Synchronized Event.</returns>
        /// <search>sync, synchronize, with, central, event, workshare, worksharing, specific, save, project</search>
        [NodeCategory("Action")]
        public static bool SynchronizeWithCentralSpecific(
            bool StandardWorksets = true, bool ViewWorksets = true, bool FamilyWorksets = true,bool UserWorksets = true,bool CheckedOutElements = true, 
            bool Compact = true, bool SaveLocalBefore = true, bool SaveLocalAfter = true, string Comment = "", bool Transact = false)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            TransactWithCentralOptions transactOptions = new TransactWithCentralOptions();
            RelinquishOptions rOptions = new RelinquishOptions(false);
            rOptions.StandardWorksets = StandardWorksets;
            rOptions.ViewWorksets = ViewWorksets;
            rOptions.FamilyWorksets = FamilyWorksets;
            rOptions.UserWorksets = UserWorksets;
            rOptions.CheckedOutElements = CheckedOutElements;
            SynchronizeWithCentralOptions sOptions = new SynchronizeWithCentralOptions();
            sOptions.SetRelinquishOptions(rOptions);
            sOptions.Compact = Compact;
            sOptions.Comment = Comment;
            sOptions.SaveLocalBefore = SaveLocalBefore;
            sOptions.SaveLocalAfter = SaveLocalAfter;
            if (Transact == true)
            {
                try
                {
                    doc.SynchronizeWithCentral(transactOptions, sOptions);
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                };
            }
            else
                return false;
        }

        /// <summary>
        /// Allows you to synchronize will all of the RelinquishOptions set to true.
        /// </summary>
        /// <param name="Compact">Compact the Model.</param>
        /// <param name="SaveLocalBefore">Save file before the Synchronize to Central transaction.</param>
        /// <param name="SaveLocalAfter">Save the file after the Synchronization has taken place.</param>
        /// <param name="Comment">Provide a custom message.</param>
        /// <param name="Transact">Should this process be completed? This is set by default not to run.</param>
        /// <returns>The Synchronized Event.</returns>
        /// <search>sync, synchronize, with, central, event, workshare, worksharing, save, project</search>
        [NodeCategory("Action")]
        public static bool SynchronizeWithCentralGeneral(bool Compact = true, bool SaveLocalBefore = true, bool SaveLocalAfter = true, string Comment = "", bool Transact = false)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            TransactWithCentralOptions transactOptions = new TransactWithCentralOptions();
            RelinquishOptions rOptions = new RelinquishOptions(true);
            SynchronizeWithCentralOptions sOptions = new SynchronizeWithCentralOptions();
            sOptions.SetRelinquishOptions(rOptions);
            sOptions.Compact = Compact;
            sOptions.Comment = Comment;
            sOptions.SaveLocalBefore = SaveLocalBefore;
            sOptions.SaveLocalAfter = SaveLocalAfter;
            if (Transact == true)
            {
                try
                {
                    doc.SynchronizeWithCentral(transactOptions, sOptions);
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                };
            }
            else
            {
                return false;
            };
        }
    }

    public static class Save
    {
        /// <summary>
        /// Gives you the ability to save a document locally without creating it as a new central. Particularly after detaching a batch of files.
        /// </summary>
        /// <param name="path">File Path to save the new file.</param>
        /// <param name="compact">Should the file be compacted.</param>
        /// <param name="maximumBackups">Set the maximum number of backups.</param>
        /// <returns>If the file was saved, the node will return true.</returns>
        /// <search>save, project, local</search>
        [NodeCategory("Action")]
        public static bool ProjectAsLocal(string path, bool compact = false, int maximumBackups = 10)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            SaveAsOptions saveAs = new SaveAsOptions
            {
                Compact = compact,
                MaximumBackups = maximumBackups
            };
            try
            {
                doc.SaveAs(path, saveAs);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gives you the ability to save this document as a new central file. Particularly after detaching a batch of files.
        /// </summary>
        /// <param name="path">The file path to save the file. This should include the path, the file name, and the file extension.</param>
        /// <param name="compact">Should the file be compacted?</param>
        /// <param name="maximumBackups">What is the number of backups the file should create automatically while users are working?</param>
        /// <param name="WorksetConfiguration">What is the WorksetConfiguration? Recommend using Lightning node "Workset Configuration".</param>
        /// <returns>Successfully completed.</returns>
        /// <search>save, project, as, central</search>
        [NodeCategory("Action")]
        public static bool ProjectAsCentral(string path, bool compact = false, int maximumBackups = 10, string WorksetConfiguration = "AskUserToSpecify")
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            SaveAsOptions saveAs = new SaveAsOptions
            {
                Compact = compact,
                MaximumBackups = maximumBackups
            };
            SimpleWorksetConfiguration simple = new SimpleWorksetConfiguration();
            WorksharingSaveAsOptions worksharing = new WorksharingSaveAsOptions();
            if(WorksetConfiguration == "AskUserToSpecify")
            {
                simple = SimpleWorksetConfiguration.AskUserToSpecify;
            }
            else if(WorksetConfiguration == "AllEditable")
            {
                simple = SimpleWorksetConfiguration.AllEditable;
            }
            else if(WorksetConfiguration == "AllWorksets")
            {
                simple = SimpleWorksetConfiguration.AllWorksets;
            }
            else if(WorksetConfiguration == "LastViewed")
            {
                simple = SimpleWorksetConfiguration.LastViewed;
            }
            else
            {
                throw new NotSupportedException("Workset Configuration may only be one of four types: AskUserToSpecify, AllEditable, AllWorksets, or LastViewed. Use WorksetConfiguration node in Lightning package for seamless execution.");
            }

            worksharing.OpenWorksetsDefault = simple;
            worksharing.SaveAsCentral = true;
            saveAs.SetWorksharingOptions(worksharing);
            
            try
            {
                doc.SaveAs(path, saveAs);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
