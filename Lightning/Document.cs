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
        /// Gathers the links associated with the current document.
        /// </summary>
        /// <returns>The name, type, id, and document of the link found.</returns>
        /// <search>document, current, link, revit, id, get, all, collect</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "Document", "Name","ID","Type" })]
        public static Dictionary<string, object> RevitLinkCollector()
        {
            List<dynamic> document = new List<dynamic>();
            List<dynamic> name = new List<dynamic>();
            List<dynamic> id = new List<dynamic>();
            List<dynamic> type = new List<dynamic>();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            FilteredElementCollector filtered = new FilteredElementCollector(doc);
            var links = filtered.OfClass(typeof(RevitLinkInstance));

            foreach (var l in links)
            {
                document.Add(l);
                name.Add(l.Name);
                id.Add(l.Id);
                type.Add(l.GetTypeId());
            }

            return new Dictionary<string, object>
            {
                {"Document", document },
                {"Name", name },
                {"ID", id },
                {"Type", type }
            };
        }

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
        public static Dictionary<string,object> Phase()
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
        /// Collect all model elements in a project.
        /// </summary>
        /// <returns>Model Elements.</returns>
        [IsVisibleInDynamoLibrary(false)]
        [NodeCategory("Query")]
        [MultiReturn(new[] { "Element ID", "phaseName"})]
        public static Dictionary<string, object> ModelElementCollector()
        {
            List<dynamic> id = new List<dynamic>();
            List<dynamic> phaseName = new List<dynamic>();
            List<dynamic> phaseString = new List<dynamic>();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            Categories uCat = doc.Settings.Categories;
            foreach(Category i in uCat)
            {
                try
                {
                    id.Add(i.Id.IntegerValue);
                }
                catch
                {
                    id.Add(null);
                };
            }

            return new Dictionary<string, object>
            {
                {"Element ID", id },
                {"phaseName", phaseName }
            };
        }

        /// <summary>
        /// Gather all Group Types in a project.
        /// </summary>
        /// <returns>Group Types, their names, and IDs.</returns>
        /// <search>model, group, type, name, id</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] {"Group Type ID","Group Type Name","Group Type" })]
        public static Dictionary<string, object> GroupType()
        {
            List<dynamic> id = new List<dynamic>();
            List<dynamic> parent = new List<dynamic>();
            List<dynamic> type = new List<dynamic>();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            FilteredElementCollector groupType = new FilteredElementCollector(doc).OfClass(typeof(Autodesk.Revit.DB.GroupType));
            foreach(var i in groupType)
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
        /// Find all unplaced model groups in a project.
        /// </summary>
        /// <returns>All unplaced model gropus in a project.</returns>
        /// <search>model, group, absent, unplaced, unavailable, types</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "Groups Not Placed", "Groups Placed" })]
        public static Dictionary<string, object> UnplacedGroups()
        {
            List<dynamic> id = new List<dynamic>();
            List<dynamic> notID = new List<dynamic>();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            FilteredElementCollector groupType = new FilteredElementCollector(doc).OfClass(typeof(Autodesk.Revit.DB.GroupType));
            foreach(GroupType k in groupType)
            {
                if (k.Groups.IsEmpty)
                {
                    id.Add(k);
                }
                else
                {
                    notID.Add(k);
                };
            }

            return new Dictionary<string, object>
            {
                {"Groups Not Placed", id },
                {"Groups Placed", notID }
            };
        }

        /// <summary>
        /// Gather all Links and Imports in a project. Note here this is not Revit Links.
        /// </summary>
        /// <returns>Links and Imports in project.</returns>
        /// <search>cad, imports, links, image, jpeg, png, decal</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] {"Import ID","Import Name", "Link ID", "Link Name"})]
        public static Dictionary<string, object> LinksImports()
        {
            List<dynamic> id = new List<dynamic>();
            List<dynamic> type = new List<dynamic>();
            List<dynamic> linkId = new List<dynamic>();
            List<dynamic> linkName = new List<dynamic>();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            //Change both of these to OnDocumentChanged?
            
            FilteredElementCollector import = new FilteredElementCollector(doc).OfClass(typeof(ImportInstance));
            foreach(var m in import)
            {
                id.Add(m.Id);
                type.Add(m.Name);
            }

            FilteredElementCollector link = new FilteredElementCollector(doc).OfClass(typeof(CADLinkType));
            foreach(var n in link)
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
    }
}
