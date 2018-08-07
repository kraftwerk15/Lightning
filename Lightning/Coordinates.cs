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
    public static class Coordinates
    {

        [NodeCategory("Query")]
        [MultiReturn(new[] { "Model Elements", "Count" })]
        public static Dictionary<string,object> GetProjectBasePoint()
        {
            double X =0;
            double Y=0;
            double Elev=0;
            XYZ projectBasePoint = new XYZ();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            ElementCategoryFilter siteCategoryfilter = new ElementCategoryFilter(BuiltInCategory.OST_ProjectBasePoint);

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> siteElements = collector.WherePasses(siteCategoryfilter).ToElements();

            foreach (Element ele in siteElements)
            {
                Parameter paramX = ele.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM);
                X = paramX.AsDouble();

                Parameter paramY = ele.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM);
                Y = paramY.AsDouble();

                Parameter paramElev = ele.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM);
                Elev = paramElev.AsDouble();

                projectBasePoint = new XYZ(X, Y, Elev);
            }
            return new Dictionary<string, object>
            {
                {"X", X },
                {"Y", Y },
                {"Elevation", Elev },
                {"Project Base Point", projectBasePoint }
            };
        }
    }
}
