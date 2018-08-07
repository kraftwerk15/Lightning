using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spring
{
    public static class Groups
    {
        /// <summary>
        /// Find all unplaced model groups in a project.
        /// </summary>
        /// <returns>All unplaced model gropus in a project.</returns>
        /// <search>model, group, absent, unplaced, unavailable, types</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "Groups Not Placed Name", "Groups Not Placed ID", "Groups Placed Name", "Groups Placed ID" })]
        public static Dictionary<string, object> Unplaced()
        {
            List<dynamic> pname = new List<dynamic>();
            List<dynamic> id = new List<dynamic>();
            List<dynamic> upname = new List<dynamic>();
            List<dynamic> notID = new List<dynamic>();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            FilteredElementCollector groupType = new FilteredElementCollector(doc).OfClass(typeof(Autodesk.Revit.DB.GroupType));
            foreach (GroupType k in groupType)
            {
                if (k.Groups.IsEmpty)
                {
                    pname.Add(k.Name.ToString());
                    id.Add(k.Id);
                }
                else
                {
                    upname.Add(k.Name.ToString());
                    notID.Add(k.Id);
                };
            }

            return new Dictionary<string, object>
            {
                {"Groups Not Placed Name", pname },
                {"Groups Not Placed ID", id },
                {"Groups Placed Name",upname },
                {"Groups Placed ID", notID }
            };
        }
    }
}
