using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Dynamo.Graph.Nodes;
using Autodesk.DesignScript.Runtime;
using Revit.Elements;

namespace Spring
{
    public static class Elements
    {
        /// <summary>
        /// For a set of ElementID's, copy from one view to another view. These Elements can be View Specific or Modeled Elements.
        /// </summary>
        /// <param name="SourceView">View in which to get elements.</param>
        /// <param name="Element">A collection of Elements.</param>
        /// <param name="DestinationView">View in which to send elements.</param>
        /// <returns>Collection of Element ID's that were copied to the view.</returns>
        [NodeCategory("Action")]
        [IsVisibleInDynamoLibrary(false)]
        public static ICollection<ElementId> CopyPaste(List<Revit.Elements.Element> Element, Revit.Elements.Views.View SourceView, Revit.Elements.Views.View DestinationView)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            ICollection<ElementId> elem = new System.Collections.ObjectModel.Collection<ElementId>() as ICollection<ElementId>;
            ElementId sourceViewId = UnwrapElement(SourceView);
            View sourcefinalView = doc.GetElement(sourceViewId) as View;
            ElementId destViewId = UnwrapElement(DestinationView);
            View destfinalView = doc.GetElement(destViewId) as View;

            foreach (var k in Element)
            {
                Autodesk.Revit.DB.ElementId ElementID = UnwrapElement(k);
                elem.Add(ElementID);
            };

            try
            {
                TransactionManager.Instance.EnsureInTransaction(doc);
                ElementTransformUtils.CopyElements(sourcefinalView, elem, destfinalView, null, null);
                TransactionManager.Instance.TransactionTaskDone();
                return elem;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Dynamo Unwrapper
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        internal static ElementId UnwrapElement(Revit.Elements.Element element)
        {
            Autodesk.Revit.DB.Element UnwrappedElement;
            UnwrappedElement = element.InternalElement;
            ElementId elementId = UnwrappedElement.Id;
            return elementId;
        }

        /// <summary>
        /// Shortcut to get a family and family type.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="familyName"></param>
        /// <param name="symbolName"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        internal static FamilySymbol GetSymbol(Autodesk.Revit.DB.Document document, string familyName, string symbolName)
        {
            return new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Family)).OfType<Autodesk.Revit.DB.Family>().FirstOrDefault(f => f.Name.Equals(familyName))?.GetFamilySymbolIds().Select(id => document.GetElement(id)).OfType<FamilySymbol>().FirstOrDefault(symbol => symbol.Name.Equals(symbolName));
        }

        
        /// <summary>
        /// If the Element exists in this phase, then it will be returned in the list. "New", "Existing", "Temporary" values are returned.
        /// </summary>
        /// <param name="Element"></param>
        /// <param name="Phase"></param>
        /// <returns>A List of Elements that exist in this Phase.</returns>
        /// <search>element, family, exists, in, phase, current, future, past, status</search>
        [NodeCategory("Query")]
        public static List<Revit.Elements.Element> ExistsInPhase(List<Revit.Elements.Element> Element, Revit.Elements.Element Phase)
        {
            List<Revit.Elements.Element> vs = new List<Revit.Elements.Element>();
            List<string> phaseStat = GetPhaseStatus(Element, Phase);
            for (int i = 0; i < phaseStat.Count; i++)
            {
                string item = phaseStat[i];
                Revit.Elements.Element element = Element[i];
                switch (item)
                {
                    case "New":
                        vs.Add(element);
                        break;
                    case "Existing":
                        vs.Add(element);
                        break;
                    case "Temporary":
                        vs.Add(element);
                        break;
                    default:
                        break;
                }
            }
            return vs;
        }

        /// <summary>
        /// The Phase Status of the Element. See RevitAPI Documentation and search for GetPhaseStatus for more information.
        /// </summary>
        /// <param name="Element">The Element of the Phase to check.</param>
        /// <param name="Phase">The Phase to check.</param>
        /// <returns>A string value to inform user of the status.</returns>
        /// <search>get, phase, status, element, family, current, past, future, existing, new, construction</search>
        [NodeCategory("Query")]
        public static List<string> GetPhaseStatus(List<Revit.Elements.Element> Element, Revit.Elements.Element Phase)
        {
            List<string> vs = new List<string>();
            ElementId phaseID = UnwrapElement(Phase);
            foreach (Revit.Elements.Element v in Element)
            {
                ElementId id = UnwrapElement(v);
                string phaseStat = Spring.Phase.GetPhaseStatus(id, phaseID);
                vs.Add(phaseStat);
            }
            return vs;
        }

        /// <summary>
        /// Get a Revit Element given its ElementId.
        /// </summary>
        /// <param name="ElementID">ElementId of the Element we will query.</param>
        /// <returns>The Revit Element.</returns>
        /// <search>get, by, id, element</search>
        [NodeCategory("Query")]
        public static Revit.Elements.Element GetByID(int ElementID)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            ElementId id = new ElementId(ElementID);
            var elem = doc.GetElement(id);
            return elem.ToDSType(true);
        }


        private static Dictionary<string, object> ElementCollector()
        {
            List<dynamic> ids = new List<dynamic>();
            List<dynamic> name = new List<dynamic>();
            List<dynamic> elements = new List<dynamic>();
            List<dynamic> clean = new List<dynamic>();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            Categories categories = doc.Settings.Categories;
            foreach (Autodesk.Revit.DB.Category i in categories)
            {
                try
                {
                    ids.Add(i.Id.IntegerValue);
                }
                catch
                {
                    break;
                }
            }
            foreach (var i in ids)
            {
                try
                {
                    name.Add(System.Enum.ToObject(typeof(BuiltInCategory), i));
                }
                catch
                {
                    break;
                }
            }
            foreach (var k in name)
            {
                
                List<Autodesk.Revit.DB.Element> elem = new FilteredElementCollector(doc).WhereElementIsNotElementType().OfCategory(k).ToElements();
                foreach(var i in elem)
                {
                    Revit.Elements.Element tem = i.ToDSType(true);
                    elements.Add(tem);
                }
                List<dynamic> temp = new List<dynamic>(elem);
                clean.Add(temp);
            }
            return new Dictionary<string, object>
            {
                {"Category Id", ids },
                {"Category Name", name },
                {"Elements", elements },
                {"Clean", clean }
            };
        }

        /// <summary>
        /// Collect all elements that fall under Built-In Categories in the Project.
        /// </summary>
        /// <returns name="Category Id">The ID of the Category.</returns>
        /// <returns name="Category Name">The Name of the Category.</returns>
        /// <returns name="Elements">A list of Elements.</returns>
        /// <search>project, category, elements, collector, collect</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] {"Category Id", "Category Name", "Elements" })]
        public static Dictionary<string, object> CollectElements()
        {
            List<dynamic> ids = new List<dynamic>();
            List<dynamic> name = new List<dynamic>();
            List<dynamic> elements = new List<dynamic>();
            Dictionary<string,object> elem = ElementCollector();
            object temp;
            if(elem.TryGetValue("Category Id", out temp))
            {
                ids = temp as List<dynamic>;
            }
            if(elem.TryGetValue("Category Name",out temp))
            {
                name = temp as List<dynamic>;
            }
            if(elem.TryGetValue("Elements",out temp))
            {
                elements = temp as List<dynamic>;
            }
            return new Dictionary<string, object>
            {
                {"Category Id", ids },
                {"Category Name", name },
                {"Elements", elements }
            };
        }

        /// <summary>
        /// Collect Modeled Elements in the Project.
        /// </summary>
        /// <returns name="Model Elements">List of Model Elements.</returns>
        /// <returns name="Count">The count of model elements.</returns>
        /// <search>model, elements, walls, doors, collect, collector</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "Model Elements", "Count" })]
        public static Dictionary<string, object> CollectModelElements()
        {
            List<dynamic> ids = new List<dynamic>();
            List<dynamic> name = new List<dynamic>();
            List<dynamic> elements = new List<dynamic>();
            List<dynamic> error = new List<dynamic>();
            Dictionary<string, object> elem = ElementCollector();
            object temp;
            if (elem.TryGetValue("Clean", out temp))
            {
                elements = temp as List<dynamic>;
                foreach (var i in elements)
                {
                    if (i == null || i.Count == 0)
                        continue;
                    foreach(Autodesk.Revit.DB.Element j in i)
                    {
                        try
                        {
                            string k = j.ToString();
                            if (j.ViewSpecific == false && (k != "Insulations" || k != "Analytical" || k != "Systems" || k != "Tags" || k != "Dimension" || k != "Views"))
                            {
                                name.Add(j.ToDSType(true));
                            }
                           
                            if (k == "Insulations" || k == "Analytical" || k == "Systems" || k == "Tags" || k == "Dimension")
                            {
                                ids.Add(i);
                            }
                        }
                        catch (Exception ex)
                        {
                            error.Add(ex.ToString());
                        }
                    }
                }
            }
            int count = name.Count();
            return new Dictionary<string, object>
            {
                {"Model Elements", name },
                {"Count", count },
                {"Error", error }
            };
        }
    }
}

