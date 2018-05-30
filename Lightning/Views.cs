using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Revit.Elements;
using Revit.Transaction;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Autodesk.DesignScript.Runtime;

namespace Spring
{
    public static class Views
    {
        /// <summary>
        /// Given a ViewFamilyType name as a string, attempts to find the ViewFamilyType in the Active Document.
        /// </summary>
        /// <param name="ViewFamilyType">The View Type as a string.</param>
        /// <returns>The ViewFamilyType as an element.</returns>
        public static Revit.Elements.Element ViewFamilyType(string ViewFamilyType)
        {
            List<Autodesk.Revit.DB.Element> collector = new List<Autodesk.Revit.DB.Element>();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var e = new FilteredElementCollector(doc);

            e.OfClass(typeof(ViewFamilyType));
            var elements = e.ToElements();

            Revit.Elements.Element hold = elements.Where(x => x.Name == ViewFamilyType).FirstOrDefault().ToDSType(true);
            if (hold == null)
            {
                return null;
            }
            else
                return hold;
        }

        /// <summary>
        /// This node will take a given list of ViewFamilyTypes and find the Views that those ViewFamilyTypes are applied.
        /// </summary>
        /// <param name="ViewFamilyType">The View Family Type.</param>
        /// <param name="Views">The Views to compare against.</param>
        /// <returns>A nested list of matching ViewFamilyTypes and Views.</returns>
        [MultiReturn(new[] { "View Family Type Found", "View Family Type Not Found" })]
        public static Dictionary<string, List<List<Revit.Elements.Element>>> CompareViewFamilyTypes(List<Revit.Elements.Element> ViewFamilyType, List<Revit.Elements.Element> Views)
        {
            List<List<Revit.Elements.Element>> found = new List<List<Revit.Elements.Element>>();
            List<List<Revit.Elements.Element>> not = new List<List<Revit.Elements.Element>>();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            foreach (Revit.Elements.Element k in Views)
            {
                if(k != null)
                {
                    List<Revit.Elements.Element> nest = new List<Revit.Elements.Element>();
                    nest.Clear();
                    ElementId searcher = Spring.Elements.UnwrapElement(k);
                    View finalView = doc.GetElement(searcher) as View;
                    ElementId viewID = finalView.GetTypeId();
                    if (viewID != null && searcher != null)
                    {
                        foreach (Revit.Elements.Element l in ViewFamilyType)
                        {
                            ElementId finder = Spring.Elements.UnwrapElement(l);
                            ViewFamilyType elements = doc.GetElement(finder) as ViewFamilyType;
                            Revit.Elements.Element oView = finalView.ToDSType(false);
                            Revit.Elements.Element eMent = elements.ToDSType(false);
                            nest.Add(oView);
                            nest.Add(eMent);
                            if (viewID == finder)
                            {
                                found.Add(nest);
                            }
                            else
                            {
                                not.Add(nest);
                            };
                        }
                    }

                }
            }
            return new Dictionary<string, List<List<Revit.Elements.Element>>>()
            {
                { "View Family Type Found", found },
                { "View Family Type Not Found", not }
            };

        }

        /// <summary>
        /// Given a Plan View, find the Associated Level or Generated Level of the View.
        /// </summary>
        /// <param name="View">Expecting a Plan View.</param>
        /// <returns>A Level to use.</returns>
        public static Revit.Elements.Element AssociatedLevel(Revit.Elements.Element View)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            ElementId searcher = Spring.Elements.UnwrapElement(View);
            View finalView = doc.GetElement(searcher) as View;
            Revit.Elements.Element level = finalView.GenLevel.ToDSType(true);
            return level;
        }

    }
}
