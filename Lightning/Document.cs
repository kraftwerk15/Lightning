using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Dynamo.Graph.Nodes;
using Autodesk.DesignScript.Runtime;


namespace Spring
{
    public static class Document
    {

        /// <summary>
        /// Get the Name of the Project Location for this project as a string.
        /// </summary>
        /// <returns>Name of the Project Location.</returns>
        /// <search>active, location, shared, coordinates, name, place</search>
        [NodeCategory("Query")]
        public static string ActiveProjectLocationName()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            return doc.ActiveProjectLocation.Name;
        }

        /// <summary>
        /// Gathers a list of phases and their IDs.
        /// </summary>
        /// <returns name="Phase Name">A list Phase Names.</returns>
        /// <returns name="Phase">A list of phase objects.</returns>
        /// <search>phase, status, all, collect</search>
        [MultiReturn(new[] { "Phase Name", "Phase" })]
        public static Dictionary<string, object> Phase()
        {
            List<dynamic> phases = new List<dynamic>();
            List<dynamic> phaseName = new List<dynamic>();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            PhaseArray phase = doc.Phases;
            foreach (Autodesk.Revit.DB.Phase i in phase)
            {
                phases.Add(i.Id);
                phaseName.Add(i.Name);
            }

            return new Dictionary<string, object>
            {
                {"Phase Name", phaseName },
                { "Phase", phases }
            };
        }

        /// <summary>
        /// Gather all Group Types in a project.
        /// </summary>
        /// <returns>Group Types, their names, and IDs.</returns>
        /// <search>model, group, type, name, id</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "Group Type ID", "Group Type Name", "Group Type" })]
        public static Dictionary<string, object> GroupType()
        {
            List<dynamic> id = new List<dynamic>();
            List<dynamic> parent = new List<dynamic>();
            List<dynamic> type = new List<dynamic>();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            FilteredElementCollector groupType = new FilteredElementCollector(doc).OfClass(typeof(Autodesk.Revit.DB.GroupType));
            foreach (var i in groupType)
            {
                id.Add(i.Id);
                parent.Add(i.Name);
                type.Add(i.Category.Name);
            }

            return new Dictionary<string, object>
            {
                {"Group Type ID", id },
                {"Group Type Name", parent },
                {"Group Type", type }
            };
        }

        /// <summary>
        /// Gather all Links and Imports in a project. Note here this is not Revit Links.
        /// </summary>
        /// <returns>Links and Imports in project.</returns>
        /// <search>cad, imports, links, image, jpeg, png, decal</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "Import ID", "Import Name", "Link ID", "Link Name" })]
        public static Dictionary<string, object> LinksImports()
        {
            List<dynamic> id = new List<dynamic>();
            List<dynamic> type = new List<dynamic>();
            List<dynamic> linkId = new List<dynamic>();
            List<dynamic> linkName = new List<dynamic>();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            //Change both of these to OnDocumentChanged?

            FilteredElementCollector import = new FilteredElementCollector(doc).OfClass(typeof(ImportInstance));
            foreach (var m in import)
            {
                id.Add(m.Id);
                type.Add(m.Name);
            }

            FilteredElementCollector link = new FilteredElementCollector(doc).OfClass(typeof(CADLinkType));
            foreach (var n in link)
            {
                linkId.Add(n.Id);
                linkName.Add(n.Name);
            }

            return new Dictionary<string, object>
            {
                {"Import ID", id },
                {"Import Name", type },
                {"Link ID", linkId},
                {"Link Name", linkName }
            };
        }

        /// <summary>
        /// Returns if the Document is workshared.
        /// </summary>
        /// <param name="document">Can be null. Pass in a Revit Document.</param>
        /// <returns>Boolean</returns>
        /// <search>workshare, synch, document, project, central</search>
        [NodeCategory("Query")]
        public static bool IsWorkshared(Revit.Application.Document document = null)
        {
            Autodesk.Revit.DB.Document doc = null;
            if (document.GetType().ToString() == "Autodesk.Revit.DB.Document")
            {
                //doc = document; Cannot modify Revit.Application.Document to Autodesk.Revit.DB.Document
            }
            else if (document == null)
            {
                //removing this because of something passed in that is not null
                //doc = DocumentManager.Instance.CurrentDBDocument;
            }
            else
            {
                doc = DocumentManager.Instance.CurrentDBDocument;
            }
            if (doc.IsWorkshared)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Get the Central Model's File Path.
        /// </summary>
        /// <param name="document">The Document to Query.</param>
        /// <returns>The File Path to the Central Model.</returns>
        /// <search>central, file, path</search>
        [NodeCategory("Query")]
        public static string CentralFilePath()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            if (doc.IsWorkshared)
            {
                ModelPath path = doc.GetWorksharingCentralModelPath();
                string finalPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(path);
                return finalPath;
            }
            else
                throw new Exception("This Document is not workshared.");
        }

        /// <summary>
        /// This node gathers the Version information for the current Revit Document.
        /// </summary>
        /// <returns name="Version Name">The Application name.</returns>
        /// <returns name="Version Number">The Year Build for the Application. i.e. 2018</returns>
        /// <returns name="Version Build">The Application build number.</returns>
        /// <returns name="Version Language">The langguage of the current application.</returns>
        /// <returns name="Full Version">The Year and Patch/Hotfix number. i.e. 2018.3</returns>
        /// <search>application, build, language, version, build, patch, hotfix, information</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "Version Name", "Version Number", "Version Build", "Version Language", "Full Version" })]
        public static Dictionary<string, object> ApplicationVersion()
        {
            var document = DocumentManager.Instance.CurrentDBDocument;
            var uiapp = DocumentManager.Instance.CurrentUIApplication;
            var app = uiapp.Application;
            object fullVersion = null;
            if(Convert.ToInt32(app.VersionNumber) > 2017)
            {
                fullVersion = app.SubVersionNumber;
            }
            else
            {
                fullVersion = app.VersionNumber;
            }

            return new Dictionary<string, object>
            {
                {"Version Name", app.VersionName },
                {"Version Number", Convert.ToInt32(app.VersionNumber)},
                {"Version Build", app.VersionBuild },
                {"Version Language", app.Language.ToString() },
                {"Full Version", fullVersion }
            };
        }

        /// <summary>
        /// Gathers the Project Position for this model or for Revit Link Instances.
        /// </summary>
        /// <param name="Document_RevitLinkInstance">This is optional. If a "null" object or a Document.Current node is passed in, it will operate on the current document.</param>
        /// <returns name="Angle">The Project Position of the project.</returns>
        /// <returns name="NorthSouth">The North/South direction.</returns>
        /// <returns name="EastWest">The East/West direction.</returns>
        /// <returns name="Elevation">The Elevation of the project.</returns>
        /// <search>project, location, position, angle, north, east, south, west</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "Angle", "NorthSouth", "EastWest", "Elevation" })]
        public static Dictionary<string, object> ProjectPosition(dynamic Document_RevitLinkInstance)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            Dictionary<string, object> appversion = ApplicationVersion();
            int version = 0;
            ProjectPosition pData;
            SiteLocation lData;
            string dispsym;
            if(appversion.TryGetValue("Version Number", out object value))
            {
                version = Convert.ToInt32(value);
            }
            if(Document_RevitLinkInstance != null && Document_RevitLinkInstance.GetType().FullName.ToString() != "Revit.Application.Document")
            {
                doc = Document_RevitLinkInstance.InternalElement.Document;
            }

            ProjectLocation pLoc = doc.ActiveProjectLocation;
            if(version > 2017)
            {
                pData = pLoc.GetProjectPosition(XYZ.Zero);
                lData = pLoc.GetSiteLocation();
            }
            else
            {
                pData = pLoc.GetProjectPosition(XYZ.Zero);
                lData = pLoc.SiteLocation;
            }
            
            double angle = pData.Angle * (180 / Math.PI);
            double elevation = Math.Round(pData.Elevation,8);
            double ew = Math.Round(pData.EastWest,8);
            double ns = Math.Round(pData.NorthSouth,8);
            UnitType unittype = UnitType.UT_Length;
            FormatOptions formatoptions = doc.GetUnits().GetFormatOptions(unittype);
            DisplayUnitType dispunits = formatoptions.DisplayUnits;
            UnitSymbolType symtype = formatoptions.UnitSymbol;
            if (symtype == UnitSymbolType.UST_NONE)
                dispsym = "none";
            else
                dispsym = LabelUtils.GetLabelFor(symtype);
            elevation = UnitUtils.ConvertFromInternalUnits(elevation, dispunits);
            ew = UnitUtils.ConvertFromInternalUnits(ew, dispunits);
            ns = UnitUtils.ConvertFromInternalUnits(ns, dispunits);
            return new Dictionary<string, object>
            {
                {"Angle", angle },
                {"NorthSouth", ns},
                {"EastWest", ew },
                {"Elevation", elevation }
            };
        }

        /// <summary>
        /// Retrieve the Project Information (Manage tab > Project Information) for the active project.
        /// </summary>
        /// <returns>Multiple items to use.</returns>
        /// <search>project, proj, information, manage, building, name, client, number, organization, description, status, issue</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "Project Name", "Project Number", "Client Name", "Address", "Building Name", "Organization Name", "Organization Description" , "Status" , "Issue Date" })]
        public static Dictionary<string, object> ProjectInformation()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            string address = doc.ProjectInformation.Address;
            string buildingName = doc.ProjectInformation.BuildingName;
            string clientName = doc.ProjectInformation.ClientName;
            string projName = doc.ProjectInformation.Name;
            string projNumber = doc.ProjectInformation.Number;
            string orgDesc = doc.ProjectInformation.OrganizationDescription;
            string orgName = doc.ProjectInformation.OrganizationName;
            string status = doc.ProjectInformation.Status;
            string issue = doc.ProjectInformation.IssueDate;

            return new Dictionary<string, object>
            {
                {"Project Name", projName },
                {"Project Number", projNumber},
                {"Client Name", clientName},
                {"Address",address },
                {"Building Name", buildingName },
                {"Organization Name", orgName },
                {"Organization Description", orgDesc },
                {"Status", status },
                {"Issue Date", issue }
            };
        }
    }
}
