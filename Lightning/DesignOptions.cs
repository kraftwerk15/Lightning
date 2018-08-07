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
    public static class DesignOptions
    {
        /// <summary>
        /// Gather Design Options and Design Options Sets
        /// </summary>
        /// <returns name="Design Option Name">The Design Option.</returns>
        /// <returns name="Design Option Id">The Design Option Element Id.</returns>
        /// <returns name="Design Option Set Name">The Design Option Set Name.</returns>
        /// <returns name="Design Option Set Id">The Design Option Set Element Id.</returns>
        /// <search>design, options, option, collector</search>
        [NodeName("Get Design Options")]
        [NodeCategory("Query")]
        [MultiReturn(new[] { "Design Option Name", "Design Option Id", "Design Option Set Name", "Design Option Set Id" })]
        public static Dictionary<string,object> GetDesignOptions()
        {
            List<dynamic> name = new List<dynamic>();
            List<dynamic> id = new List<dynamic>();
            List<ElementId> setIds = new List<ElementId>();
            List<dynamic> sets = new List<dynamic>();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            FilteredElementCollector design = new FilteredElementCollector(doc).OfClass(typeof(DesignOption));
            IList<Element> n = design.ToElements();
            foreach(var k in n)
            {
                name.Add(k.Name.ToString());
                id.Add(k.Id);
                ElementId setId = k.get_Parameter(BuiltInParameter.OPTION_SET_ID).AsElementId();
                if (!setIds.Contains(setId))
                    setIds.Add(setId);
            }
            foreach(var m in setIds)
            {
                string setsName = doc.GetElement(m).Name.ToString();
                sets.Add(setsName);
            }
            return new Dictionary<string, object>
            {
                {"Design Option Name", name },
                {"Design Option Id", id },
                {"Design Option Set Name", sets },
                {"Design Option Set Id", setIds}
            };
        }
    }
}
