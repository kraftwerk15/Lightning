using System;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using DB = Autodesk.Revit.DB;

namespace Spring
{
    public static class ProjectInformation
    {
        /// <summary>
        /// This will retrieve the built-in Project Number from the Project Information panel on the Manage Tab.
        /// </summary>
        /// <returns>Returns the current Project Number for the Project.</returns>
        /// <search>get, project, number, information</search>
        [NodeCategory("Query")]
        public static string GetProjectNumber()
        {
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            ProjectInfo projectInfo = doc.ProjectInformation;
            var num = projectInfo.Number;
            return num;
        }

        /// <summary>
        /// This will retrieve the built-in Project Name from the Project Information panel on the Manage Tab.
        /// </summary>
        /// <returns>Returns the current Project Name for the Project.</returns>
        /// <search>get, project, name, information</search>
        [NodeCategory("Query")]
        public static string GetProjectName()
        {
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            ProjectInfo projectInfo = doc.ProjectInformation;
            var name = projectInfo.Name;
            return name;
        }

        /// <summary>
        /// This node will allow you to set the Project Number for the Project.
        /// </summary>
        /// <param name="ProjectNumber">The Project Number you wish the Project to use.</param>
        /// <returns>Confirmation that the Project Number was changed.</returns>
        /// <search>set, project, number, information</search>
        [NodeCategory("Action")]
        public static string SetProjectNumber(string ProjectNumber)
        {
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
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
        /// <search>establish, set, project, name, information</search>
        [NodeCategory("Action")]
        public static string SetProjectName(string ProjectName)
        {
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
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
