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
using System.Collections;
using Revit.Elements;
using Revit.GeometryConversion;
using Autodesk.DesignScript.Runtime;

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
        /// <returns></returns>
        [NodeCategory("Action")]
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
    }
}

