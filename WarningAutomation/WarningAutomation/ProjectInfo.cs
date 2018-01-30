using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Revit.Transaction;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Spring
{
    public static class ProjectInformation
    {
        /// <summary>
        /// This will retrieve the built-in Project Number from the Project Information panel on the Manage Tab.
        /// </summary>
        /// <returns>Returns the current Project Number for the Project.</returns>
        public static string GetProjectNumber()
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;
            ProjectInfo projectInfo = doc.ProjectInformation;
            var num = projectInfo.Number;
            return num;
        }

        /// <summary>
        /// This will retrieve the built-in Project Name from the Project Information panel on the Manage Tab.
        /// </summary>
        /// <returns>Returns the current Project Name for the Project.</returns>
        public static string GetProjectName()
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;
            ProjectInfo projectInfo = doc.ProjectInformation;
            var name = projectInfo.Name;
            return name;
        }

        /// <summary>
        /// This node will allow you to set the Project Number for the Project.
        /// </summary>
        /// <param name="ProjectNumber">The Project Number you wish the Project to use.</param>
        /// <returns>Confirmation that the Project Number was changed.</returns>
        public static string SetProjectNumber(string ProjectNumber)
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;
            try
            {
                using (Autodesk.Revit.DB.Transaction t = new Autodesk.Revit.DB.Transaction(doc, "Dynamo_SetProjectNumber"))
                {
                    t.Start();
                    ProjectInfo projectInfo = doc.ProjectInformation;
                    projectInfo.Number = ProjectNumber;
                    t.Commit();
                    return projectInfo.Number;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// This node will allow you to set the Project Name for the Project.
        /// </summary>
        /// <param name="ProjectName">The Project Name you wish the Project to use.</param>
        /// <returns>Confirmation that the Project Name was changed.</returns>
        public static string SetProjectName(string ProjectName)
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;
            try
            {
                using (Autodesk.Revit.DB.Transaction t = new Autodesk.Revit.DB.Transaction(doc, "Dynamo_SetProjectName"))
                {
                    t.Start();
                    ProjectInfo projectInfo = doc.ProjectInformation;
                    projectInfo.Name = ProjectName;
                    t.Commit();
                    return projectInfo.Name;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
