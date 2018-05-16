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
        /// Create a new Line-Based Family Instance given a line, the family, and the view to place the new family.
        /// </summary>
        /// <param name="Curve">The Line to create the Symbol.</param>
        /// <param name="FamilyName">The family name of the Line-based family.</param>
        /// <param name="FamilyType">The family type name of the Line-based family.</param>
        /// <param name="DestinationView">The view to create the family instance.</param>
        /// <returns>The Family Instances.</returns>
        [NodeName("FamilyInstanceByCurve")]
        [NodeCategory("Create")]
        public static List<Autodesk.Revit.DB.FamilyInstance> FamilyInstanceByCurve(List<Autodesk.DesignScript.Geometry.Curve> Curve, string FamilyName, string FamilyType, Revit.Elements.Views.FloorPlanView DestinationView)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            List<Autodesk.Revit.DB.FamilyInstance> famColl = new List<Autodesk.Revit.DB.FamilyInstance>();

            ElementId viewId = UnwrapElement(DestinationView);
            View finalView = doc.GetElement(viewId) as View;
            FamilySymbol symbol = GetSymbol(doc, FamilyName, FamilyType);
            try
            {
                TransactionManager.Instance.EnsureInTransaction(doc);
                foreach (Autodesk.DesignScript.Geometry.Curve i in Curve)
                {
                    Line j = i.ToRevitType() as Line;
                    Autodesk.Revit.DB.FamilyInstance instance = doc.Create.NewFamilyInstance(j, symbol, finalView);
                    instance.ToDSType(true);
                    famColl.Add(instance);
                };
                TransactionManager.Instance.TransactionTaskDone();
                return famColl;
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
        private static FamilySymbol GetSymbol(Autodesk.Revit.DB.Document document, string familyName, string symbolName)
        {
            return new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Family)).OfType<Autodesk.Revit.DB.Family>().FirstOrDefault(f => f.Name.Equals(familyName))?.GetFamilySymbolIds().Select(id => document.GetElement(id)).OfType<FamilySymbol>().FirstOrDefault(symbol => symbol.Name.Equals(symbolName));
        }

        /// <summary>
        /// Given a Family Name and a Family Type as strings, this node will find the Family Type if it exists in the project for your use.
        /// </summary>
        /// <param name="FamilyName">The family name of the family type we are trying to find.</param>
        /// <param name="FamilyType">The family type from the family.</param>
        /// <returns>The FamilyType as the family type.</returns>
        public static Revit.Elements.Element GetFamilyType(string FamilyName, string FamilyType)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            FamilySymbol symbol = GetSymbol(doc, FamilyName, FamilyType);
            FamilySymbol familySymbol = symbol as FamilySymbol;
            return symbol.ToDSType(false);
        }
    }
}

