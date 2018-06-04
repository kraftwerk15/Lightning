using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Revit.Elements;
using RevitServices.Persistence;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using RevitServices.Transactions;
using Revit.GeometryConversion;

namespace Spring
{
    public static class FamilyInstance
    {
        /// <summary>
        /// Get the From and To Room information from a Door or Window.
        /// </summary>
        /// <param name="FamilyInstance">Door or Window</param>
        /// <param name="Phase">The Phase to check against.</param>
        /// <returns>A List of From and To Rooms.</returns>
        /// <search>host, door, window, from, to, flip, room</search>
        [NodeCategory("Action")]
        [MultiReturn(new[] { "From Room", "To Room" })]
        public static Dictionary<string, List<Revit.Elements.Element>> GetToFromRoom(List<Revit.Elements.Element> FamilyInstance, Revit.Elements.Element Phase)
        {
            List<Revit.Elements.Element> toRooms = new List<Revit.Elements.Element>();
            List<Revit.Elements.Element> fromRooms = new List<Revit.Elements.Element>();
            ElementId phaseId = Spring.Elements.UnwrapElement(Phase);
            try
            {
                Autodesk.Revit.DB.Phase phase = DocumentManager.Instance.CurrentDBDocument.GetElement(phaseId) as Autodesk.Revit.DB.Phase;
                try
                {
                    foreach (Revit.Elements.Element v in FamilyInstance)
                    {
                        ElementId elemId = Spring.Elements.UnwrapElement(v);
                        Autodesk.Revit.DB.FamilyInstance elements = DocumentManager.Instance.CurrentDBDocument.GetElement(elemId) as Autodesk.Revit.DB.FamilyInstance;
                        Autodesk.Revit.DB.Architecture.Room fromRoom = elements.get_FromRoom(phase);
                        Autodesk.Revit.DB.Architecture.Room toRoom = elements.get_ToRoom(phase);
                        toRooms.Add(toRoom.ToDSType(true));
                        fromRooms.Add(fromRoom.ToDSType(true));
                    };
                }
                catch (Exception ex)
                {
                    toRooms.Add(null);
                    fromRooms.Add(null);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return new Dictionary<string, List<Revit.Elements.Element>>
            {
                {"From Room", fromRooms },
                {"To Room", toRooms }
            };
        }

        /// <summary>
        /// Create a new Line-Based Family Instance given a line, the family, and the view to place the new family.
        /// </summary>
        /// <param name="Curve">The Line to create the Symbol.</param>
        /// <param name="FamilyName">The family name of the Line-based family.</param>
        /// <param name="FamilyType">The family type name of the Line-based family.</param>
        /// <param name="DestinationView">The view to create the family instance.</param>
        /// <returns>The Family Instances.</returns>
        /// <search>line, based, sweep, create, family, curve, line, edge</search>
        [NodeName("FamilyInstanceByCurve")]
        [NodeCategory("Create")]
        public static List<Autodesk.Revit.DB.FamilyInstance> ByCurve(List<Autodesk.DesignScript.Geometry.Curve> Curve, string FamilyName, string FamilyType, Revit.Elements.Views.FloorPlanView DestinationView)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            List<Autodesk.Revit.DB.FamilyInstance> famColl = new List<Autodesk.Revit.DB.FamilyInstance>();

            ElementId viewId = Spring.Elements.UnwrapElement(DestinationView);
            View finalView = doc.GetElement(viewId) as View;
            FamilySymbol symbol = Elements.GetSymbol(doc, FamilyName, FamilyType);
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
    }
}
