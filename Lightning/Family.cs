using AD = Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RevitServices.Persistence;
using Dynamo.Graph.Nodes;
using Autodesk.DesignScript.Runtime;

namespace Spring
{
    public static class Family
    {

        /// <summary>
        /// Is the Family an In-Place Family?
        /// </summary>
        /// <param name="element">A list of families to check.</param>
        /// <returns>Boolean List</returns>
        /// <search>in, place, family, families</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "Boolean List", "Any True" })]
        public static Dictionary<string,object> IsInPlace(Revit.Elements.Element[] element)
        {
            List<bool> list = new List<bool>();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            foreach(var i in element)
            {
                try
                {
                    AD.ElementId id = Elements.UnwrapElement(i);
                    AD.Element elem = doc.GetElement(id) as AD.Element;
                    AD.FamilyInstance elFamInst = elem as AD.FamilyInstance;
                    AD.Family elFam = elFamInst.Symbol.Family;
                    if (elFam.IsInPlace)
                    {
                        list.Add(true);
                    }
                    else
                        list.Add(false);
                }
                catch
                {

                }
            }
            bool contain = list.Any(item => item);
            return new Dictionary<string, object>
            {
                {"Boolean List", list },
                {"Any True", contain }
            };
        }
    }
}
