using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revit.GeometryConversion;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Autodesk.DesignScript.Runtime;

namespace Spring
{
    public static class Link
    {
        /// <summary>
        /// Gathers coordinate systems of Revit Link Instances.
        /// </summary>
        /// <param name="LinkInstance">The Revit Link Instance.</param>
        /// <returns>Coordinate systems.</returns>
        /// <search>coordinate, system, project, location, x, y, z</search>
        [NodeCategory("Query")]
        public static List<CoordinateSystem> CoordinateSystem(Revit.Elements.Element[] LinkInstance)
        {
            List<CoordinateSystem> coordinates = new List<CoordinateSystem>();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            foreach (Revit.Elements.Element i in LinkInstance)
            {
                ElementId elementId = Spring.Elements.UnwrapElement(i);
                RevitLinkInstance elem = doc.GetElement(elementId) as RevitLinkInstance;
                Autodesk.Revit.DB.Document linkDoc = elem.GetLinkDocument();
                Transform transform = elem.GetTransform();
                Autodesk.DesignScript.Geometry.Point point = transform.Origin.ToPoint(true);
                Vector bx = transform.BasisX.ToVector();
                Vector by = transform.BasisY.ToVector();
                Vector bz = transform.BasisZ.ToVector();
                CoordinateSystem x = Autodesk.DesignScript.Geometry.CoordinateSystem.ByOriginVectors(point,bx,by,bz);
                coordinates.Add(x);
            }
            return coordinates;
        }

        /// <summary>
        /// Gathers the links associated with the current document.
        /// </summary>
        /// <returns name="Document">The Revit Link.</returns>
        /// <returns name="Name">The name of the Revit Link.</returns>
        /// <returns name="ID">The Element ID of the Revit Link.</returns>
        /// <returns name="Type">The Type of the Revit Link.</returns>
        /// <search>document, current, link, revit, id, get, all, collect</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "Document", "Name", "ID", "Type" })]
        public static Dictionary<string, object> CollectRevitLinks()
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

    }
}
