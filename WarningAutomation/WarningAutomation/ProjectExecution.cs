using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Revit.Transaction;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Dynamo.Graph.Nodes;

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
        /// <returns></returns>
        [NodeCategory("Action")]
        public static bool SynchronizeWithCentral_Specific(
            bool StandardWorksets = true, bool ViewWorksets = true, bool FamilyWorksets = true,bool UserWorksets = true,bool CheckedOutElements = true, 
            bool Compact = true, bool SaveLocalBefore = true, bool SaveLocalAfter = true, string Comment = "", bool Transact = false)
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;
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
        /// <returns></returns>
        [NodeCategory("Action")]
        public static bool SynchronizeWithCentral_General(bool Compact = true, bool SaveLocalBefore = true, bool SaveLocalAfter = true, string Comment = "", bool Transact = false)
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;
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
}
